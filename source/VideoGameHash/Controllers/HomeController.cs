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
            var articles = _infoRepository.GetGameArticles(query.GameTitle, "All", "All").ToList();
            
            var model = new GameArticlesHeaderModel
            {
                GameTitle = query.GameTitle,
                Sources = articles.GroupBy(x => x.InfoSource).OrderBy(x => x.Key.InfoSourceSortOrder.SortOrder).Select(x => x.Key.InfoSourceName).ToList(),
                Systems = articles.GroupBy(x => x.GameSystem).OrderBy(x => x.Key.GameSystemSortOrder.SortOrder).Select(x => x.Key.GameSystemName).ToList(),
            };

            return PartialView("ArticleContainer", model);
        }

        [HttpGet]
        public ActionResult GetGameArticles(GetGameArticlesQuery query)
        {
            var articles = _infoRepository.GetGameArticles(query.GameTitle, query.Source, query.System).ToList();
            var multiplier = query.View.Equals("List") ? 10 : 12;

            var model = new GameArticlesViewModel
            {
                GameTitle = query.GameTitle,
                Articles = articles.Skip((query.Page - 1) * multiplier).Take(multiplier).Select(x => new ArticleViewModel
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
