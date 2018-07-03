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

        public async Task<ActionResult> Index()
        {
            var model = new InfoTypeViewModel
            {
                InfoSources = await _infoRepository.GetSources(),
                InfoTypes = await _infoRepository.GetInfoTypes(),
                InfoSourceRssUrls = await _infoRepository.GetRssUrls(),
                Polls = await _infoRepository.GetPolls()
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
        public async Task<ActionResult> AddInfoType(AddInfoModel model)
        {
            await _infoRepository.AddInfoType(model.Name);
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
        public async Task<ActionResult> AddInfoSource(AddInfoModel model)
        {
            await _infoRepository.AddInfoSource(model.Name);
            
            return RedirectToAction("Index");
        }

        // GET: AddUrl
        public async Task<ActionResult> AddUrl()
        {
            var urlModel = new AddUrlModel();
            var model = new AddUrlViewModel(urlModel);
            model.Section = new SelectList((await _infoRepository.GetInfoTypes()).Select(x => x.InfoTypeName).ToList(), model.Section);
            model.Source = new SelectList((await _infoRepository.GetSources()).Select(x => x.InfoSourceName).ToList(), model.Source);
            model.GameSystem = new SelectList(_gameSystemsRepository.GetGameSystems().Select(x => x.GameSystemName).ToList(), model.GameSystem);
            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> AddUrl(FormCollection collection)
        {
            var model = new AddUrlModel
            {
                Section = collection["Section"],
                Source = collection["Source"],
                GameSystem = collection["GameSystem"],
                Url = collection["Url"]
            };
            await _infoRepository.AddUrl(model);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> EditInfoType(int id)
        {
            var model = new EditSectionModel();
            var infoType = await _infoRepository.GetInfoType(id);

            model.Id = infoType.Id;
            model.Name = infoType.InfoTypeName;
            model.UseGameSystem = infoType.UseGameSystem;

            return View("EditSection", model);
        }

        [HttpPost]
        public async Task<ActionResult> EditInfoType(EditSectionModel model)
        {
            await _infoRepository.EditSectionInfo(model);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> EditInfoSource(int id)
        {
            var model = new EditModel();
            var infoSource = await _infoRepository.GetInfoSource(id);

            model.Id = infoSource.Id;
            model.Name = infoSource.InfoSourceName;
            model.ActionLink = "EditInfoSource";

            return View("EditInfo", model);
        }

        [HttpPost]
        public async Task<ActionResult> EditInfoSource(EditModel model)
        {
            await _infoRepository.EditInfo("Source", model);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> EditUrl(int id)
        {
            var model = new EditModel();
            var url = await _infoRepository.GetRssUrl(id);

            model.Id = url.Id;
            model.Name = url.URL;
            model.ActionLink = "EditUrl";

            return View("EditInfo", model);
        }

        [HttpPost]
        public async Task<ActionResult> EditUrl(EditModel model)
        {
            await _infoRepository.EditInfo("URL", model);

            return RedirectToAction("Index");
        }

        // GET: /GameSystemList
        public async Task<ActionResult> InfoTypeList()
        {
            var order = new InfoTypeSortOrderEdit
            {
                InfoTypeSortOrders = await _infoRepository.GetInfoTypeSortOrder()
            };

            return View(order);
        }

        // POST: /GameSystemList
        [HttpPost]
        public async Task<ActionResult> InfoTypeList(InfoTypeSortOrderEdit model)
        {
            if (model.InfoTypeSortOrders != null)
            {
                foreach (var order in model.InfoTypeSortOrders)
                {
                    await _infoRepository.UpdateOrder(order);
                }
            }

            return RedirectToAction("Index");
        }

        // GET: /GameSystemList
        public async Task<ActionResult> InfoSourceList()
        {
            var order = new InfoSourceSortOrderEdit
            {
                InfoSourceSortOrders = await _infoRepository.GetInfoSourceSortOrder()
            };

            return View(order);
        }

        // POST: /GameSystemList
        [HttpPost]
        public async Task<ActionResult> InfoSourceList(InfoSourceSortOrderEdit model)
        {
            if (model.InfoSourceSortOrders != null)
            {
                foreach (var order in model.InfoSourceSortOrders)
                {
                    await _infoRepository.UpdateOrder(order);
                }
            }

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> GetAllArticles()
        {
            await _infoRepository.AddFeedItems();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteOldArticles()
        {
            await _infoRepository.DeleteOldArticles();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> MakeTrending()
        {
            await _infoRepository.MakeTrending();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> MakePopular()
        {
            await _infoRepository.MakePopular();
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteInfoType(int id)
        {
            await _infoRepository.DeleteInfoType(id);

            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteInfoSource(int id)
        {
            await _infoRepository.DeleteInfoSource(id);

            return RedirectToAction("Index");
        }

        public ActionResult AddPoll()
        {
            return View();
        }

        [HttpPost]
        public async Task<ActionResult> AddPoll(AddPollModel model)
        {
            await _infoRepository.AddPoll(model);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<ActionResult> EditPoll(int id)
        {
            var poll = await _infoRepository.GetPoll(id);

            var model =  new EditPollModel
            {
                Id = poll.Id,
                Title = poll.Title,
                Answers = string.Join(Environment.NewLine, poll.PollAnswers.Select(x => x.Answer).ToList())
            };

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> EditPoll(EditPollModel model)
        {
            await _infoRepository.EditPoll(model);
            return RedirectToAction("Index");
        }

        public async Task<ActionResult> DeleteUrl(int id)
        {
            await _infoRepository.DeleteUrl(id);
            return RedirectToAction("Index");
        }

        // Http Post
        [HttpPost]
        public async Task<ActionResult> JsonImport()
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
                        var infoTypeId = await _infoRepository.AddInfoType(item.Type);

                        // Import News Source
                        var sourceId = await _infoRepository.AddInfoSource(item.Source);

                        // Import Game System
                        var systemId = _gameSystemsRepository.AddGameSystem(item.System);

                        // Import Rss Url
                        await _infoRepository.AddUrl(infoTypeId, sourceId, systemId, item.Link);
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

        public async Task<ActionResult> ReplaceGameSystemNamedAll()
        {
            await _infoRepository.ReplaceGameSystemAll();

            return RedirectToAction("Index");
        }
    }
}
