using System;
using System.Collections.Generic;

namespace Racetrack.Server.Models {
	public class WorldModel {
		public int Width { get; private set; }
		public int Height { get; private set; }
		private List<List<int>> _map;
		// координаты точек, через которые должен проехать игрок
		private List<Tuple<Coordinates, Coordinates>> _wayPoints;

		public WorldModel(string mapPath) {
			ReadMap(mapPath);
		}

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

				_map = new List<List<int>>(Height);

				for (var i = 0; i < Height; ++i) {
					line = reader.ReadLine();
					if (line == null) {
						break;
					}

					fields = line.Split(' ');
					_map.Add(new List<int>(Width));
					for (var j = 0; j < Width; ++j) {
						_map[i].Add(int.Parse(fields[j]));
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
	}
}