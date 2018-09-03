using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Handlers;
using VideoGameHash.Helpers;
using VideoGameHash.Messages.Games.Commands;
using VideoGameHash.Messages.Games.Queries;
using VideoGameHash.Messages.GameSystems.Queries;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class GamesController : Controller
    {
        private readonly IQueryProcessor _queryProcessor;
        private readonly ICommandHandler<AddGames> _addGamesHandler;
        private readonly ICommandHandler<AddGameInfo> _addGameInfoHandler;
        private readonly ICommandHandler<DeleteGame> _deleteGameHandler;

        public GamesController(IGameSystemsRepository gameSystemsRepository, 
            IQueryProcessor queryProcessor, 
            ICommandHandler<AddGames> addGamesHandler, 
            ICommandHandler<AddGameInfo> addGameInfoHandler, 
            ICommandHandler<DeleteGame> deleteGameHandler)
        {
            _queryProcessor = queryProcessor;
            _addGamesHandler = addGamesHandler;
            _addGameInfoHandler = addGameInfoHandler;
            _deleteGameHandler = deleteGameHandler;
        }

        public async Task<ActionResult> Index()
        {
            var result = await _queryProcessor.Process(new GetGames());
            return View(result);
        }

        [HttpGet]
        public async Task<ActionResult> AddGames()
        {
            var model = await _queryProcessor.Process(new GetGameSystemForms());

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
                GameSystem = gameSystem,
                Publishers = await TheGamesDBHelper.GetDataByField("Publishers"),
                Developers = await TheGamesDBHelper.GetDataByField("Developers")
            });

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
