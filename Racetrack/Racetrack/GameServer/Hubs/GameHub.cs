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
			game.AddPlayer(playerId, Context.User.Identity.Name);

			await Groups.Add(Context.ConnectionId, gameId);
			Clients.Caller.showMap(game.GetWorldModel(), playerId, game.PlayerIds);
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

		#region IGameUpdatesHandler

		public void OnCarCrash(string playerId) {
			Clients.Caller.onCrash();
		}

		public void OnUpdateRound(string playerId) {
			var gameId = _gameManager.GetUserGroup(playerId);
			Clients.Group(gameId).beginNextRound();
		}

		public void OnShowMovements(string playerId) {
			var gameId = _gameManager.GetUserGroup(playerId);
			var game = _gameManager.GetGame(Context.User.Identity.GetUserId());
			var player = game.GetPlayer(playerId);

			Clients.Group(gameId).showMovements(game.RoundNumber, playerId,
				player?.PrevPosition, player?.CurPosition);
		}

		public void OnEndOfGame(string playerId, bool isWinner) {
			var gameId = _gameManager.GetUserGroup(playerId);
			if (isWinner) {
				Clients.Client(Context.ConnectionId).showEndOfGame(true);
				Clients.Group(gameId, Context.ConnectionId).showEndOfGame(false);
			} else {
				Clients.Group(gameId).showEndOfGame(false);
			}
		}

		public void OnDeletePlayer(string playerId) {
			var gameId = _gameManager.GetUserGroup(playerId);
			var game = _gameManager.GetGame(Context.User.Identity.GetUserId());
			Clients.Group(gameId, playerId).showMap(game.GetWorldModel(), playerId);
		}

		#endregion
	}
}