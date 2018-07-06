using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class HomeController : Controller
    {
        private readonly IGamesRepository _gamesRepository;
        private readonly IInfoRepository _infoRepository;

        public HomeController(IInfoRepository infoRepository,
            IGamesRepository gamesRepository)
        {
            _infoRepository = infoRepository;
            _gamesRepository = gamesRepository;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.Message = "Welcome to VideoGameHash!";

            var model = new HomePageModel
            {
                Polls = await _infoRepository.GetPolls(),
                TrendingGames = await _gamesRepository.GetTrendingGames(10),
                PopularGames = await _gamesRepository.GetPopularGames(10)
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


        public async Task<ActionResult> GameDetails(int id)
        {
            var game = await _gamesRepository.GetGame(id);

            if (game == null) return RedirectToAction("Index");

            var model = await _gamesRepository.GetGameDetailsViewModel(game, useInfometrics: true);

            if (model == null)
                return RedirectToAction("Index");
            
            ViewBag.GameSystem = model.AvailableGameSystems[0];

            return View("GameDetails", model);
        }

        public async Task<ActionResult> GameDetailsByTitle(string gameTitle)
        {
            var game = await _gamesRepository.GetGame(gameTitle);

            if (game == null) return RedirectToAction("Index");

            var model = await _gamesRepository.GetGameDetailsViewModel(game, useInfometrics: true);

            if (model == null)
                return RedirectToAction("Index");
            
            ViewBag.GameSystem = model.AvailableGameSystems[0];

            return View("GameDetails", model);
        }

        [HttpPost]
        public async Task<ActionResult> SearchGames(string search)
        {
            var list = await _gamesRepository.SearchGameTitles(search);
            return Json(new {data = list}, JsonRequestBehavior.AllowGet);
        }

        public async Task<ActionResult> SubmitPollVote(int pollId, int pollVal)
        {
            await _infoRepository.UpdatePoll(pollId, pollVal);
            var poll = await _infoRepository.GetPoll(pollId);
            var pollVotes = poll.PollAnswers.Select(answers => 
                    new PollGraphModel
                    {
                        Id = answers.Id.ToString(),
                        Title = answers.Answer,
                        NumVotes = answers.NumVotes
                    }).ToList();

            return Json(pollVotes, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public async Task<ActionResult> GetGameArticleContainer(GetGameContainerQuery query)
        {
            var game = await _gamesRepository.GetGame(query.GameTitle);
            var articles = await _infoRepository.GetGameArticles(game, "All", "All");

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
        public async Task<ActionResult> GetGameArticles(GetGameArticlesQuery query)
        {
            var game = await _gamesRepository.GetGame(query.GameTitle);
            var articles = await _infoRepository.GetGameArticles(game, query.Source, query.System);
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