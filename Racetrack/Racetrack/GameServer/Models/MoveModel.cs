﻿using System;
using Newtonsoft.Json;

namespace Racetrack.GameServer.Models {
	// Модель изменения положения игрока:
	//  игрок мог подвинуться в девяти направлениях, 
	//  соответствующих девяти клавишам на нампаде
	public class MoveModel {
		[JsonProperty("key")]
		public int MoveType { get; set; }

		// Маппинг клавиш на перемещение по горизонтали
		public int GetDeltaY() {
			if ((MoveType >= 7) && (MoveType <= 9)) {
				return -1;
			}
			if ((MoveType >= 4) && (MoveType <= 6)) {
				return 0;
			}
			if ((MoveType >= 1) && (MoveType <= 3)) {
				return 1;
			}
			throw new ArgumentOutOfRangeException();
		}

		// Маппинг клавиш на перемещение по вертикали
		public int GetDeltaX() {
			if ((MoveType == 1) || (MoveType == 4) || (MoveType == 7)) {
				return -1;
			}
			if ((MoveType == 2) || (MoveType == 5) || (MoveType == 8)) {
				return 0;
			}
			if ((MoveType == 3) || (MoveType == 6) || (MoveType == 9)) {
				return 1;
			}
			throw new ArgumentOutOfRangeException();
		}
	}
}