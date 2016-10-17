namespace Racetrack.GameServer {
	// Коллбеки в GameHub для обновления состояния игры
	public interface IGameUpdatesHandler {
		void CrashCar(string playerId);
		void UpdateRound(string playerId);
		void ShowMovements(string playerId);
		void ShowEndOfGame(string playerId);
		void DeletePlayer(string playerId);
	}
}
