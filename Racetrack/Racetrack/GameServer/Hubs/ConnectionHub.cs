using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace Racetrack.GameServer.Hubs {
	public class ConnectionHub : Hub, IGamesManagerCallbacks {
		private readonly GamesManager _gamesManager;

		public ConnectionHub() : this(GamesManager.Instance) {}

		public ConnectionHub(GamesManager gamesManager) {
			_gamesManager = gamesManager;
		}

		public void JoinPlayers(List<string> players) {
			foreach (var playerId in players) {
				Clients.Client(playerId).showBtn();
			}
		}

		#region Connection lifecycle

		public override Task OnConnected() {
			var playerId = Context.ConnectionId;
			_gamesManager.OnNewPlayerConnection(playerId, this);

			return base.OnConnected();
		}

		public override Task OnDisconnected(bool stopCalled) {
			var playerId = Context.ConnectionId;

			return base.OnDisconnected(stopCalled);
		}

		#endregion
	}
}