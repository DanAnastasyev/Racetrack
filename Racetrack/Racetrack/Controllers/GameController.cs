using System.Web.Mvc;

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