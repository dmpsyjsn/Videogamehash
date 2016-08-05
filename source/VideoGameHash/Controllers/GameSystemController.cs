using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VideoGameHash.Models;

namespace VideoGameHash.Controllers
{
    public class GameSystemsController : Controller
    {
        private GameSystemsRepository gameSystemsRepository = new GameSystemsRepository();

        //
        // GET: /GameSystem/

        public ActionResult Index()
        {
            return View(gameSystemsRepository.GetGameSystems());
        }

        // 
        // GET: /AddGameSystem
        public ActionResult AddGameSystem()
        {
            return View();
        }

        // POST: /AddGameSystem

        [HttpPost]
        public ActionResult AddGameSystem(GameSystemModel model)
        {

            gameSystemsRepository.AddGameSystem(model);

            return RedirectToAction("Index");
        }

        // 
        // GET: /DeleteGameSystem
        public ActionResult DeleteGameSystem(int Id)
        {
            gameSystemsRepository.DeleteGameSystem(Id);

            return RedirectToAction("Index");
        }

        // GET: /GameSystemList
        public ActionResult GameSystemList()
        {
            GameSystemSortOrderEdit order = new GameSystemSortOrderEdit();
            order.GameSystemSortOrders = gameSystemsRepository.GetGameSystemSortOrder();

            return View(order);
        }

        // POST: /GameSystemList
        [HttpPost]
        public ActionResult GameSystemList(GameSystemSortOrderEdit model)
        {
            if (model.GameSystemSortOrders != null)
            {
                foreach (GameSystemSortOrder order in model.GameSystemSortOrders)
                {
                    gameSystemsRepository.UpdateOrder(order);
                }
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /Edit/1
        public ActionResult Edit(int Id)
        {
            GameSystem gameSystem = gameSystemsRepository.GetGameSystemById(Id);

            return View(gameSystem);
        }
    }
}
