using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Helpers;
using VideoGameHash.Messages.Games.Commands;
using VideoGameHash.Messages.Games.Queries;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class GamesController : Controller
    {

        private readonly IGamesRepository _repository;
        private readonly IGameSystemsRepository _gameSystemsRepository;
        private readonly IQueryProcessor _queryProcessor;
        private readonly ICommandHandler<AddGames> _addGamesHandler;
        private readonly ICommandHandler<AddGameInfo> _addGameInfoHandler;
        private readonly ICommandHandler<DeleteGame> _deleteGameHandler;

        public GamesController(IGamesRepository repository, IGameSystemsRepository gameSystemsRepository, 
            IQueryProcessor queryProcessor, 
            ICommandHandler<AddGames> addGamesHandler, 
            ICommandHandler<AddGameInfo> addGameInfoHandler, 
            ICommandHandler<DeleteGame> deleteGameHandler)
        {
            _repository = repository;
            _gameSystemsRepository = gameSystemsRepository;
            _queryProcessor = queryProcessor;
            _addGamesHandler = addGamesHandler;
            _addGameInfoHandler = addGameInfoHandler;
            _deleteGameHandler = deleteGameHandler;
        }

        public async Task<ActionResult> Index()
        {
            return View(await _queryProcessor.Process(new GetGames()));
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
            await _deleteGameHandler.Handle(new DeleteGame
            {
                Id = id
            });

            return RedirectToAction("Index");
        }
    }
}
