﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VideoGameHash.Models;

namespace VideoGameHash.Controllers
{
    public class GameSystemsController : Controller
    {
        private readonly GameSystemsRepository _gameSystemsRepository;

        public GameSystemsController(GameSystemsRepository gameSystemsRepository)
        {
            _gameSystemsRepository = gameSystemsRepository;
        }

        //
        // GET: /GameSystem/

        public ActionResult Index()
        {
            return View(_gameSystemsRepository.GetGameSystems());
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

            _gameSystemsRepository.AddGameSystem(model);

            return RedirectToAction("Index");
        }

        // 
        // GET: /DeleteGameSystem
        public ActionResult DeleteGameSystem(int id)
        {
            _gameSystemsRepository.DeleteGameSystem(id);

            return RedirectToAction("Index");
        }

        // GET: /GameSystemList
        public ActionResult GameSystemList()
        {
            var order = new GameSystemSortOrderEdit
            {
                GameSystemSortOrders = _gameSystemsRepository.GetGameSystemSortOrder()
            };

            return View(order);
        }

        // POST: /GameSystemList
        [HttpPost]
        public ActionResult GameSystemList(GameSystemSortOrderEdit model)
        {
            if (model.GameSystemSortOrders != null)
            {
                foreach (var order in model.GameSystemSortOrders)
                {
                    _gameSystemsRepository.UpdateOrder(order);
                }
            }

            return RedirectToAction("Index");
        }

        //
        // GET: /Edit/1
        public ActionResult Edit(int id)
        {
            var gameSystem = _gameSystemsRepository.GetGameSystemById(id);

            return View(gameSystem);
        }
    }
}
