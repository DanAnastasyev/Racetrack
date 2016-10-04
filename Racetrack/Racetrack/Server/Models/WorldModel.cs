using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Racetrack.Server.Models {
	public class WorldModel {
		[JsonProperty("Width")]
		public int Width { get; private set; }
		[JsonProperty("Height")]
		public int Height { get; private set; }
		[JsonProperty("Map")]
		public List<List<int>> Map;
		// координаты точек, через которые должен проехать игрок
		private List<Tuple<Coordinates, Coordinates>> _wayPoints;

		private void ReadMap(string mapPath) {
			// TODO: errors handling
			using (var reader = new System.IO.StreamReader(mapPath)) {
				var line = reader.ReadLine();
				if (line == null) {
					throw new ArgumentException();
				}

				var fields = line.Split(' ');
				Height = int.Parse(fields[0]);
				Width = int.Parse(fields[1]);

				Map = new List<List<int>>(Height);
				for (var i = 0; i < Height; ++i) {
					line = reader.ReadLine();
					if (line == null) {
						break;
					}

					fields = line.Split(' ');
					Map.Add(new List<int>(Width));
					for (var j = 0; j < Width; ++j) {
						Map[i].Add(int.Parse(fields[j]));
					}
				}

				_wayPoints = new List<Tuple<Coordinates, Coordinates>>();
				while (true) {
					line = reader.ReadLine();
					if (line == null) {
						break;
					}

					fields = line.Split(' ');
					_wayPoints.Add(new Tuple<Coordinates, Coordinates>(
						new Coordinates(int.Parse(fields[0]), int.Parse(fields[1])),
						new Coordinates(int.Parse(fields[2]), int.Parse(fields[3]))));
				}
			}
		}

		public WorldModel(string mapPath) {
			ReadMap(mapPath);
		}

		// Можно ли стоять на позиции coord
		public bool CheckPosition(Coordinates coord) {
			if (!(coord.X >= 0 && coord.Y <= Width
				&& coord.Y >= 0 && coord.Y <= Height)) {
				return false;
			}
			return Map[coord.X][coord.Y] != 1;
		}
	}
}