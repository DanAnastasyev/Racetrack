using System;
using Newtonsoft.Json;

namespace Racetrack.GameServer.Models {
	public class Coordinates : ICloneable {
		[JsonProperty("X")]
		public int X { get; set; }
		[JsonProperty("Y")]
		public int Y { get; set; }

		public Coordinates(int x, int y) {
			X = x;
			Y = y;
		}

		public void MoveBy(Coordinates delta) {
			X += delta.X;
			Y += delta.Y;
		}

		public object Clone() {
			return new Coordinates(X, Y);
		}
	}

	public class Line : ICloneable {
		public Coordinates First { get; private set; }
		public Coordinates Second { get; private set; }

		public Line(Coordinates first, Coordinates second) {
			First = first;
			Second = second;
		}

		public object Clone() {
			return new Line(First, Second);
		}
	}
}