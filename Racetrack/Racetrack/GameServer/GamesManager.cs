using System;
using System.Collections.Generic;
using System.Linq;

namespace Racetrack.GameServer {
	public class GamesManager {
		private static readonly Lazy<GamesManager> _instance = new Lazy<GamesManager>(() => new GamesManager());
		private readonly List<Game> _games;
		private readonly Dictionary<string, Game> _players;
		private IGamesManagerCallbacks _handler;
		private readonly List<string> _playersQueue;

		public GamesManager() {
			_games = new List<Game>();
			_games.Add(new Game());
			_playersQueue = new List<string>();
			_players = new Dictionary<string, Game>();
		}

		public static GamesManager Instance => _instance.Value;

		public Game GetGame(string playerId) => _games[0];

		public void OnNewPlayerConnection(string playerId, IGamesManagerCallbacks handler) {
			_playersQueue.Add(playerId);
			if (_playersQueue.Count > 1) {
				var joinPlayersList = new List<string>();
				for (var i = 0; i < 2; ++i) {
					joinPlayersList.Add(_playersQueue[i]);
				}
				_playersQueue.RemoveRange(0, 2);

				_games.Add(new Game());
				foreach (var player in joinPlayersList) {
					_players[player] = _games.Last();
				}

				handler.JoinPlayers(joinPlayersList);
			}
		}

		public void SetConnectionCallbacksHandler(IGamesManagerCallbacks handler) {
			_handler = handler;
		}
	}
}