using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Racetrack.GameServer;
using Racetrack.Models;

namespace Racetrack.Controllers {
	public class GameController : Controller {
		// GET: Game
		[Authorize]
		public ActionResult Index() {
			return View();
		}

		[Authorize]
		public ActionResult WaitForConnection() {
			return View();
		}

		[Authorize]
		public ActionResult Result() {
			var players = GamesManager.Instance.GetGame(User.Identity.GetUserId()).GetPlayers();
			players.Sort();
			return View(players);
		}
	}
}