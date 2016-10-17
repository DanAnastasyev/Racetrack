using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Racetrack.GameServer.Models;

namespace Racetrack.GameServer.Hubs {
	public class GameHub : Hub, IGameUpdatesHandler {
		private readonly Game _game;

		public GameHub() : this(Game.Instance) { }

		public GameHub(Game game) {
			_game = game;
		}

		public void UpdatePlayer(MoveModel move) {
			_game.UpdatePlayer(Context.ConnectionId, move, this);
		}

		#region Connection lifecycle

		public override Task OnConnected() {
			_game.AddPlayer(Context.ConnectionId);
			Clients.All.showMap(_game.GetWorldModel());
			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled) {
			_game.DeletePlayer(Context.ConnectionId, this);
			return base.OnDisconnected(stopCalled);
		}

		public override Task OnReconnected() {
			// Add your own code here.
			// For example: in a chat application, you might have marked the
			// user as offline after a period of inactivity; in that case 
			// mark the user as online again.
			return base.OnReconnected();
		}

		#endregion

		#region GameUpdatesHandler

		public void CrashCar(string playerId) {
			Clients.Caller.crashMyCar(_game.RoundNumber, playerId);
			Clients.Others.crashOtherCar(_game.RoundNumber, playerId);
		}

		public void UpdateRound(string playerId) {
			Clients.All.beginNextRound();
		}

		public void ShowMovements(string playerId) {
			var player = _game.GetPlayer(playerId);

			Clients.All.showMovements(_game.RoundNumber, playerId,
				player?.PrevPosition, player?.CurPosition);
		}

		public void ShowEndOfGame(string playerId) {
			Clients.Caller.showEndOfGame(true);
			Clients.Others.showEndOfGame(false);
		}

		public void DeletePlayer(string playerId) {
			Clients.Others.showMap(_game.GetWorldModel());
		}

		#endregion
	}
}