using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace Racetrack.Server.Models {
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
}