using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using VideoGameHash.Handlers;
using VideoGameHash.Messages.Games.Queries;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class HomeController : Controller
    {
        private readonly IInfoRepository _infoRepository;
        private readonly IQueryProcessor _queryProcessor;

        public HomeController(IInfoRepository infoRepository, IQueryProcessor queryProcessor)
        {
            _infoRepository = infoRepository;
            _queryProcessor = queryProcessor;
        }

        public async Task<ActionResult> Index()
        {
            ViewBag.Message = "Welcome to VideoGameHash!";

            var relatedGames = await _queryProcessor.Process(new GetTrendingAndPopularGames(10));

            var model = new HomePageModel
            {
                Polls = await _infoRepository.GetPolls(),
                TrendingGames = relatedGames.TrendingGames,
                PopularGames = relatedGames.PopularGames
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
            var model = await _queryProcessor.Process(new GetGameDetailsByGameId
            {
                GameId = id,
                UseInfoMetrics = true
            });

            if (model == null) return RedirectToAction("Index");
            
            ViewBag.GameSystem = model.AvailableGameSystems[0];

            return View("GameDetails", model);
        }

        public async Task<ActionResult> GameDetailsByTitle(string gameTitle)
        {
            var model = await _queryProcessor.Process(new GetGameDetailsByGameTitle
            {
                GameTitle = gameTitle,
                UseInfoMetrics = true
            });

            if (model == null) return RedirectToAction("Index");
            
            ViewBag.GameSystem = model.AvailableGameSystems[0];

            return View("GameDetails", model);
        }

        [HttpPost]
        public async Task<ActionResult> SearchGames(string search)
        {
            var searchTerm = HttpUtility.HtmlDecode(search);
            var list = await _queryProcessor.Process(new SearchGames
            {
                GameTitle = searchTerm
            });
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
        [ValidateInput(false)]
        public async Task<ActionResult> GetGameArticleContainer(GetGameContainerQuery query)
        {
            var gameTitle = HttpUtility.HtmlDecode(query.GameTitle);
            var game = await _queryProcessor.Process(new GetGameByTitle
            {
                Title = gameTitle
            });

            if (game == null) return PartialView("ArticleContainer", new GameArticlesHeaderModel());

            var articles = await _infoRepository.GetGameArticles(game, "All", "All");

            var model = new GameArticlesHeaderModel
            {
                GameTitle = gameTitle,
                Sources = articles.GroupBy(x => x.InfoSource).OrderBy(x => x.Key.InfoSourceSortOrder.SortOrder)
                    .Select(x => x.Key.InfoSourceName).ToList(),
                Systems = game.GameSystems
            };

            return PartialView("ArticleContainer", model);
        }

        [HttpGet]
        [ValidateInput(false)]
        public async Task<ActionResult> GetGameArticles(GetGameArticlesQuery query)
        {
            var gameTitle = HttpUtility.HtmlDecode(query.GameTitle);
            var game = await _queryProcessor.Process(new GetGameByTitle
            {
                Title = gameTitle
            });

            if (game == null) return PartialView("Article", new GameArticlesViewModel());

            var articles = await _infoRepository.GetGameArticles(game, query.Source, query.System);
            var multiplier = query.View.Equals("List") ? 10 : 12;

            var model = new GameArticlesViewModel
            {
                GameTitle = gameTitle,
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