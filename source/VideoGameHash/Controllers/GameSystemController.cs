using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class GameSystemsController : Controller
    {
        private readonly IGameSystemsRepository _gameSystemsRepository;

        public GameSystemsController(IGameSystemsRepository gameSystemsRepository)
        {
            _gameSystemsRepository = gameSystemsRepository;
        }

        //
        // GET: /GameSystem/

        public async Task<ActionResult> Index()
        {
            return View(await _gameSystemsRepository.GetGameSystems());
        }

        // 
        // GET: /AddGameSystem
        public ActionResult AddGameSystem()
        {
            return View();
        }

        // POST: /AddGameSystem

        [HttpPost]
        public async Task<ActionResult> AddGameSystem(GameSystemModel model)
        {
            await _gameSystemsRepository.AddGameSystem(model.GameSystem);

            return RedirectToAction("Index");
        }

        // 
        // GET: /DeleteGameSystem
        public async Task<ActionResult> DeleteGameSystem(int id)
        {
            await _gameSystemsRepository.DeleteGameSystem(id);

            return RedirectToAction("Index");
        }

        // GET: /GameSystemList
        public async Task<ActionResult> GameSystemList()
        {
            var order = new GameSystemSortOrderEdit
            {
                GameSystemSortOrders = await _gameSystemsRepository.GetGameSystemSortOrder()
            };

            return View(order);
        }

        // POST: /GameSystemList
        [HttpPost]
        public async Task<ActionResult> GameSystemList(GameSystemSortOrderEdit model)
        {
            if (model.GameSystemSortOrders != null)
            {
                foreach (var order in model.GameSystemSortOrders)
                {
                    await _gameSystemsRepository.UpdateOrder(order);
                }
            }

            return RedirectToAction("Index");
        }
    }
}
