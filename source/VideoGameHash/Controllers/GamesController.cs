using System.Web.Mvc;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class GamesController : Controller
    {

        private readonly GamesRepository _repository;

        public GamesController(GamesRepository repository)
        {
            _repository = repository;
        }

        //
        // GET: /Games/

        public ActionResult Index()
        {
            return View(_repository.GetGames());
        }

        [HttpGet]
        public ActionResult AddGames()
        {
            var model = new GameFormViewModel
            {
                ActionName = "AddGames"
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult AddGames(string gameSystem)
        {
            _repository.AddGame(gameSystem);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public ActionResult AddGamesWikipedia()
        {
            var model = new GameFormViewModel("Wikipedia")
            {
                ActionName = "AddGamesWikipedia"
            };
            return View("AddGames", model);
        }

        public ActionResult Delete(int id)
        {
            _repository.DeleteGame(id);

            return RedirectToAction("Index");
        }
    }
}
