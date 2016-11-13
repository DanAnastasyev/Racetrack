using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Racetrack.Models {
	public class GameSettingsModel {
		public int Id { get; set; }
		public string Map { get; set; }
		public string WithAI { get; set; }
	}
}