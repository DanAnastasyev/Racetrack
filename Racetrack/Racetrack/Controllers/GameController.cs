using System.Collections.Generic;
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
		[HttpPost]
		public ActionResult WaitForConnection(GameSettingsModel game) {
			GamesManager.Instance.AddPlayerToMap(User.Identity.GetUserId(), game.Map);
			if (game.WithAI == "true") {
				GamesManager.Instance.BeginSinglePlayerGame(User.Identity.GetUserId());
				return RedirectToAction("Index");
			}
			return View();
		}

		[Authorize]
		public ActionResult GameSettings() {
			ViewBag.Maps = new List<string> { "map1" };
			return View();
		}

		[Authorize]
		public ActionResult Result() {
			var players = GamesManager.Instance.GetGame(User.Identity.GetUserId()).GetPlayers();
			players.Sort();
			GamesManager.Instance.DeletePlayer(User.Identity.GetUserId());
			return View(players);
		}
	}
}