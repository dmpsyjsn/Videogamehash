using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Newtonsoft.Json;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    [Authorize]
    public class InfoController : Controller
    {
        private readonly IInfoRepository _infoRepository;
        private readonly IGameSystemsRepository _gameSystemsRepository;
        private readonly IErrorRepository _errorRepository;

        public InfoController(IInfoRepository infoRepository, IGameSystemsRepository gameSystemsRepository, IErrorRepository errorRepository)
        {
            _infoRepository = infoRepository;
            _gameSystemsRepository = gameSystemsRepository;
            _errorRepository = errorRepository;
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
            _infoRepository.AddInfoType(model.Name);
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
            _infoRepository.AddInfoSource(model.Name);

            return RedirectToAction("Index");
        }

        // GET: AddUrl
        public ActionResult AddUrl()
        {
            var urlModel = new AddUrlModel();
            var model = new AddUrlViewModel(urlModel);
            model.Section = new SelectList(_infoRepository.GetInfoTypes().Select(x => x.InfoTypeName).ToList(), model.Section);
            model.Source = new SelectList(_infoRepository.GetSources().Select(x => x.InfoSourceName).ToList(), model.Source);
            model.GameSystem = new SelectList(_gameSystemsRepository.GetGameSystems().Select(x => x.GameSystemName).ToList(), model.GameSystem);
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

        public async Task<ActionResult> GetAllArticles()
        {
            await _infoRepository.AddFeedItems();
            return RedirectToAction("Index");
        }

        public ActionResult DeleteOldArticles()
        {
            _infoRepository.DeleteOldArticles();
            return RedirectToAction("Index");
        }

        public ActionResult MakeTrending()
        {
            _infoRepository.MakeTrending();
            return RedirectToAction("Index");
        }

        public ActionResult MakePopular()
        {
            _infoRepository.MakePopular();
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

        [HttpGet]
        public ActionResult EditPoll(int id)
        {
            var poll = _infoRepository.GetPoll(id);

            var model =  new EditPollModel
            {
                Id = poll.Id,
                Title = poll.Title,
                Answers = string.Join(Environment.NewLine, poll.PollAnswers.Select(x => x.Answer).ToList())
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult EditPoll(EditPollModel model)
        {
            _infoRepository.EditPoll(model);
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
            if (Request.Files.Count > 0)
            {
                var file = Request.Files[0];

                if (file != null && file.ContentLength > 0)
                {
                    // get contents to string
                    var str = new StreamReader(file.InputStream).ReadToEnd();

                    // deserializes string into object
                    var data = JsonConvert.DeserializeObject<ImportModel>(str);
                    
                    foreach (var item in data.Data)
                    {
                        // Import Type of link (News, Reviews, etc)
                        var infoTypeId = _infoRepository.AddInfoType(item.Type);

                        // Import News Source
                        var sourceId = _infoRepository.AddInfoSource(item.Source);

                        // Import Game System
                        var systemId = _gameSystemsRepository.AddGameSystem(item.System);

                        // Import Rss Url
                        _infoRepository.AddUrl(infoTypeId, sourceId, systemId, item.Link);
                    }
                }
            }

            return RedirectToAction("Index");
        }

        public ActionResult Errors()
        {
            var errorModel = new ErrorModel {ErrorMessages = _errorRepository.GetErrorMessages().ToList()};

            return View(errorModel);
        }

        public ActionResult DeleteAllErrors()
        {
            _errorRepository.DeleteAllErrors();
            return RedirectToAction("Manage", "Account");
        }

        public ActionResult ReplaceGameSystemNamedAll()
        {
            _infoRepository.ReplaceGameSystemAll();

            return RedirectToAction("Index");
        }
    }
}
