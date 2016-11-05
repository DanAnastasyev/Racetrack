using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Racetrack.GameServer.Models;

namespace Racetrack.GameServer.AI {
	public class Strategy {
		public readonly WorldModel WorldModel;
		private List<List<int>> distancesToFinish = new List<List<int>>();
		private static readonly int MaxPathLength = int.MaxValue;

		private void AddFinishLine(Queue<Coordinates> bfsQueue) {
			Line finishLine = WorldModel.GetFinishLine();

			double dx = finishLine.First.X - finishLine.Second.X;
			double dy = finishLine.First.Y - finishLine.Second.Y;

			int minX = Math.Min(finishLine.First.X, finishLine.Second.X);
			int minY = Math.Min(finishLine.First.Y, finishLine.Second.Y);
			int maxX = Math.Max(finishLine.First.X, finishLine.Second.X);
			int maxY = Math.Max(finishLine.First.Y, finishLine.Second.Y);

			double deltaX = (dx > dy) ? 1 : (dx / dy);
			double deltaY = (dx > dy) ? (dy / dx) : 1;

			for (double i = finishLine.First.X; i >= minX && i <= maxX; i += deltaX) {
				for (double j = finishLine.First.Y; j >= minY && j <= maxY; j += deltaY) {
					for (int di = -1; di <= 1; ++di) {
						for (int dj = -1; dj <= 1; ++dj) {
							if (WorldModel.IsEmptyPosition((int) (i + di), (int) (j + dj))
									&& WorldModel.IsOnRightSideOfFinishLine((int) (i + di), (int) (j + dj))) {
								bfsQueue.Enqueue(new Coordinates((int) (i + di), (int) (j + dj)));
							}
						}
					}
					distancesToFinish[(int) j][(int) i] = 0;

					if (Math.Abs(deltaY) < 0.0001) {
						break;
					}
				}
				if (Math.Abs(deltaX) < 0.0001) {
					break;
				}
			}
		}

		private void CalculateDistancesToFinish() {
			distancesToFinish.AddRange(Enumerable.Repeat(new List<int>(), WorldModel.Height));
			distancesToFinish.ForEach(list => list.AddRange(Enumerable.Repeat(-1, WorldModel.Width)));

			var bfsQueue = new Queue<Coordinates>();
			AddFinishLine(bfsQueue);
			var finishLineNumber = WorldModel.WayPointsCount() - 1;

			while (bfsQueue.Count != 0) {
				var curNode = bfsQueue.Dequeue();

				if (distancesToFinish[curNode.Y][curNode.X] != -1) {
					continue;
				}
				// Ищем минимальное значение расстояния до финиша среди соседей
				int minDistance = MaxPathLength; 
				for (int i = -1; i <= 1; ++i) {
					for (int j = -1; j <= 1; ++j) {
						if (!WorldModel.IsEmptyPosition(curNode.X + j, curNode.Y + i)) {
							continue;
						}
						// Если на клетку можно встать, её еще не посетили 
						//  и она не находится с другой стороны от финиша - идем в неё
						var movementLine = new Line(new Coordinates(curNode.Y, curNode.X),
							new Coordinates(curNode.Y + j, curNode.X + i));
						var reverseMovementLine = new Line(new Coordinates(curNode.Y + j, curNode.X + i),
							new Coordinates(curNode.Y, curNode.X));
						if (!WorldModel.FindIntersectedWayPoints(movementLine).Contains(finishLineNumber)
						    && distancesToFinish[curNode.Y + j][curNode.X + i] == -1) {
							bfsQueue.Enqueue(new Coordinates(curNode.X + j, curNode.Y + i));
						} else if (!WorldModel.FindIntersectedWayPoints(reverseMovementLine).Contains(finishLineNumber)
						           && distancesToFinish[curNode.Y + j][curNode.X + i] != -1) {
							// Уже посещенная вершина
							minDistance = Math.Min(distancesToFinish[curNode.Y + j][curNode.X + i], minDistance);
						}
					}
				}
				// Мин. расстояние до текущей вершины выражается через мин. расстояние до соседей
				distancesToFinish[curNode.Y][curNode.X] = minDistance + 1;
			}
		}

//		public MoveModel NextMove(List<PlayerModel> players, int playerNumber) {
//			
//		}
	}
}