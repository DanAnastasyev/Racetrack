using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Racetrack.GameServer.Models {
	public class WorldModel {
		// координаты точек, через которые должен проехать игрок
		// первая пара - старт, последняя - финиш
		// пересечение старта после прохождения всех waypoint'ов - переход на новый круг
		private List<Line> _wayPoints;

		[JsonProperty("Map")] public List<List<int>> Map;

		public List<Coordinates> PlayersStartPositions;

		public WorldModel(string mapPath) {
			ReadMap(mapPath);
		}

		[JsonProperty("Width")]
		public int Width { get; private set; }

		[JsonProperty("Height")]
		public int Height { get; private set; }

		private void ReadMap(string mapPath) {
			// TODO: errors handling
			using (var reader = new StreamReader(mapPath)) {
				var line = reader.ReadLine();
				if (line == null) {
					throw new ArgumentException();
				}

				var fields = line.Split(' ');
				Height = int.Parse(fields[0]);
				Width = int.Parse(fields[1]);

				Map = new List<List<int>>(Height);
				PlayersStartPositions = new List<Coordinates>();
				for (var i = 0; i < Height; ++i) {
					line = reader.ReadLine();
					if (line == null) {
						break;
					}

					fields = line.Split(' ');
					Map.Add(new List<int>(Width));
					for (var j = 0; j < Width; ++j) {
						var val = int.Parse(fields[j]);
						if (val != 2) {
							Map[i].Add(val);
						} else {
							PlayersStartPositions.Add(new Coordinates(j, i));
							Map[i].Add(0);
						}
					}
				}

				_wayPoints = new List<Line>();
				while (true) {
					line = reader.ReadLine();
					if (line == null) {
						break;
					}

					fields = line.Split(' ');
					_wayPoints.Add(new Line(
						new Coordinates(int.Parse(fields[1]), int.Parse(fields[0])),
						new Coordinates(int.Parse(fields[3]), int.Parse(fields[2]))));
				}
			}
		}

		private static bool IsInBoxOnAxis(int firstPoint, int secondPoint, int thirdPoint, int forthPoint) {
			if (firstPoint > secondPoint) {
				var tmp = firstPoint;
				firstPoint = secondPoint;
				secondPoint = tmp;
			}
			if (thirdPoint > forthPoint) {
				var tmp = thirdPoint;
				thirdPoint = forthPoint;
				forthPoint = tmp;
			}
			return Math.Max(firstPoint, thirdPoint) <= Math.Max(secondPoint, forthPoint);
		}

		private int Area(Coordinates firstPoint, Coordinates secondPoint, Coordinates thirdPoint) {
			return (secondPoint.X - firstPoint.X) * (thirdPoint.Y - firstPoint.Y)
			       - (secondPoint.Y - firstPoint.Y) * (thirdPoint.X - firstPoint.X);
		}

		private bool IsIntersects(Line firstLine, Line secondLine) {
			return IsInBoxOnAxis(firstLine.First.X, firstLine.Second.X, secondLine.First.X, secondLine.Second.X)
			       && IsInBoxOnAxis(firstLine.First.Y, firstLine.Second.Y, secondLine.First.Y, secondLine.Second.Y)
			       && (Area(firstLine.First, firstLine.Second, secondLine.First)
			           * Area(firstLine.First, firstLine.Second, secondLine.Second) <= 0)
			       && (Area(secondLine.First, secondLine.Second, firstLine.First)
			           * Area(secondLine.First, secondLine.Second, firstLine.Second) <= 0);
		}

		// Пересекается ли линия secondLine в нужную сторону
		// Предполагается, что переданы две пересекающиеся линии
		public bool IsProperIntersection(Line firstLine, Line secondLine) {
			var normalVectorCoordinates = new Coordinates(secondLine.Second.X - secondLine.First.X,
				secondLine.Second.Y - secondLine.First.Y);
			if ((Area(secondLine.First, firstLine.Second, secondLine.Second)
			     * Area(secondLine.First, normalVectorCoordinates, secondLine.Second) >= 0)
			    && (Area(secondLine.First, firstLine.First, secondLine.Second)
			        * Area(secondLine.First, normalVectorCoordinates, secondLine.Second) < 0)) {
				return true;
			}
			return false;
		}

		public bool IsValidPosition(int x, int y) {
			return 0 <= y && y < Map.Count
			       && 0 <= x && x < Map[0].Count;
		}

		public bool IsEmptyPosition(int x, int y) {
			return IsValidPosition(x, y) && Map[y][x] != 1;
		}

		public bool IsMovementOutOfTrack(Line movement) {
			// Закончилось ли движение за пределами трека
			if ((movement.Second.X < 0) ||
			    (movement.Second.Y < 0) ||
			    (movement.Second.X >= Width) ||
			    (movement.Second.Y >= Height)) {
				return true;
			}

			// Было ли пересечение границ трека
			var playersPreviousCoordinates = movement.First;
			var playersCoordinates = movement.Second;

			var minX = Math.Min(playersPreviousCoordinates.X, playersCoordinates.X);
			var maxX = Math.Max(playersPreviousCoordinates.X, playersCoordinates.X);
			var minY = Math.Min(playersPreviousCoordinates.Y, playersCoordinates.Y);
			var maxY = Math.Max(playersPreviousCoordinates.Y, playersCoordinates.Y);

			var realCoordinates = new Coordinates(playersCoordinates.X * 10 + 5, playersCoordinates.Y * 10 + 5);
			var realPreviousCoordinates = new Coordinates(playersPreviousCoordinates.X * 10 + 5,
				playersPreviousCoordinates.Y * 10 + 5);

			for (var i = minX; i <= maxX; ++i) {
				for (var j = minY; j <= maxY; ++j) {
					if (IsEmptyPosition(i, j)) {
						continue;
					}
					var firstPoint = new Coordinates(i * 10, j * 10);
					var secondPoint = new Coordinates((i + 1) * 10, j * 10);
					var thirdPoint = new Coordinates((i + 1) * 10, (j + 1) * 10);
					var fourthPoint = new Coordinates(i * 10, (j + 1) * 10);

					if (IsIntersects(new Line(realPreviousCoordinates, realCoordinates), 
										new Line(firstPoint, secondPoint))
							|| IsIntersects(new Line(realPreviousCoordinates, realCoordinates), 
											new Line(secondPoint, thirdPoint))
							|| IsIntersects(new Line(realPreviousCoordinates, realCoordinates), 
											new Line(thirdPoint, fourthPoint))
							|| IsIntersects(new Line(realPreviousCoordinates, realCoordinates), 
											new Line(fourthPoint, firstPoint))) {
						return true;
					}
				}
			}
			return false;
		}

		public bool IsOnRightSideOfFinishLine(int x, int y) {
			var finishLine = _wayPoints.Last();
			return Area(finishLine.First, finishLine.Second, new Coordinates(x, y)) > 0;
		}

		// Возвращает список пересеченных waypoint'ов
		public List<int> FindIntersectedWayPoints(Line movement) {
			var intersectedWayPoints = new List<int>();
			for (var i = 0; i < _wayPoints.Count; ++i) {
				if (IsIntersects(movement, _wayPoints[i])) {
					intersectedWayPoints.Add(i);
				}
			}
			return intersectedWayPoints;
		}

		public int WayPointsCount() => _wayPoints.Count;

		public Line GetFinishLine() {
			return _wayPoints.Last();
		}
	}
}