﻿using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Racetrack.GameServer.Models;

namespace Racetrack.GameServer.Hubs {
	public class GameHub : Hub, IGameUpdatesHandler {
		private readonly GamesManager _gameManager;

		public GameHub() : this(GamesManager.Instance) {}

		public GameHub(GamesManager gameManager) {
			_gameManager = gameManager;
		}

		public void UpdatePlayer(MoveModel move) {
			var game = _gameManager.GetGame(Context.ConnectionId);
			game.UpdatePlayer(Context.ConnectionId, move, this);
		}

		#region Connection lifecycle

		public override Task OnConnected() {
			var playerId = Context.ConnectionId;
			var game = _gameManager.GetGame(playerId);
			game.AddPlayer(playerId);
			Clients.All.showMap(game.GetWorldModel());
			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled) {
			var playerId = Context.ConnectionId;
			var game = _gameManager.GetGame(playerId);
			game.DeletePlayer(playerId, this);
			return base.OnDisconnected(stopCalled);
		}

		#endregion

		#region GameUpdatesHandler

		public void CrashCar(string playerId) {
			var game = _gameManager.GetGame(Context.ConnectionId);
			Clients.Caller.crashMyCar(game.RoundNumber, playerId);
			Clients.Others.crashOtherCar(game.RoundNumber, playerId);
		}

		public void UpdateRound(string playerId) {
			Clients.All.beginNextRound();
		}

		public void ShowMovements(string playerId) {
			var game = _gameManager.GetGame(Context.ConnectionId);
			var player = game.GetPlayer(playerId);

			Clients.All.showMovements(game.RoundNumber, playerId,
				player?.PrevPosition, player?.CurPosition);
		}

		public void ShowEndOfGame(string playerId) {
			Clients.Caller.showEndOfGame(true);
			Clients.Others.showEndOfGame(false);
		}

		public void DeletePlayer(string playerId) {
			var game = _gameManager.GetGame(Context.ConnectionId);
			Clients.Others.showMap(game.GetWorldModel());
		}

		#endregion
	}
}