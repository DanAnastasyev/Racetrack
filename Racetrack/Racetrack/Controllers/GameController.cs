using System.Web.Mvc;
using Racetrack.GameServer;

namespace Racetrack.Controllers {
	public class GameController : Controller {
		// GET: Game
		public ActionResult Index() {
			return View();
		}

		public ActionResult WaitForConnection() {
			return View();
		}
	}
}