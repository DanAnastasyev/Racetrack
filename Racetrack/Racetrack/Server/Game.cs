using System;
using System.Collections.Generic;
using Racetrack.Server.Models;

namespace Racetrack.Server {
	public class Game {
		private static readonly Lazy<Game> _instance = new Lazy<Game>(() => new Game());
		private readonly Dictionary<string, PlayerModel> _players;

		public Game() {
			_players = new Dictionary<string, PlayerModel>();
		}

		public static Game Instance => _instance.Value;

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
				return;
			}
			_players[playerId].Update(move);
		}
	}
}