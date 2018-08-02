using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Helpers;
using VideoGameHash.Messages.Games.Commands;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class GamesController : Controller
    {

        private readonly IGamesRepository _repository;
        private readonly IGameSystemsRepository _gameSystemsRepository;
        private readonly ICommandHandler<AddGames> _addGamesHandler;
        private readonly ICommandHandler<AddGameInfo> _addGameInfoHandler;

        public GamesController(IGamesRepository repository, IGameSystemsRepository gameSystemsRepository, ICommandHandler<AddGames> addGamesHandler, ICommandHandler<AddGameInfo> addGameInfoHandler)
        {
            _repository = repository;
            _gameSystemsRepository = gameSystemsRepository;
            _addGamesHandler = addGamesHandler;
            _addGameInfoHandler = addGameInfoHandler;
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
            // Step 1: Download games from the GamesDBnet
            var games = await TheGamesDBHelper.GetGamesBySystem(gameSystem);
            
            // Step 2: Add Games
            await _addGamesHandler.Handle(new AddGames
            {
                Games = games
            });

            // Step 3: Add GameInfo
            await _addGameInfoHandler.Handle(new AddGameInfo
            {
                Games = games,
                GameSystem = gameSystem
            });
            
            //await _repository.AddGame(gameSystem);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> Delete(int id)
        {
            await _repository.DeleteGame(id);

            return RedirectToAction("Index");
        }
    }
}
