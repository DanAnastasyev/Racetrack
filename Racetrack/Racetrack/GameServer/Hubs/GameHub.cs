using System.Threading.Tasks;
using Microsoft.AspNet.Identity;
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
			var playerId = Context.User.Identity.GetUserId();
			var game = _gameManager.GetGame(playerId);
			game.UpdatePlayer(playerId, move, this);
		}

		#region Connection lifecycle

		public override async Task OnConnected() {
			var playerId = Context.User.Identity.GetUserId();
			var gameId = _gameManager.GetUserGroup(playerId);

			var game = _gameManager.GetGame(playerId);
			game.AddPlayer(playerId);

			await Groups.Add(Context.ConnectionId, gameId);
			Clients.Group(gameId).showMap(game.GetWorldModel());
			await base.OnConnected();
		}

		public override async Task OnDisconnected(bool stopCalled) {
			var playerId = Context.User.Identity.GetUserId();
			var game = _gameManager.GetGame(playerId);
			game.DeletePlayer(playerId, this);

			await Groups.Remove(Context.ConnectionId, _gameManager.GetUserGroup(playerId));
			await base.OnDisconnected(stopCalled);
		}

		#endregion

		#region GameUpdatesHandler

		public void CrashCar(string playerId) {
			var gameId = _gameManager.GetUserGroup(playerId);
			var game = _gameManager.GetGame(Context.User.Identity.GetUserId());
			Clients.Caller.crashMyCar(game.RoundNumber, playerId);
			Clients.Group(gameId, playerId).crashOtherCar(game.RoundNumber, playerId);
		}

		public void UpdateRound(string playerId) {
			var gameId = _gameManager.GetUserGroup(playerId);
			Clients.Group(gameId).beginNextRound();
		}

		public void ShowMovements(string playerId) {
			var gameId = _gameManager.GetUserGroup(playerId);
			var game = _gameManager.GetGame(Context.User.Identity.GetUserId());
			var player = game.GetPlayer(playerId);

			Clients.Group(gameId).showMovements(game.RoundNumber, playerId,
				player?.PrevPosition, player?.CurPosition);
		}

		public void ShowEndOfGame(string playerId) {
			var gameId = _gameManager.GetUserGroup(playerId);
			Clients.Caller.showEndOfGame(true);
			Clients.Group(gameId, playerId).showEndOfGame(false);
		}

		public void DeletePlayer(string playerId) {
			var gameId = _gameManager.GetUserGroup(playerId);
			var game = _gameManager.GetGame(Context.User.Identity.GetUserId());
			Clients.Group(gameId, playerId).showMap(game.GetWorldModel());
		}

		#endregion
	}
}