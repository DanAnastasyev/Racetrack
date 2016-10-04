using Newtonsoft.Json;

namespace Racetrack.Server.Models {
	public class PlayerModel {
		public Coordinates CurPosition { get; private set; }
		public Coordinates PrevPosition { get; private set; }
		public Coordinates Inertia { get; private set; }

		public PlayerModel() : this(new Coordinates(0, 0)) {}

		public PlayerModel(Coordinates initialPosition) {
			CurPosition = initialPosition;
			Inertia = new Coordinates(0, 0);
		}

		public void Move(MoveModel move) {
			Inertia = new Coordinates(Inertia.X + move.GetDeltaX(), Inertia.Y + move.GetDeltaY());
			PrevPosition = (Coordinates) CurPosition.Clone();
			CurPosition.MoveBy(Inertia);
		}
	}
}