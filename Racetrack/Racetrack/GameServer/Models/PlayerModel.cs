using System;

namespace Racetrack.GameServer.Models {
	public class PlayerModel : IComparable<PlayerModel>, ICloneable {
		public PlayerModel(Coordinates initialPosition, string playerName) {
			CurPosition = initialPosition;
			PrevPosition = initialPosition;
			Inertia = new Coordinates(0, 0);
			IsAlive = true;
			IsWinner = false;
			LastWayPoint = -1; // Инициализируем -1, чтобы нужно было проехать по всем точкам
			PlayerName = playerName;
		}

		public Coordinates CurPosition { get; }
		public Coordinates PrevPosition { get; private set; }
		public Coordinates Inertia { get; private set; }
		public int CurLap { get; set; }
		public bool IsAlive { get; set; }
		public bool IsWinner { get; set; }
		public int LastWayPoint { get; set; }
		public int NumberOfMovements { get; private set; }
		public string PlayerName { get; set; }

		public void Move(MoveModel move) {
			Inertia = new Coordinates(Inertia.X + move.GetDeltaX(), Inertia.Y + move.GetDeltaY());
			PrevPosition = (Coordinates) CurPosition.Clone();
			CurPosition.MoveBy(Inertia);
			++NumberOfMovements;
		}

		public PlayerModel GetMovementResult(int dx, int dy) {
			var result = (PlayerModel) Clone();
			result.Inertia.X += dx;
			result.Inertia.Y += dy;
			result.PrevPosition = (Coordinates) result.CurPosition.Clone();
			result.CurPosition.MoveBy(Inertia);
			++result.NumberOfMovements;
			return result;
		}

		public Line GetLastMovement() {
			return new Line(PrevPosition, CurPosition);
		}

		public int CompareTo(PlayerModel other) {
			if (IsWinner || other.IsWinner) {
				if (IsWinner && other.IsWinner) {
					// Если оба победители - выше тот,
					//  который сделал меньше ходов
					return NumberOfMovements - other.NumberOfMovements;
				}
				return IsWinner ? -1 : 1;
			}
			if (!IsAlive || !other.IsAlive) {
				if (!IsAlive && !other.IsAlive) {
					// Если оба врезались - выше тот,
					//  кто проехал дальше ~ сделал больше ходов
					return other.NumberOfMovements - NumberOfMovements;
				}
				return IsAlive ? -1 : 1;
			}
			// Если оба не доехали до финиша и не слетели с трассы -
			//  сортируем лексикографически
			return string.Compare(PlayerName, other.PlayerName, StringComparison.Ordinal);
		}

		public object Clone() {
			var result = new PlayerModel((Coordinates) CurPosition.Clone(), PlayerName) {
				PrevPosition = (Coordinates) PrevPosition.Clone(),
				Inertia = (Coordinates) Inertia.Clone(),
				CurLap = CurLap,
				IsAlive = IsAlive,
				IsWinner = IsWinner,
				LastWayPoint = LastWayPoint,
				NumberOfMovements = NumberOfMovements
			};
			return result;
		}
	}
}