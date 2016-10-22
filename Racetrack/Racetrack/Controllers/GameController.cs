using System.Web.Mvc;
using Microsoft.AspNet.Identity;

namespace Racetrack.Controllers {
	public class GameController : Controller {
		// GET: Game
		public ActionResult Index() {
			if (!User.Identity.IsAuthenticated) {
				return RedirectToAction("Login", "Account", new { ReturnUrl = Request.RawUrl });
			}
			return View();
		}

		public ActionResult WaitForConnection() {
			string a = User.Identity.GetUserId();
			if (!User.Identity.IsAuthenticated) {
				return RedirectToAction("Login", "Account", new { ReturnUrl = Request.RawUrl });
			}
			return View();
		}
	}
}