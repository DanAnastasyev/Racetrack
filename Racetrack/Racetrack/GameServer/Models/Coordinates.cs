using System;
using Newtonsoft.Json;

namespace Racetrack.GameServer.Models {
	public class Coordinates : ICloneable {
		public Coordinates(int x, int y) {
			X = x;
			Y = y;
		}

		[JsonProperty("X")]
		public int X { get; set; }

		[JsonProperty("Y")]
		public int Y { get; set; }

		public object Clone() {
			return new Coordinates(X, Y);
		}

		public void MoveBy(Coordinates delta) {
			X += delta.X;
			Y += delta.Y;
		}
	}

	public class Line : ICloneable {
		public Line(Coordinates first, Coordinates second) {
			First = first;
			Second = second;
		}

		public Coordinates First { get; }
		public Coordinates Second { get; }

		public object Clone() {
			return new Line(First, Second);
		}
	}
}