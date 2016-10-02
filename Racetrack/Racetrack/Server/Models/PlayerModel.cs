namespace Racetrack.Server.Models {
	public class PlayerModel {
		public int XPos { get; set; }
		public int YPos { get; set; }

		public void Update(MoveModel move) {
			XPos += move.GetDeltaX();
			YPos += move.GetDeltaY();
		}
	}
}