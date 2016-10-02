using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Racetrack.Server.Models;

namespace Racetrack.Server.Hubs {
	public class GameHub : Hub {
		private readonly Game _game;

		public GameHub() : this(Game.Instance) {}

		public GameHub(Game game) {
			_game = game;
		}

		public void UpdatePlayer(MoveModel move) {
			_game.UpdatePlayer(Context.ConnectionId, move);

			var player = _game.GetPlayer(Context.ConnectionId);

			Clients.All.showMovements(_game.RoundNumber, Context.ConnectionId, 
				player?.PrevPosition, player?.CurPosition);

			if (_game.IsEndOfRound()) {
				Clients.All.beginNextRound();
				_game.BeginNextRound();
			}
		}

		#region Connection lifecycle

		public override Task OnConnected() {
			_game.AddPlayer(Context.ConnectionId);
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
	}
}