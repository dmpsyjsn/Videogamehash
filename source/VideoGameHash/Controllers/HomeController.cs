using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class HomeController : Controller
    {
        private readonly GamesRepository _gamesRepository;
        private readonly InfoRepository _infoRepository;

        public HomeController(InfoRepository infoRepository,
            GamesRepository gamesRepository)
        {
            _infoRepository = infoRepository;
            _gamesRepository = gamesRepository;
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


        public ActionResult GameDetails(int id)
        {
            if (id < 0)
                return RedirectToAction("Index");

            var game = _gamesRepository.GetGame(id);

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

        [HttpGet]
        public ActionResult GetGameArticleContainer(GetGameContainerQuery query)
        {
            var game = _gamesRepository.GetGame(query.GameTitle);
            var articles = _infoRepository.GetGameArticles(game, "All", "All").ToList();

            var model = new GameArticlesHeaderModel
            {
                GameTitle = query.GameTitle,
                Sources = articles.GroupBy(x => x.InfoSource).OrderBy(x => x.Key.InfoSourceSortOrder.SortOrder)
                    .Select(x => x.Key.InfoSourceName).ToList(),
                Systems = game.GameInfoes.Select(x => x.GameSystem.GameSystemName).Distinct().ToList()
            };

            return PartialView("ArticleContainer", model);
        }

        [HttpGet]
        public ActionResult GetGameArticles(GetGameArticlesQuery query)
        {
            var game = _gamesRepository.GetGame(query.GameTitle);
            var articles = _infoRepository.GetGameArticles(game, query.Source, query.System).ToList();
            var multiplier = query.View.Equals("List") ? 10 : 12;

            var model = new GameArticlesViewModel
            {
                GameTitle = query.GameTitle,
                Articles = articles.Skip((query.Page - 1) * multiplier).Take(multiplier).Select(x =>
                    new ArticleViewModel
                    {
                        Title = x.Title,
                        Source = x.InfoSource.InfoSourceName,
                        DatePublished = x.DatePublished.ToShortDateString(),
                        System = x.GameSystem.GameSystemName,
                        Link = x.Link
                    }).ToList(),
                PageMultiplier = multiplier,
                View = query.View,
                ShowPrevPage = query.Page - 1 > 0,
                NextPage = query.Page + 1,
                PrevPage = query.Page - 1
            };

            return PartialView("Article", model);
        }
    }
}