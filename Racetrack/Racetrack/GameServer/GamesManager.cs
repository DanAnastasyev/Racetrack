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

		private static readonly Lazy<GamesManager> _instance =
			new Lazy<GamesManager>(() => new GamesManager());
		private readonly List<Game> _games;
		private readonly Dictionary<string, Game> _players;
		// список ожидающих подключения игроков для каждой карты
		private readonly Dictionary<string, List<UserConnection>> _playersQueue;
		private readonly Dictionary<string, string> _playerToMap;

		public GamesManager() {
			_games = new List<Game>();
			_playersQueue = new Dictionary<string, List<UserConnection>>();
			_players = new Dictionary<string, Game>();
			_playerToMap = new Dictionary<string, string>();
		}

		public static GamesManager Instance => _instance.Value;

		public Game GetGame(string userId) => _players[userId];

		public void AddPlayerToWaitingQueue(string userId, string connectionId, 
				IGamesManagerCallbacks handler) {
			var map = _playerToMap[userId];
			_playerToMap.Remove(userId);
			if (!_playersQueue.ContainsKey(map)) {
				_playersQueue[map] = new List<UserConnection>();
			}
			_playersQueue[map].Add(new UserConnection(userId, connectionId));
			if (_playersQueue[map].Count <= 1) {
				return;
			}
			_games.Add(new Game());

			var connectionIds = new List<string>();
			var userIds = new List<string>();
			for (var i = 0; i < 2; ++i) {
				connectionIds.Add(_playersQueue[map][i].ConnectionId);
				userIds.Add( _playersQueue[map][i].UserId );
				_players[_playersQueue[map][i].UserId] = _games.Last();
			}
			_playersQueue[map].RemoveRange(0, 2);

			_games.Last().PlayerIds = userIds;

			handler.JoinPlayers(connectionIds);
		}

        public void BeginSinglePlayerGame(string userId) {
			_games.Add(new Game(true));
			_players[userId] = _games.Last();
		} 

		public void AddAIToGame(Game game, string aiId) {
			_players[aiId] = game;
		}

		public void AddPlayerToMap(string userId, string map) {
			_playerToMap[userId] = map;
		}

		public void DeletePlayer(string userId) {
			_players.Remove(userId);
		}

		public string GetUserGroup(string userId) {
			return "game_" + _games.IndexOf(_players[userId]);
		}
	}
}