using System;
using System.Collections.Generic;
using System.Linq;

namespace Racetrack.GameServer {
	public class GamesManager {
		private class UserConnection {
			public string UserId { get; }
			public string ConnectionId { get; }

			public UserConnection(string userId, string connectionId) {
				UserId = userId;
				ConnectionId = connectionId;
			}
		}

		private static readonly Lazy<GamesManager> _instance = new Lazy<GamesManager>(() => new GamesManager());
		private readonly List<Game> _games;
		private readonly Dictionary<string, Game> _players;
		private readonly List<UserConnection> _playersQueue;

		public GamesManager() {
			_games = new List<Game>();
			_playersQueue = new List<UserConnection>();
			_players = new Dictionary<string, Game>();
		}

		public static GamesManager Instance => _instance.Value;

		public Game GetGame(string userId) => _players[userId];

		public void AddPlayerToWaitingQueue(string userId, string connectionId, IGamesManagerCallbacks handler) {
			_playersQueue.Add(new UserConnection(userId, connectionId));
			if (_playersQueue.Count > 0) {
				_games.Add(new Game());

				var connectionIds = new List<string>();
				for (var i = 0; i < 1; ++i) {
					connectionIds.Add(_playersQueue[i].ConnectionId);
					_players[_playersQueue[i].UserId] = _games.Last();
				}
				_playersQueue.RemoveRange(0, 1);

				handler.JoinPlayers(connectionIds);
			}
		}

		public void AddAIToGame(Game game, string aiId) {
			_players[aiId] = game;
		}

		public string GetUserGroup(string userId) {
			return "game_" + _games.IndexOf(_players[userId]);
		}
	}
}