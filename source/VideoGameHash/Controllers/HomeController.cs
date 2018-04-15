using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using PagedList;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class HomeController : Controller
    {
        private readonly InfoRepository _infoRepository;
        private readonly GameSystemsRepository _gameSystemsRepository;
        private readonly GamesRepository _gamesRepository;
        private readonly ErrorRepository _errorRepository;

        public HomeController(InfoRepository infoRepository, GameSystemsRepository gameSystemsRepository, GamesRepository gamesRepository, ErrorRepository errorRepository)
        {
            _infoRepository = infoRepository;
            _gameSystemsRepository = gameSystemsRepository;
            _gamesRepository = gamesRepository;
            _errorRepository = errorRepository;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to VideoGameHash!";

            var model = new HomePageModel
            {
                Polls = _infoRepository.GetPolls(),
                TrendingGames = _gamesRepository.GetTrendingGames(10),
                PopularGames = _gamesRepository.GetPopularGames(10)
            };
            
            return View(model);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        public ActionResult ViewArticlesN(int section, int gameSystem, string search, string viewType)
        {
            var model = new MainPageViewModel
            {
                GameSystemList = new List<string>(),
                SourceList = new List<string>(),

                Section = section,
                Source = -1,
                GameSystem = gameSystem,
                Search = search,
                ViewType = viewType ?? "List",  // TO DO: Get this from database
                CurrentPage = 1
            };

            foreach (var system in _gameSystemsRepository.GetGameSystems())
            {
                if (system.GameSystemName == "All" || _infoRepository.GetNumSourceEntriesByGameSystem(section, system.Id) > 0)
                    model.GameSystemList.Add(system.GameSystemName);
            }

            foreach (var source in _infoRepository.GetSources())
            {
                if (_infoRepository.GetNumEntriesBySectionAndSource(section, source.InfoSourceName) > 0)
                    model.SourceList.Add(source.InfoSourceName);
            }

            return View("ViewArticles", model);
        }

        //
        // GET: News
        public ActionResult ViewArticles(int section, int source, int gameSystem, string search, string viewType)
        {
            var model = new MainPageViewModel
            {
                GameSystemList = new List<string>(),
                SourceList = new List<string>()
            };

            if (source > 0 && _infoRepository.GetNumSourceEntriesByGameSystem(section, source, gameSystem) == 0)
                gameSystem = _infoRepository.GetGameSystemId("All");

            model.Section = section;
            model.Source = source;
            model.GameSystem = gameSystem;
            model.Search = search;
            model.ViewType = viewType ?? "List";  // TO DO: Get this from database

            foreach (var system in _gameSystemsRepository.GetGameSystems())
            {
                if (system.GameSystemName == "All" || _infoRepository.GetNumSourceEntriesByGameSystem(section, source, system.Id) > 0)
                    model.GameSystemList.Add(system.GameSystemName);
            }

            foreach (var src in _infoRepository.GetSources())
            {
                if (_infoRepository.GetNumEntriesBySectionAndSource(section, src.InfoSourceName) > 0)
                    model.SourceList.Add(src.InfoSourceName);
            }

            return View(model);
        }

        //
        // ChangeView
        public ActionResult ChangeView(string viewType, int section, int gameSystem, int source)
        {
            if (source < 0)
                return RedirectToAction("ViewArticlesN", new { Section = section, GameSystem = gameSystem, ViewType = viewType });
            else
                return RedirectToAction("ViewArticles", new { Section = section, Source = source, GameSystem = gameSystem, ViewType = viewType });
        }

        //
        // POST: SearchNews
        [HttpPost]
        public ActionResult SearchArticles(string search, int section, int source, int gameSystem, string viewType)
        {
            if (search.Trim().Length == 0)
                search = null;
            if (source < 0)
                return RedirectToAction("ViewArticlesN", new { Section = section, GameSystem = gameSystem, Search = search, ViewType = viewType });

            return RedirectToAction("ViewArticles", new { Section = section, Source = source, GameSystem = gameSystem, Search = search, ViewType = viewType });
        }

        public ActionResult GameDetails(string gameTitle)
        {
            if (string.IsNullOrEmpty(gameTitle))
                return RedirectToAction("Index");

            var game = _gamesRepository.GetGame(gameTitle);

            if (game == null)
                return RedirectToAction("Index");

            var model = new GameDetailsModel
            {
                Game = game,
                AvailableGameSystems = _gamesRepository.GetGameSystemsForThisGame(game)
            };

            var currentGameSystem = model.AvailableGameSystems[0];
           
            model.UseInfoMetrics = true;
            model.ImageLinks = _gamesRepository.GetImages(game.Id, model.AvailableGameSystems);
            model.Publisher = _gamesRepository.GetPublisher(game.Id, currentGameSystem);
            model.Developer = _gamesRepository.GetDeveloper(game.Id, currentGameSystem);
            model.UsReleaseDate = _gamesRepository.GetReleaseDate(game.Id, currentGameSystem);
            model.Overview = _gamesRepository.GetOverview(game.Id, currentGameSystem);
            model.GamesDbNetId = _gamesRepository.GetGamesDbNetId(game.Id, currentGameSystem);
            ViewBag.GameSystem = currentGameSystem;

            return View("GameDetails", model);
        }
        
        [HttpPost]
        public ActionResult GetLatestArticles(int section, int source, int gameSystem, bool small)
        {
            var entries = new List<QDFeedParser.BaseFeedItem>();

            var url = _infoRepository.GetUrl(section, source, gameSystem);
            var systemName = _infoRepository.GetGameSystemName(gameSystem);

            var feedUri = new Uri(url);
            QDFeedParser.IFeedFactory factory = new QDFeedParser.HttpFeedFactory();
            var feed = factory.CreateFeed(feedUri);

            var maxItems = small ? 5 : 10;

            if (feed.Items.Any())
            {
                for (var j = 0; j < maxItems; j++)
                {
                    if (feed.Items.Count() < j)
                        break;
                    entries.Add(feed.Items[j]);
                }
            }

            var model = new LatestInfoModels
            {
                Entries = entries,
                Small = small
            };
            if (systemName == "All")
                model.Source = $"Latest News from {_infoRepository.GetInfoSourceName(source)}";
            else
                model.Source = $"Latest {systemName} News from {_infoRepository.GetInfoSourceName(source)}";

            return PartialView("LatestInfo", model);
        }

        public ActionResult GetArticles(int section, int source, int gameSystem, string search, string viewType, int articleIndex)
        {
            var model = new ArticleModel
            {
                Section = section,
                Source = source,
                GameSystem = gameSystem,
                Search = search,
                ViewType = viewType ?? "List"
            };

            const int pageSize = 9;

            IQueryable<Articles> items;

            if (!String.IsNullOrEmpty(search))
                items = _infoRepository.GetArticles(section, source, gameSystem, search).Skip(articleIndex * pageSize).Take(pageSize);
            else
                items = _infoRepository.GetArticles(section, source, gameSystem).Skip(articleIndex * pageSize).Take(pageSize);

            model.CurrentPage = items.ToPagedList(articleIndex, pageSize);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult SearchGames(string search)
        {
            var list = _gamesRepository.SearchGameTitles(search).ToList();
            return Json(new {data = list}, JsonRequestBehavior.AllowGet);
        }

        public ActionResult SubmitPollVote(int pollId, int pollVal)
        {
            _infoRepository.UpdatePoll(pollId, pollVal);
            var poll = _infoRepository.GetPoll(pollId);
            var pollVotes = new List<PollGraphModel>();

            foreach (var answers in poll.PollAnswers)
            {
                var model = new PollGraphModel
                {
                    Id = answers.Id.ToString(),
                    Title = answers.Answer,
                    NumVotes = answers.NumVotes
                };
                pollVotes.Add(model);
            }

            return Json(pollVotes, JsonRequestBehavior.AllowGet);
        }
    }
}
