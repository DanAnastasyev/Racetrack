using Microsoft.Owin;
using Owin;
using Racetrack.Server;

[assembly: OwinStartup(typeof(Startup))]

namespace Racetrack.Server {
	public class Startup {
		public void Configuration(IAppBuilder app) {
			// Any connection or hub wire up and configuration should go here
			app.MapSignalR();
		}
	}
}