using System;
using System.Collections.Generic;
using System.Linq;
using Racetrack.GameServer.Models;

namespace Racetrack.GameServer {
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
			_world = new WorldModel(System.Web.HttpContext.Current.Server.MapPath("~/GameServer/Resources/map.txt"));
		}

		public void AddPlayer(string playerId) {
			if (_players.ContainsKey(playerId)) {
				return;
			}
			_players.Add(playerId, new PlayerModel(GetNextAvailablePlayerPosition()));
			_world.Map[_players[playerId].CurPosition.Y][_players[playerId].CurPosition.X] = 2;
		}

		public void DeletePlayer(string playerId, IGameUpdatesHandler handler) {
			if (!_players.ContainsKey(playerId)) {
				// TODO: handle error
				return;
			}
			_world.Map[_players[playerId].CurPosition.Y][_players[playerId].CurPosition.X] = 0;
			_players.Remove(playerId);
			handler.DeletePlayer(playerId);
		}

		private void PerformMovement(PlayerModel player, string playerId, 
				MoveModel move, IGameUpdatesHandler handler) {
			player.Move(move);
			++_movesCount;
			if (!_world.IsMovementOutOfTrack(player.GetLastMovement())) {
				handler.ShowMovements(playerId);
			} else {
				handler.ShowMovements(playerId);
				handler.CrashCar(playerId);
				DeletePlayer(playerId, handler);
			}
		}

		private void CheckIntersections(PlayerModel player, string playerId, IGameUpdatesHandler handler) {
			var intersectedWayPoint = _world.FindIntersectedWayPoints(player.GetLastMovement());
			foreach (var pointNumber in intersectedWayPoint) {
				if (pointNumber == player.LastWayPoint + 1) {
					player.LastWayPoint = pointNumber;
				}
			}
			// Конец игры == игрок пересек последний вэйпоинт
			if (player.LastWayPoint == _world.WayPointsCount() - 1) {
				handler.ShowEndOfGame(playerId);
			}
		}

		private void UpdateRound(string playerId, IGameUpdatesHandler handler) {
			if (_movesCount != _players.Count) {
				return;
			}
			handler.UpdateRound(playerId);
			_movesCount = 0;
			++RoundNumber;
		}

		public void UpdatePlayer(string playerId, MoveModel move, IGameUpdatesHandler handler) {
			if (!_players.ContainsKey(playerId)) {
				// TODO: Handling error?
				return;
			}
			var player = _players[playerId];
			PerformMovement(player, playerId, move, handler);
			CheckIntersections(player, playerId, handler);

			UpdateRound(playerId, handler);
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

		private Coordinates GetNextAvailablePlayerPosition() {
			return _world.PlayersStartPositions[_players.Count];
		}
	}
}