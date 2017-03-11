using System.Linq;
using System.Web.Mvc;
using PagedList;
using Racetrack.Context;

namespace Racetrack.Controllers {
	public class HomeController : Controller {
		private readonly ResultContext _db = new ResultContext();

		public ActionResult Index() {
			return View();
		}

		public ActionResult About() {
			return View();
		}

		public ActionResult Rating(string sortOrder, string currentFilter, string searchString, int? page) {
			ViewBag.CurrentSort = sortOrder;
			ViewBag.MovementsSortParam = string.IsNullOrEmpty(sortOrder) ? "movement_desc" : "";
			ViewBag.NameSortParam = sortOrder == "Name" ? "name_desc" : "Name";

			if (searchString != null) {
				page = 1;
			} else {
				searchString = currentFilter;
			}

			ViewBag.CurrentFilter = searchString;

			var players = _db.Results.AsQueryable();

			if (!string.IsNullOrEmpty(searchString)) {
				players = players.Where(s => s.PlayerName.StartsWith(searchString));
			}
			switch (sortOrder) {
				case "movement_desc":
					players = players.OrderByDescending(s => s.NumberOfMovements);
					break;
				case "Name":
					players = players.OrderBy(s => s.PlayerName);
					break;
				case "name_desc":
					players = players.OrderByDescending(s => s.PlayerName);
					break;
				default:
					players = players.OrderBy(s => s.NumberOfMovements);
					break;
			}

			int pageSize = 3;
			int pageNumber = (page ?? 1);
			return View(players.ToPagedList(pageNumber, pageSize));
		}
	}
}