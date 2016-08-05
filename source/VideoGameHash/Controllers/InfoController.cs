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

        private InfoRepository ir = new InfoRepository();

        public ActionResult Index()
        {
            InfoTypeViewModel model = new InfoTypeViewModel();
            model.InfoSources = ir.GetSources();
            model.InfoTypes = ir.GetInfoTypes();
            model.InfoSourceRssUrls = ir.GetRssUrls();
            model.Polls = ir.GetPolls();
            
            return View(model);
        }

        // GET: AddInfoType
        public ActionResult AddInfoType()
        {
            AddInfoModel model = new AddInfoModel();
            model.InfoType = "AddInfoType";
            return View("AddInfo", model);
        }

        // POST: AddInfoType
        [HttpPost]
        public ActionResult AddInfoType(AddInfoModel model)
        {
            ir.AddInfoType(model);
            return RedirectToAction("Index");
        }

        // GET: AddInfoSource
        public ActionResult AddInfoSource()
        {
            AddInfoModel model = new AddInfoModel();
            model.InfoType = "AddInfoSource";
            return View("AddInfo", model);
        }

        // POST: AddInfoSource
        [HttpPost]
        public ActionResult AddInfoSource(AddInfoModel model)
        {
            ir.AddInfoSource(model);

            return RedirectToAction("Index");
        }

        // GET: AddUrl
        public ActionResult AddUrl()
        {
            AddUrlModel urlModel = new AddUrlModel();
            AddUrlViewModel model = new AddUrlViewModel(urlModel);
            return View(model);
        }

        [HttpPost]
        public ActionResult AddUrl(FormCollection collection)
        {
            AddUrlModel model = new AddUrlModel();
            model.Section = collection["Section"];
            model.Source = collection["Source"];
            model.GameSystem = collection["GameSystem"];
            model.Url = collection["Url"];
            ir.AddUrl(model);

            return RedirectToAction("Index");
        }

        public ActionResult EditInfoType(int id)
        {
            EditSectionModel model = new EditSectionModel();
            InfoType infoType = ir.GetInfoType(id);

            model.Id = infoType.Id;
            model.Name = infoType.InfoTypeName;
            model.UseGameSystem = infoType.UseGameSystem;

            return View("EditSection", model);
        }

        [HttpPost]
        public ActionResult EditInfoType(EditSectionModel model)
        {
            ir.EditSectionInfo(model);

            return RedirectToAction("Index");
        }

        public ActionResult EditInfoSource(int id)
        {
            EditModel model = new EditModel();
            InfoSource infoSource = ir.GetInfoSource(id);

            model.Id = infoSource.Id;
            model.Name = infoSource.InfoSourceName;
            model.ActionLink = "EditInfoSource";

            return View("EditInfo", model);
        }

        [HttpPost]
        public ActionResult EditInfoSource(EditModel model)
        {
            ir.EditInfo("Source", model);

            return RedirectToAction("Index");
        }

        public ActionResult EditUrl(int id)
        {
            EditModel model = new EditModel();
            InfoSourceRssUrls url = ir.GetRssUrl(id);

            model.Id = url.Id;
            model.Name = url.URL;
            model.ActionLink = "EditUrl";

            return View("EditInfo", model);
        }

        [HttpPost]
        public ActionResult EditUrl(EditModel model)
        {
            ir.EditInfo("URL", model);

            return RedirectToAction("Index");
        }

        // GET: /GameSystemList
        public ActionResult InfoTypeList()
        {
            InfoTypeSortOrderEdit order = new InfoTypeSortOrderEdit();
            order.InfoTypeSortOrders = ir.GetInfoTypeSortOrder();

            return View(order);
        }

        // POST: /GameSystemList
        [HttpPost]
        public ActionResult InfoTypeList(InfoTypeSortOrderEdit model)
        {
            if (model.InfoTypeSortOrders != null)
            {
                foreach (InfoTypeSortOrder order in model.InfoTypeSortOrders)
                {
                    ir.UpdateOrder(order);
                }
            }

            return RedirectToAction("Index");
        }

        // GET: /GameSystemList
        public ActionResult InfoSourceList()
        {
            InfoSourceSortOrderEdit order = new InfoSourceSortOrderEdit();
            order.InfoSourceSortOrders = ir.GetInfoSourceSortOrder();

            return View(order);
        }

        // POST: /GameSystemList
        [HttpPost]
        public ActionResult InfoSourceList(InfoSourceSortOrderEdit model)
        {
            if (model.InfoSourceSortOrders != null)
            {
                foreach (InfoSourceSortOrder order in model.InfoSourceSortOrders)
                {
                    ir.UpdateOrder(order);
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult GetArticles(int section)
        {
            ir.AddFeedItems(section);

            return RedirectToAction("Index");
        }

        public ActionResult GetAllArticles()
        {
            ir.AddFeedItems();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteOldArticles()
        {
            ir.DeleteOldArticles();
            return RedirectToAction("Index");
        }

        public ActionResult MakeFeatured(int section)
        {
            ir.MakeFeatured(section);
            return RedirectToAction("Index");
        }

        public ActionResult MakeTrending(int section)
        {
            ir.MakeTrending(section);
            return RedirectToAction("Index");
        }

        public ActionResult DeleteInfoType(int id)
        {
            ir.DeleteInfoType(id);

            return RedirectToAction("Index");
        }

        public ActionResult DeleteInfoSource(int id)
        {
            ir.DeleteInfoSource(id);

            return RedirectToAction("Index");
        }

        public ActionResult AddPoll()
        {
            return View();
        }

        [HttpPost]
        public ActionResult AddPoll(AddPollModel model)
        {
            ir.AddPoll(model);
            return RedirectToAction("Index");
        }

        public ActionResult DeleteUrl(int id)
        {
            ir.DeleteUrl(id);
            return RedirectToAction("Index");
        }
    }
}
