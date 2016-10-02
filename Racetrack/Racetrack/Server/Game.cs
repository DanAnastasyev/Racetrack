using System;
using System.Collections.Generic;
using Racetrack.Server.Models;

namespace Racetrack.Server {
	// Инстанс игры
	public class Game {
		private static readonly Lazy<Game> _instance = new Lazy<Game>(() => new Game());
		private readonly Dictionary<string, PlayerModel> _players;
		private int _movesCount; // число игроков, походивших в данном раунде

		public int RoundNumber { get; private set; } // номер текущего раунда

		public static Game Instance => _instance.Value;

		public Game() {
			_players = new Dictionary<string, PlayerModel>();
			_movesCount = 0;
			RoundNumber = 0;
		}

		public void AddPlayer(string playerId) {
			if (_players.ContainsKey(playerId)) {
				return;
			}
			_players.Add(playerId, new PlayerModel());
		}

		public void DeletePlayer(string playerId) {
			_players.Remove(playerId);
		}

		public void UpdatePlayer(string playerId, MoveModel move) {
			if (!_players.ContainsKey(playerId)) {
				// TODO: Handling error?
				return;
			}
			_players[playerId].Update(move);
			++_movesCount;
		}

		public PlayerModel GetPlayer(string playerId) {
			if (!_players.ContainsKey(playerId)) {
				// TODO: Handling error?
				return null;
			}
			return _players[playerId];
		}

		public bool IsEndOfRound() {
			return _movesCount == _players.Count;
		}

		public void BeginNextRound() {
			_movesCount = 0;
			++RoundNumber;
		}
	}
}