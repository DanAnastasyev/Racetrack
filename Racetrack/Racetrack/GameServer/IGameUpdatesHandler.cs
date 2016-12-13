using System.Collections.Generic;

namespace Racetrack.GameServer {
	// Коллбеки в GameHub для обновления состояния игры
	public interface IGameUpdatesHandler {
		void OnCarCrash(string playerId);
		void OnUpdateRound(string playerId);
		void OnShowMovements(string playerId, List<bool> scope);
		void OnEndOfGame(string playerId, bool isWinne);
		void OnDeletePlayer(string playerId);
	}
}