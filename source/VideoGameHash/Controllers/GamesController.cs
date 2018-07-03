using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class GamesController : Controller
    {

        private readonly IGamesRepository _repository;
        private readonly IGameSystemsRepository _gameSystemsRepository;

        public GamesController(IGamesRepository repository, IGameSystemsRepository gameSystemsRepository)
        {
            _repository = repository;
            _gameSystemsRepository = gameSystemsRepository;
        }

        public ActionResult Index()
        {
            return View(_repository.GetGames());
        }

        [HttpGet]
        public ActionResult AddGames()
        {
            var gameSytems = _gameSystemsRepository.GetGameSystems().Select(x => x.GameSystemName).ToArray();
            var model = new GameFormViewModel(gameSytems)
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
            var gameSytems = _gameSystemsRepository.GetGameSystems().Select(x => x.GameSystemName).ToArray();
            var model = new GameFormViewModel("Wikipedia", gameSytems)
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
