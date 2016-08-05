using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VideoGameHash.Models;

namespace VideoGameHash.Controllers
{
    public class GamesController : Controller
    {

        private GamesRepository repository = new GamesRepository();

        //
        // GET: /Games/

        public ActionResult Index()
        {
            return View(repository.GetGames());
        }

        [HttpGet]
        public ActionResult AddGames()
        {
            GameFormViewModel model = new GameFormViewModel();
            model.ActionName = "AddGames";

            return View(model);
        }

        [HttpPost]
        public ActionResult AddGames(string GameSystem)
        {
            repository.AddGame(GameSystem);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult AddGamesWikipedia()
        {
            GameFormViewModel model = new GameFormViewModel("Wikipedia");
            model.ActionName = "AddGamesWikipedia";
            return View("AddGames", model);
        }

        [HttpPost]
        public ActionResult AddGamesWikipedia(string GameSystem)
        {
            repository.AddGameWikipedia(GameSystem);

            return RedirectToAction("Index");
        }

        public ActionResult Delete(int Id)
        {
            repository.DeleteGame(Id);

            return RedirectToAction("Index");
        }
    }
}
