using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Handlers;
using VideoGameHash.Handlers.GameSystems.Commands;
using VideoGameHash.Messages.GameSystems.Commands;
using VideoGameHash.Messages.GameSystems.Queries;
using VideoGameHash.Models;

namespace VideoGameHash.Controllers
{
    public class GameSystemsController : Controller
    {
        private readonly IQueryProcessor _queryProcessor;
        private readonly GameSystemCommandHandlers _handlers;

        public GameSystemsController(IQueryProcessor queryProcessor, 
            GameSystemCommandHandlers handlers)
        {
            _queryProcessor = queryProcessor;
            _handlers = handlers;
        }

        public async Task<ActionResult> Index()
        {
            var result = await _queryProcessor.Process(new GetGameSystems());
            return View(result);
        }

        public ActionResult AddGameSystem()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddGameSystem(GameSystemModel model)
        {
            var addGameSystem = new AddGameSystem(model.GameSystem);
            await _handlers.Handle(addGameSystem);

            var addGameSystemSortOrder = new AddGameSystemSortOrder(model.GameSystem);
            await _handlers.Handle(addGameSystemSortOrder);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteGameSystem(int id)
        {
            await _handlers.Handle(new DeleteGameSystem(id));

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> GameSystemList()
        {
            var order = await _queryProcessor.Process(new GetGameSystemSortOrder());

            return View(order);
        }

        [HttpPost]
        public async Task<ActionResult> GameSystemList(GameSystemSortOrderEdit model)
        {
            await _handlers.Handle(new UpdateGameSystemOrder(model.GameSystemSortOrders));

            return RedirectToAction("Index");
        }
    }
}
