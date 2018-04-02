using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Web.Mvc;
using PagedList;
using VideoGameHash.Models;
using VideoGameHash.Helpers;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Options;

namespace VideoGameHash.Controllers
{
    public class HomeController : Controller
    {
        private readonly InfoRepository _infoRepository;
        private readonly GameSystemsRepository _gameSystemsRepository;
        private readonly GamesRepository _gamesRepository;

        public HomeController(InfoRepository infoRepository, GameSystemsRepository gameSystemsRepository, GamesRepository gamesRepository)
        {
            _infoRepository = infoRepository;
            _gameSystemsRepository = gameSystemsRepository;
            _gamesRepository = gamesRepository;
        }

        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to VideoGameHash!";

            var model = new HomePageModel();

            

            model.Polls = _infoRepository.GetPolls();

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
            else
                return RedirectToAction("ViewArticles", new { Section = section, Source = source, GameSystem = gameSystem, Search = search, ViewType = viewType });
        }

        public ActionResult GameDatabase(string gameTitle)
        {
            var game = new Games();
            if (!String.IsNullOrEmpty(gameTitle))
                game = _gamesRepository.GetGameByGameTitle(gameTitle);
            return View(game);
        }

        [HttpPost]
        public ActionResult GetGameDetails(int id, string gameSystem)
        {
            var game = _gamesRepository.GetGame(id);

            var model = new GameDetailsModel
            {
                Game = game,

                AvailableGameSystems = _gamesRepository.GetGameSystemsForThisGame(game)
            };

            string currentGameSystem;
            if (String.IsNullOrEmpty(gameSystem))
                currentGameSystem = model.AvailableGameSystems[0];
            else
                currentGameSystem = gameSystem;

            var details = new Dictionary<int, IEnumerable<Articles>>();

            foreach (var type in _infoRepository.GetInfoTypes())
            {
                if (_infoRepository.ContainsArticles(type.Id, game.GameTitle, currentGameSystem))
                    details.Add(type.Id, _infoRepository.GetGameArticles(type.Id, game.GameTitle, currentGameSystem));
            }

            if (_infoRepository.GetInfoTypeName(id) == "News" && _infoRepository.ContainsArticles(id, game.GameTitle, currentGameSystem))
                model.UseInfoMetrics = true;
            else
                model.UseInfoMetrics = false;

            model.ImageLink = _gamesRepository.GetImage(game.Id, currentGameSystem);
            model.Publisher = _gamesRepository.GetPublisher(game.Id, currentGameSystem);
            model.Developer = _gamesRepository.GetDeveloper(game.Id, currentGameSystem);
            model.UsReleaseDate = _gamesRepository.GetReleaseDate(game.Id, currentGameSystem);
            model.Overview = _gamesRepository.GetOverview(game.Id, currentGameSystem);
            model.GamesDbNetId = _gamesRepository.GetGamesDbNetId(game.Id, currentGameSystem);
            model.Articles = details;
            ViewBag.GameSystem = currentGameSystem;

            return PartialView("_GameDetails", model);
        }

        [HttpPost]
        public ActionResult GetLatestInfo(string infoType, string source)
        {
            var entries = new List<QDFeedParser.BaseFeedItem>();

            Uri feedUri;
            if (infoType == "News")
                feedUri = new Uri(LatestInfoHelper.LatestNewsUrls[source]);
            else if (infoType == "Reviews")
                feedUri = new Uri(LatestInfoHelper.LatestReviewsUrls[source]);
            else // InfoType == "Media", hopefully
                feedUri = new Uri(LatestInfoHelper.LatestMediaUrls[source]);

            QDFeedParser.IFeedFactory factory = new QDFeedParser.HttpFeedFactory();

            try
            {
                var feed = factory.CreateFeed(feedUri);

                if (feed.Items.Count() > 0)
                {
                    for (var j = 0; j < 10; j++)
                    {
                        if (feed.Items.Count() < j)
                            break;
                        entries.Add(feed.Items[j]);
                    }
                }
            }
            catch
            { 
                // do nothing 
            }

            var model = new LatestInfoModels();

            if (entries.Count() > 0)
            {
                model.Entries = entries;
                model.Source = source;
            }

            return PartialView("LatestInfo", model);
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
        public ActionResult GetDatabaseList(char letter)
        {
            return PartialView("_DatabaseList", _gamesRepository.GetSortedGamesByLetter(letter));
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

        public ActionResult GetGameInfometricsPieChart(string gameTitle, string gameSystem)
        {
            try
            {
                // Retreive the relevant articles
                var gameArticles = _infoRepository.GetGameArticles(1, gameTitle, gameSystem).ToList();
                var gameArticlesBySource = gameArticles.GroupBy(u => u.InfoSource.InfoSourceName).Select(u => u.FirstOrDefault()).ToList();
                var count = gameArticlesBySource.Count();

                var chartData = new object[count, 2];

                // Populate the data series
                for (var i = 0; i < count; i++)
                {
                    var article = gameArticlesBySource[i];

                    // Set the sector title of the pie chart
                    chartData.SetValue(article.InfoSource.InfoSourceName, i, 0);

                    // Set the sector data of the pie chart
                    chartData.SetValue(gameArticles.Count(u => u.InfoSourceId == article.InfoSourceId), i, 1);
                }

                // Create the chart
                var chart = new Highcharts("infometricspiechart")
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = $"{gameTitle.Replace("'", "\\\'")} News Articles By Source - Last 6 Months"
                    })
                .SetTooltip(new Tooltip { Enabled = false })
                .SetPlotOptions(new PlotOptions
                {
                    Pie = new PlotOptionsPie
                    {
                        AllowPointSelect = true,
                        Cursor = Cursors.Pointer,
                        DataLabels = new PlotOptionsPieDataLabels
                        {
                            Color = ColorTranslator.FromHtml("#000000"),
                            ConnectorColor = ColorTranslator.FromHtml("#000000"),
                            Formatter = "function() { return '<b>'+ this.point.name +'</b>: '+ this.y; }"
                        }
                    }
                })
                .SetSeries(new Series
                {
                    Type = ChartTypes.Pie,
                    Name = "News Articles",
                    Data = new DotNet.Highcharts.Helpers.Data(chartData)
                });

                return PartialView("_ChartView", chart);
            }
            catch (Exception ex)
            {
                LogError(ex);

                // Just use an empty chart
                var emptyChart = new Highcharts("infometricspiechart")
                .InitChart(new Chart { PlotShadow = false })
                .SetTitle(new Title { Text = "Unable to create chart" })
                .SetTooltip(new Tooltip { Enabled = false });
                return PartialView("_ChartView", emptyChart);
            }
        }

        private void LogError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public ActionResult GetGameInfometricsLineChart(string gameTitle, string gameSystem)
        {         
            var categories = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            
            // Retreive the relevant articles
            var gameArticles = _infoRepository.GetGameArticles(1, gameTitle, gameSystem).ToList(); // 1 = News Section
            var gameArticlesBySource = gameArticles.GroupBy(u => u.DatePublished.Month).Select(u => u.First()).OrderBy(u => u.DatePublished).ToList();
            
            var count = gameArticlesBySource.Count();
            var monthsUsed = new string[count];
            var chartData = new object[count, 2];
            
            // Populate the data series
            for (var i = 0; i < count; i++)
            {
                var article = gameArticlesBySource[i];
                monthsUsed[i] = $"{categories[article.DatePublished.Month - 1]} {article.DatePublished.Year}";
                chartData.SetValue(monthsUsed[i], i, 0);
                chartData.SetValue(gameArticles.Count(u => u.DatePublished.Month == article.DatePublished.Month), i, 1);
            }
            
            var chart = new Highcharts("infometricslinechart")
                .InitChart(new Chart { DefaultSeriesType = ChartTypes.Line })
                .SetLegend(new Legend { Enabled = false })
                .SetTitle(new Title { Text = $"News Articles Per Month for {gameTitle}"})
                .SetXAxis(new XAxis { Categories = monthsUsed })
                .SetYAxis(new YAxis { Title = new YAxisTitle { Text = "# of News Articles" } })
                .SetTooltip(new Tooltip { Enabled = true, Formatter = @"function() { return '<b>'+ this.series.name +'</b><br/>'+ this.x +': '+ this.y; }" })
                .SetPlotOptions(new PlotOptions
                {
                    Line = new PlotOptionsLine
                    {
                        DataLabels = new PlotOptionsLineDataLabels
                        {
                            Enabled = true
                        },
                        EnableMouseTracking = false
                    }
                })
                .SetSeries(new[]
                {
                    new Series { Data = new DotNet.Highcharts.Helpers.Data(chartData) }
                });
            
            return PartialView("_ChartView", chart);
        }
    }
}
