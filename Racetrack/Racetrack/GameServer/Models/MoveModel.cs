using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Racetrack.GameServer.Models {
	// Модель изменения положения игрока:
	//  игрок мог подвинуться в девяти направлениях, 
	//  соответствующих девяти клавишам на нампаде
	public class MoveModel {
		[JsonProperty("key")]
		public int MoveType { get; set; }

		public static readonly Dictionary<int, Tuple<int, int>> MoveTypeToDelta =
			new Dictionary<int, Tuple<int, int>> {
				{1, new Tuple<int, int>(-1, 1)},
				{2, new Tuple<int, int>(0, 1)},
				{3, new Tuple<int, int>(1, 1)},
				{4, new Tuple<int, int>(-1, 0)},
				{5, new Tuple<int, int>(0, 0)},
				{6, new Tuple<int, int>(1, 0)},
				{7, new Tuple<int, int>(-1, -1)},
				{8, new Tuple<int, int>(0, -1)},
				{9, new Tuple<int, int>(1, -1)}
			};

		// Маппинг клавиш на перемещение по горизонтали
		public int GetDeltaY() {
			return MoveTypeToDelta[MoveType].Item2;
		}

		// Маппинг клавиш на перемещение по вертикали
		public int GetDeltaX() {
			return MoveTypeToDelta[MoveType].Item1;
		}

		public MoveModel(int dx, int dy) {
			MoveType = MoveTypeToDelta.FirstOrDefault(x => ( x.Value.Item1 == dx && x.Value.Item2 == dy )).Key;
		}
	}
}