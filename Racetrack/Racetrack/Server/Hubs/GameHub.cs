using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Racetrack.Server.Models;

namespace Racetrack.Server.Hubs {
	public class GameHub : Hub, IGameUpdatesHandler {
		private readonly Game _game;

		public GameHub() : this(Game.Instance) {}

		public GameHub(Game game) {
			_game = game;
		}

		public void UpdatePlayer(MoveModel move) {
			_game.UpdatePlayer(Context.ConnectionId, move, this);
		}

		#region Connection lifecycle

		public override Task OnConnected() {
			_game.AddPlayer(Context.ConnectionId);
			Clients.Caller.showMap(_game.GetWorldModel());
			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled) {
			_game.DeletePlayer(Context.ConnectionId);
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
			var player = _game.GetPlayer(playerId);

			Clients.All.showMovements(_game.RoundNumber, playerId,
				player?.PrevPosition, player?.CurPosition);
		}

		public void ShowMovements(string playerId) {
			Clients.All.beginNextRound();
		}

		#endregion
	}
}