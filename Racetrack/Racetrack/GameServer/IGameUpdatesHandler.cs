namespace Racetrack.Server.Hubs {
	// Коллбеки в GameHub для обновления состояния игры
	public interface IGameUpdatesHandler {
		void CrashCar(string playerId);
		void UpdateRound(string playerId);
		void ShowMovements(string playerId);
	}
}
