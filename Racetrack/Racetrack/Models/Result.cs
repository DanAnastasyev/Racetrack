using System.ComponentModel.DataAnnotations.Schema;

namespace Racetrack.Models {
	public class Result {
		public int Id { get; set; }
		public string PlayerName { get; set; }
		public int NumberOfMovements { get; set; }
		//public GameLogModel GameLog { get; set; }

		public Result() {}

		public Result(string playerName, int numberOfMovements) {
			PlayerName = playerName;
			NumberOfMovements = numberOfMovements;
		}
	}
}