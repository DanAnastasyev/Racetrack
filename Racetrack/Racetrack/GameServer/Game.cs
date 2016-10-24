using System;
using System.Collections.Generic;
using System.Web;
using Racetrack.GameServer.Models;

namespace Racetrack.GameServer {
	// Инстанс игры
	public class Game {
		private static readonly Lazy<Game> _instance = new Lazy<Game>(() => new Game());
		private readonly Dictionary<string, PlayerModel> _players;
		private readonly WorldModel _world;
		private int _movesCount; // число игроков, походивших в данном раунде

		public Game() {
			_players = new Dictionary<string, PlayerModel>();
			_world = new WorldModel(HttpContext.Current.Server.MapPath("~/GameServer/Resources/map.txt"));
		}

		public int RoundNumber { get; private set; } // номер текущего раунда

		public static Game Instance => _instance.Value;

		public void AddPlayer(string playerId) {
			if (_players.ContainsKey(playerId)) {
				return;
			}
			_players.Add(playerId, new PlayerModel(GetNextAvailablePlayerPosition()));
			if (_world.IsValidPosition(_players[playerId].CurPosition.X, _players[playerId].CurPosition.Y)) {
				_world.Map[_players[playerId].CurPosition.Y][_players[playerId].CurPosition.X] = 2;
			} else {
				throw new ArgumentException();
			}
		}

		public void DeletePlayer(string playerId, IGameUpdatesHandler handler) {
			if (!_players.ContainsKey(playerId)) {
				return;
			}
			_players.Remove(playerId);
			//handler.OnDeletePlayer(playerId);
		}

		private void PerformMovement(PlayerModel player, string playerId,
			MoveModel move, IGameUpdatesHandler handler) {
			player.Move(move);
			++_movesCount;
			if (!_world.IsMovementOutOfTrack(player.GetLastMovement())) {
				handler.OnShowMovements(playerId);
				CheckWayPointsIntersections(player, playerId, handler);
			} else {
				handler.OnShowMovements(playerId);
				handler.OnCarCrash(playerId);
				DeletePlayer(playerId, handler);
			}
		}

		private void CheckWayPointsIntersections(PlayerModel player, string playerId, IGameUpdatesHandler handler) {
			var intersectedWayPoint = _world.FindIntersectedWayPoints(player.GetLastMovement());
			foreach (var pointNumber in intersectedWayPoint) {
				if (pointNumber == player.LastWayPoint + 1) {
					player.LastWayPoint = pointNumber;
				}
			}
			// Конец игры == игрок пересек последний вэйпоинт
			if (player.LastWayPoint == _world.WayPointsCount() - 1) {
				handler.OnEndOfGame(playerId, true);
			}
		}

		// Конец раунда - это момент, когда все игроки уже походили и нужно им дать возможность снова походить
		private void UpdateRound(string playerId, IGameUpdatesHandler handler) {
			// Если все игроки вылетели с поля
			if (_players.Count == 0) {
				handler.OnEndOfGame(playerId, false);
			}

			if (_movesCount < _players.Count) {
				return;
			}
			handler.OnUpdateRound(playerId);
			_movesCount = 0;
			++RoundNumber;
		}

		public void UpdatePlayer(string playerId, MoveModel move, IGameUpdatesHandler handler) {
			if (!_players.ContainsKey(playerId)) {
				return;
			}
			var player = _players[playerId];
			PerformMovement(player, playerId, move, handler);

			UpdateRound(playerId, handler);
		}

		public bool HasPlayer(string playerId) {
			return _players.ContainsKey(playerId);
		}

		public PlayerModel GetPlayer(string playerId) {
			if (!_players.ContainsKey(playerId)) {
				return null;
			}
			return _players[playerId];
		}

		public WorldModel GetWorldModel() {
			return _world;
		}

		private Coordinates GetNextAvailablePlayerPosition() {
			return _world.PlayersStartPositions[_players.Count];
		}
	}
}