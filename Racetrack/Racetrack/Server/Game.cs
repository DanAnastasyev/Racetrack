using System;
using System.Collections.Generic;
using Racetrack.Server.Hubs;
using Racetrack.Server.Models;

namespace Racetrack.Server {
	// Инстанс игры
	public class Game {
		private static readonly Lazy<Game> _instance = new Lazy<Game>(() => new Game());
		private readonly Dictionary<string, PlayerModel> _players;
		private int _movesCount; // число игроков, походивших в данном раунде
		private readonly WorldModel _world;

		public int RoundNumber { get; private set; } // номер текущего раунда

		public static Game Instance => _instance.Value;

		public Game() {
			_players = new Dictionary<string, PlayerModel>();
			_world = new WorldModel(System.Web.HttpContext.Current.Server.MapPath("~/Resources/map.txt"));
		}

		public void AddPlayer(string playerId) {
			if (_players.ContainsKey(playerId)) {
				return;
			}
			_players.Add(playerId, new PlayerModel(new Coordinates(16, 2)));
		}

		public void DeletePlayer(string playerId) {
			_players.Remove(playerId);
		}

		public void UpdatePlayer(string playerId, MoveModel move, IGameUpdatesHandler handler) {
			if (!_players.ContainsKey(playerId)) {
				// TODO: Handling error?
				return;
			}
			PlayerModel player = _players[playerId];
			player.Move(move);
			++_movesCount;
			if (!_world.IsMovementOutOfTrack(player.GetLastMovement())) {
				handler.ShowMovements(playerId);
			} else {
				player.IsAlive = false;
				handler.CrashCar(playerId);
			}

			var intersectedWayPoint = _world.FindIntersectedWayPoints(player.GetLastMovement());
			foreach (var pointNumber in intersectedWayPoint) {
				if (pointNumber > player.LastWayPoint
				    || player.LastWayPoint == _world.WayPointsCount() - 1 && pointNumber == 0) {
					player.LastWayPoint = pointNumber;
				}
			}

			if (_movesCount == _players.Count) {
				handler.UpdateRound(playerId);
				_movesCount = 0;
				++RoundNumber;
			}
		}

		public PlayerModel GetPlayer(string playerId) {
			if (!_players.ContainsKey(playerId)) {
				// TODO: Handling error?
				return null;
			}
			return _players[playerId];
		}
		
		public WorldModel GetWorldModel() {
			return _world;
		}
	}
}