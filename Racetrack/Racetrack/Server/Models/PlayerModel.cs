using Newtonsoft.Json;

namespace Racetrack.Server.Models {
	public class PlayerModel {
		// Результат движения игрока
		public class PlayerMovementModel {
			[JsonProperty("XOld")]
			public int XOld { get; }
			[JsonProperty("YOld")]
			public int YOld { get; }
			[JsonProperty("XNew")]
			public int XNew { get; }
			[JsonProperty("YNew")]
			public int YNew { get; }

			public PlayerMovementModel(PlayerModel player, MoveModel move) {
				XNew = player.XPos;
				YNew = player.YPos;
				XOld = XNew - move.GetDeltaX();
				YOld = YNew - move.GetDeltaY();
			}
		}

		public int XPos { get; set; }
		public int YPos { get; set; }

		public PlayerMovementModel Update(MoveModel move) {
			XPos += move.GetDeltaX();
			YPos += move.GetDeltaY();

			return new PlayerMovementModel(this, move);
		}
	}
}