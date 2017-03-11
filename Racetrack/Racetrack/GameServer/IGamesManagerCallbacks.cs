using System.Collections.Generic;

namespace Racetrack.GameServer {
	public interface IGamesManagerCallbacks {
		void JoinPlayers(List<string> players);
	}
}