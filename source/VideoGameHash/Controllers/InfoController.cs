using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VideoGameHash.Models;
using VideoGameHash.Helpers;

namespace VideoGameHash.Controllers
{
    public class InfoController : Controller
    {
        //
        // GET: /Info/

        private readonly InfoRepository _infoRepository;

        public InfoController(InfoRepository infoRepository)
        {
            _infoRepository = infoRepository;
        }

        public ActionResult Index()
        {
            var model = new InfoTypeViewModel
            {
                InfoSources = _infoRepository.GetSources(),
                InfoTypes = _infoRepository.GetInfoTypes(),
                InfoSourceRssUrls = _infoRepository.GetRssUrls(),
                Polls = _infoRepository.GetPolls()
            };

            return View(model);
        }

        // GET: AddInfoType
        public ActionResult AddInfoType()
        {
            var model = new AddInfoModel
            {
                InfoType = "AddInfoType"
            };
            return View("AddInfo", model);
        }

        // POST: AddInfoType
        [HttpPost]
        public ActionResult AddInfoType(AddInfoModel model)
        {
            _infoRepository.AddInfoType(model);
            return RedirectToAction("Index");
        }

        // GET: AddInfoSource
        public ActionResult AddInfoSource()
        {
            var model = new AddInfoModel
            {
                InfoType = "AddInfoSource"
            };
            return View("AddInfo", model);
        }

        // POST: AddInfoSource
        [HttpPost]
        public ActionResult AddInfoSource(AddInfoModel model)
        {
            _infoRepository.AddInfoSource(model);

            return RedirectToAction("Index");
        }

        // GET: AddUrl
        public ActionResult AddUrl()
        {
            var urlModel = new AddUrlModel();
            var model = new AddUrlViewModel(urlModel);
            return View(model);
        }

        [HttpPost]
        public ActionResult AddUrl(FormCollection collection)
        {
            var model = new AddUrlModel
            {
                Section = collection["Section"],
                Source = collection["Source"],
                GameSystem = collection["GameSystem"],
                Url = collection["Url"]
            };
            _infoRepository.AddUrl(model);

            return RedirectToAction("Index");
        }

        public ActionResult EditInfoType(int id)
        {
            var model = new EditSectionModel();
            var infoType = _infoRepository.GetInfoType(id);

            model.Id = infoType.Id;
            model.Name = infoType.InfoTypeName;
            model.UseGameSystem = infoType.UseGameSystem;

            return View("EditSection", model);
        }

        [HttpPost]
        public ActionResult EditInfoType(EditSectionModel model)
        {
            _infoRepository.EditSectionInfo(model);

            return RedirectToAction("Index");
        }

        public ActionResult EditInfoSource(int id)
        {
            var model = new EditModel();
            var infoSource = _infoRepository.GetInfoSource(id);

            model.Id = infoSource.Id;
            model.Name = infoSource.InfoSourceName;
            model.ActionLink = "EditInfoSource";

            return View("EditInfo", model);
        }

        [HttpPost]
        public ActionResult EditInfoSource(EditModel model)
        {
            _infoRepository.EditInfo("Source", model);

            return RedirectToAction("Index");
        }

        public ActionResult EditUrl(int id)
        {
            var model = new EditModel();
            var url = _infoRepository.GetRssUrl(id);

            model.Id = url.Id;
            model.Name = url.URL;
            model.ActionLink = "EditUrl";

            return View("EditInfo", model);
        }

        [HttpPost]
        public ActionResult EditUrl(EditModel model)
        {
            _infoRepository.EditInfo("URL", model);

            return RedirectToAction("Index");
        }

        // GET: /GameSystemList
        public ActionResult InfoTypeList()
        {
            var order = new InfoTypeSortOrderEdit
            {
                InfoTypeSortOrders = _infoRepository.GetInfoTypeSortOrder()
            };

            return View(order);
        }

        // POST: /GameSystemList
        [HttpPost]
        public ActionResult InfoTypeList(InfoTypeSortOrderEdit model)
        {
            if (model.InfoTypeSortOrders != null)
            {
                foreach (var order in model.InfoTypeSortOrders)
                {
                    _infoRepository.UpdateOrder(order);
                }
            }

            return RedirectToAction("Index");
        }

        // GET: /GameSystemList
        public ActionResult InfoSourceList()
        {
            var order = new InfoSourceSortOrderEdit
            {
                InfoSourceSortOrders = _infoRepository.GetInfoSourceSortOrder()
            };

            return View(order);
        }

        // POST: /GameSystemList
        [HttpPost]
        public ActionResult InfoSourceList(InfoSourceSortOrderEdit model)
        {
            if (model.InfoSourceSortOrders != null)
            {
                foreach (var order in model.InfoSourceSortOrders)
                {
                    _infoRepository.UpdateOrder(order);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult GetArticles(int section)
        {
            _infoRepository.AddFeedItems(section);

            return RedirectToAction("Index");
        }

        public ActionResult GetAllArticles()
        {
            _infoRepository.AddFeedItems();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteOldArticles()
        {
            _infoRepository.DeleteOldArticles();
            return RedirectToAction("Index");
        }

        public ActionResult MakeFeatured(int section)
        {
            _infoRepository.MakeFeatured(section);
            return RedirectToAction("Index");
        }

        public ActionResult MakeTrending(int section)
        {
            _infoRepository.MakeTrending(section);
            return RedirectToAction("Index");
        }

        public ActionResult DeleteInfoType(int id)
        {
            _infoRepository.DeleteInfoType(id);

            return RedirectToAction("Index");
        }

        public ActionResult DeleteInfoSource(int id)
        {
            _infoRepository.DeleteInfoSource(id);

            return RedirectToAction("Index");
        }

        public ActionResult AddPoll()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPoll(AddPollModel model)
        {
            _infoRepository.AddPoll(model);
            return RedirectToAction("Index");
        }

        public ActionResult DeleteUrl(int id)
        {
            _infoRepository.DeleteUrl(id);
            return RedirectToAction("Index");
        }

        // Http Post
        [HttpPost]
        public ActionResult JsonImport()
        {
            return RedirectToAction("Index");
        }
    }
}
