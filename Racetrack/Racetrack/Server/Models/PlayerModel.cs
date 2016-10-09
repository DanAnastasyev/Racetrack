using Newtonsoft.Json;

namespace Racetrack.Server.Models {
	public class PlayerModel {
		public Coordinates CurPosition { get; private set; }
		public Coordinates PrevPosition { get; private set; }
		public Coordinates Inertia { get; private set; }
		public int CurLap { get; set; }
		public bool IsAlive { get; set; }
		public int LastWayPoint { get; set; }
		
		public PlayerModel(Coordinates initialPosition) {
			CurPosition = initialPosition;
			PrevPosition = initialPosition;
			Inertia = new Coordinates(0, 0);
			IsAlive = true;
		}

		public void Move(MoveModel move) {
			Inertia = new Coordinates(Inertia.X + move.GetDeltaX(), Inertia.Y + move.GetDeltaY());
			PrevPosition = (Coordinates) CurPosition.Clone();
			CurPosition.MoveBy(Inertia);
		}

		public Line GetLastMovement() {
			return new Line(PrevPosition, CurPosition);
		}
	}
}