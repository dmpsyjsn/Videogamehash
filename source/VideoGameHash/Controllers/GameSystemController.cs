using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Handlers;
using VideoGameHash.Messages.GameSystems.Commands;
using VideoGameHash.Messages.GameSystems.Queries;
using VideoGameHash.Models;

namespace VideoGameHash.Controllers
{
    public class GameSystemsController : Controller
    {
        private readonly IQueryProcessor _queryProcessor;
        private readonly ICommandHandler<AddGameSystem> _addGameSystemHandler;
        private readonly ICommandHandler<AddGameSystemSortOrder> _addGameSystemSortOrderHandler;
        private readonly ICommandHandler<DeleteGameSystem> _deleteGameSystemHandler;
        private readonly ICommandHandler<UpdateGameSystemOrder> _updateGameSystemSortOrderHandler;

        public GameSystemsController(IQueryProcessor queryProcessor, 
            ICommandHandler<AddGameSystem> addGameSystemHandler, 
            ICommandHandler<AddGameSystemSortOrder> addGameSystemSortOrderHandler, 
            ICommandHandler<DeleteGameSystem> deleteGameSystemHandler, ICommandHandler<UpdateGameSystemOrder> updateGameSystemSortOrderHandler)
        {
            _addGameSystemHandler = addGameSystemHandler;
            _addGameSystemSortOrderHandler = addGameSystemSortOrderHandler;
            _deleteGameSystemHandler = deleteGameSystemHandler;
            _updateGameSystemSortOrderHandler = updateGameSystemSortOrderHandler;
            _queryProcessor = queryProcessor;
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
            await _addGameSystemHandler.Handle(addGameSystem);

            var addGameSystemSortOrder = new AddGameSystemSortOrder(model.GameSystem);
            await _addGameSystemSortOrderHandler.Handle(addGameSystemSortOrder);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteGameSystem(int id)
        {
            await _deleteGameSystemHandler.Handle(new DeleteGameSystem(id));

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
            await _updateGameSystemSortOrderHandler.Handle(new UpdateGameSystemOrder(model.GameSystemSortOrders));

            return RedirectToAction("Index");
        }
    }
}
