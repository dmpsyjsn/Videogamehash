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

        public async Task<ActionResult> Index()
        {
            return View(await _repository.GetGames());
        }

        [HttpGet]
        public async Task<ActionResult> AddGames()
        {
            var gameSytems = (await _gameSystemsRepository.GetGameSystems()).Select(x => x.GameSystemName).ToArray();
            var model = new GameFormViewModel(gameSytems)
            {
                ActionName = "AddGames"
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> AddGames(string gameSystem)
        {
            await _repository.AddGame(gameSystem);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteGame(id);

            return RedirectToAction("Index");
        }
    }
}
