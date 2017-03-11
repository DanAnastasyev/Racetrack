using Microsoft.Owin;
using Owin;
using Racetrack;

[assembly: OwinStartup(typeof(Startup))]

namespace Racetrack {
	public partial class Startup {
		public void Configuration(IAppBuilder app) {
			ConfigureAuth(app);
			app.MapSignalR();
		}
	}
}