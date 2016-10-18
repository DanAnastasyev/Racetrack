using System.Collections.Generic;
using System.Web.Mvc;

namespace Racetrack.GameServer {
	public interface IGamesManagerCallbacks {
		void JoinPlayers(List<string> players);
	}
}
