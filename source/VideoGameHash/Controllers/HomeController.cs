using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.Web;
using System.Web.Mvc;
using PagedList;
using VideoGameHash.Models;
using VideoGameHash.Helpers;
using DotNet.Highcharts;
using DotNet.Highcharts.Enums;
using DotNet.Highcharts.Helpers;
using DotNet.Highcharts.Options;
using Point = DotNet.Highcharts.Options.Point;

namespace VideoGameHash.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            ViewBag.Message = "Welcome to VideoGameHash!";
            InfoRepository ir = new InfoRepository();

            HomePageModels model = new HomePageModels();
            Dictionary<int, IEnumerable<FeaturedArticles>> featured = new Dictionary<int, IEnumerable<FeaturedArticles>>();
            Dictionary<int, IEnumerable<TrendingGames>> trendingGames = new Dictionary<int, IEnumerable<TrendingGames>>();
            Dictionary<int, Dictionary<int, List<GameInfo>>> gameInfoes = new Dictionary<int, Dictionary<int, List<GameInfo>>>();

            int index = 0;
            foreach (InfoType section in ir.GetInfoTypes())
            {
                if (index >= 3)
                    break;
                featured.Add(section.Id, ir.GetFeaturedArticles(section.Id));
                trendingGames.Add(section.Id, ir.GetTrendingGames(section.Id));
                index++;
            }

            model.Featured = featured;
            model.TrendingGames = trendingGames;
            var gamesToRemove = new List<int>();
            foreach (int sectionId in trendingGames.Keys)
            {
                gameInfoes.Add(sectionId, new Dictionary<int, List<GameInfo>>());
                var localList = new Dictionary<int, List<GameInfo>>();
                foreach (TrendingGames tr in trendingGames[sectionId])
                {
                    localList.Add(tr.Id, new List<GameInfo>());
                    List<GameInfo> localGameList = new List<GameInfo>();
                    foreach (GameInfo info in tr.Game.GameInfoes)
                    {
                        if (ir.GetTrendingArticles(sectionId, tr.Id, info.GameSystemId).Count() > 0)
                        {
                            localGameList.Add(info);
                        }
                    }
                    localList[tr.Id] = localGameList;
                }
                gameInfoes[sectionId] = localList;
            }

            model.TrendGamesDesc = gameInfoes;

            model.Polls = ir.GetPolls();

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

        public ActionResult ViewArticlesN(int Section, int GameSystem, string Search, string ViewType)
        {
            GameSystemsRepository gameSystemsRepository = new GameSystemsRepository();
            InfoRepository ir = new InfoRepository();
            MainPageViewModel model = new MainPageViewModel();

            model.GameSystemList = new List<string>();
            model.SourceList = new List<string>();

            model.Section = Section;
            model.Source = -1;
            model.GameSystem = GameSystem;
            model.Search = Search;
            model.ViewType = ViewType ?? "List";  // TO DO: Get this from database

            foreach (GameSystem gameSystem in gameSystemsRepository.GetGameSystems())
            {
                if (gameSystem.GameSystemName == "All" || ir.GetNumSourceEntriesByGameSystem(Section, gameSystem.Id) > 0)
                    model.GameSystemList.Add(gameSystem.GameSystemName);
            }

            foreach (InfoSource source in ir.GetSources())
            {
                if (ir.GetNumEntriesBySectionAndSource(Section, source.InfoSourceName) > 0)
                    model.SourceList.Add(source.InfoSourceName);
            }

            return View("ViewArticles", model);
        }

        //
        // GET: News
        public ActionResult ViewArticles(int Section, int Source, int GameSystem, string Search, string ViewType)
        {
            InfoRepository ir = new InfoRepository();
            GameSystemsRepository gameSystemsRepository = new GameSystemsRepository();
            MainPageViewModel model = new MainPageViewModel();

            model.GameSystemList = new List<string>();
            model.SourceList = new List<string>();

            if (Source > 0 && ir.GetNumSourceEntriesByGameSystem(Section, Source, GameSystem) == 0)
                GameSystem = ir.GetGameSystemId("All");

            model.Section = Section;
            model.Source = Source;
            model.GameSystem = GameSystem;
            model.Search = Search;
            model.ViewType = ViewType ?? "List";  // TO DO: Get this from database

            foreach (GameSystem gameSystem in gameSystemsRepository.GetGameSystems())
            {
                if (gameSystem.GameSystemName == "All" || ir.GetNumSourceEntriesByGameSystem(Section, Source, gameSystem.Id) > 0)
                    model.GameSystemList.Add(gameSystem.GameSystemName);
            }

            foreach (InfoSource source in ir.GetSources())
            {
                if (ir.GetNumEntriesBySectionAndSource(Section, source.InfoSourceName) > 0)
                    model.SourceList.Add(source.InfoSourceName);
            }

            return View(model);
        }

        //
        // ChangeView
        public ActionResult ChangeView(string ViewType, int Section, int GameSystem, int Source)
        {
            if (Source < 0)
                return RedirectToAction("ViewArticlesN", new { Section = Section, GameSystem = GameSystem, ViewType = ViewType });
            else
                return RedirectToAction("ViewArticles", new { Section = Section, Source = Source, GameSystem = GameSystem, ViewType = ViewType });
        }

        //
        // POST: SearchNews
        [HttpPost]
        public ActionResult SearchArticles(string Search, int Section, int Source, int GameSystem, string ViewType)
        {
            if (Search.Trim().Length == 0)
                Search = null;
            if (Source < 0)
                return RedirectToAction("ViewArticlesN", new { Section = Section, GameSystem = GameSystem, Search = Search, ViewType = ViewType });
            else
                return RedirectToAction("ViewArticles", new { Section = Section, Source = Source, GameSystem = GameSystem, Search = Search, ViewType = ViewType });
        }

        public ActionResult GameDatabase(string GameTitle)
        {
            GamesRepository gr = new GamesRepository();
            Games Game = new Games();
            if (!String.IsNullOrEmpty(GameTitle))
                Game = gr.GetGameByGameTitle(GameTitle);
            return View(Game);
        }

        [HttpPost]
        public ActionResult GetGameDetails(int Id, string GameSystem)
        {
            GamesRepository gameRepository = new GamesRepository();
            InfoRepository ir = new InfoRepository();

            Games game = gameRepository.GetGame(Id);

            GameDetailsModel model = new GameDetailsModel();
            model.Game = game;

            model.AvailableGameSystems = gameRepository.GetGameSystemsForThisGame(game);

            string currentGameSystem;
            if (String.IsNullOrEmpty(GameSystem))
                currentGameSystem = model.AvailableGameSystems[0];
            else
                currentGameSystem = GameSystem;

            Dictionary<int, IEnumerable<Articles>> details = new Dictionary<int, IEnumerable<Articles>>();

            foreach (InfoType type in ir.GetInfoTypes())
            {
                if (ir.ContainsArticles(type.Id, game.GameTitle, currentGameSystem))
                    details.Add(type.Id, ir.GetGameArticles(type.Id, game.GameTitle, currentGameSystem));
            }

            if (ir.GetInfoTypeName(Id) == "News" && ir.ContainsArticles(Id, game.GameTitle, currentGameSystem))
                model.UseInfoMetrics = true;
            else
                model.UseInfoMetrics = false;

            model.ImageLink = gameRepository.GetImage(game.Id, currentGameSystem);
            model.Publisher = gameRepository.GetPublisher(game.Id, currentGameSystem);
            model.Developer = gameRepository.GetDeveloper(game.Id, currentGameSystem);
            model.USReleaseDate = gameRepository.GetReleaseDate(game.Id, currentGameSystem);
            model.Overview = gameRepository.GetOverview(game.Id, currentGameSystem);
            model.GamesDBNetId = gameRepository.GetGamesDBNetId(game.Id, currentGameSystem);
            model.Articles = details;
            ViewBag.GameSystem = currentGameSystem;

            return PartialView("_GameDetails", model);
        }

        [HttpPost]
        public ActionResult GetLatestInfo(string InfoType, string Source)
        {
            List<QDFeedParser.BaseFeedItem> entries = new List<QDFeedParser.BaseFeedItem>();

            Uri feedUri;
            if (InfoType == "News")
                feedUri = new Uri(LatestInfoHelper.LatestNewsUrls[Source]);
            else if (InfoType == "Reviews")
                feedUri = new Uri(LatestInfoHelper.LatestReviewsUrls[Source]);
            else // InfoType == "Media", hopefully
                feedUri = new Uri(LatestInfoHelper.LatestMediaUrls[Source]);

            QDFeedParser.IFeedFactory factory = new QDFeedParser.HttpFeedFactory();

            try
            {
                QDFeedParser.IFeed feed = factory.CreateFeed(feedUri);

                if (feed.Items.Count() > 0)
                {
                    for (int j = 0; j < 10; j++)
                    {
                        if (feed.Items.Count() < j)
                            break;
                        entries.Add(feed.Items[j]);
                    }
                }
            }
            catch { }

            LatestInfoModels model = new LatestInfoModels();

            if (entries.Count() > 0)
            {
                model.Entries = entries;
                model.Source = Source;
            }

            return PartialView("LatestInfo", model);
        }

        [HttpPost]
        public ActionResult GetLatestArticles(int Section, int Source, int GameSystem, bool Small)
        {
            InfoRepository ir = new InfoRepository();

            List<QDFeedParser.BaseFeedItem> entries = new List<QDFeedParser.BaseFeedItem>();

            string Url = ir.GetUrl(Section, Source, GameSystem);
            string gameSystem = ir.GetGameSystemName(GameSystem);

            Uri feedUri = new Uri(Url);
            QDFeedParser.IFeedFactory factory = new QDFeedParser.HttpFeedFactory();
            QDFeedParser.IFeed feed = factory.CreateFeed(feedUri);

            int maxItems = Small ? 5 : 10;

            if (feed.Items.Count() > 0)
            {
                for (int j = 0; j < maxItems; j++)
                {
                    if (feed.Items.Count() < j)
                        break;
                    entries.Add(feed.Items[j]);
                }
            }

            LatestInfoModels model = new LatestInfoModels();
            model.Entries = entries;
            model.Small = Small;
            if (gameSystem == "All")
                model.Source = string.Format("Latest News from {0}", ir.GetInfoSourceName(Source));
            else
                model.Source = string.Format("Latest {0} News from {1}", gameSystem, ir.GetInfoSourceName(Source));

            return PartialView("LatestInfo", model);
        }

        public ActionResult GetArticles(int Section, int Source, int GameSystem, string Search, string ViewType, int ArticleIndex)
        {
            ArticleModel model = new ArticleModel();

            model.Section = Section;
            model.Source = Source;
            model.GameSystem = GameSystem;
            model.Search = Search;
            model.ViewType = ViewType ?? "List";

            InfoRepository ir = new InfoRepository();

            const int pageSize = 9;

            IQueryable<Articles> items = null;

            if (!String.IsNullOrEmpty(Search))
                items = ir.GetArticles(Section, Source, GameSystem, Search).Skip(ArticleIndex * pageSize).Take(pageSize);
            else
                items = ir.GetArticles(Section, Source, GameSystem).Skip(ArticleIndex * pageSize).Take(pageSize);

            model.CurrentPage = items.ToPagedList(ArticleIndex, pageSize);

            return PartialView(model);
        }

        [HttpPost]
        public ActionResult GetDatabaseList(char Letter)
        {
            GamesRepository gamesRepository = new GamesRepository();
            return PartialView("_DatabaseList", gamesRepository.GetSortedGamesByLetter(Letter));
        }

        [HttpPost]
        public ActionResult SearchDatabase(string Search)
        {
            GamesRepository gamesRepository = new GamesRepository();
            return PartialView("_DatabaseList", gamesRepository.SearchGames(Search));
        }

        [HttpPost]
        public ActionResult GetTrendingInfoBySystem(int Section, int TrendingGameId, int GameSystemId)
        {
            InfoRepository ir = new InfoRepository();
            TrendingInfoModel model = new TrendingInfoModel();
            model.Section = Section;
            model.TrendingGameId = TrendingGameId;

            model.TrendingArticles = ir.GetTrendingArticles(Section, TrendingGameId, GameSystemId);
            List<TrendingArticles> test = model.TrendingArticles.ToList();
            return PartialView("TrendingInfo", model);
        }

        public ActionResult SubmitPollVote(int PollId, int PollVal)
        {
            InfoRepository ir = new InfoRepository();
            ir.UpdatePoll(PollId, PollVal);
            Poll poll = ir.GetPoll(PollId);
            var pollVotes = new List<PollGraphModel>();

            foreach (PollAnswers answers in poll.PollAnswers)
            {
                PollGraphModel model = new PollGraphModel();
                model.Id = answers.Id.ToString();
                model.Title = answers.Answer;
                model.NumVotes = answers.NumVotes;
                pollVotes.Add(model);
            }

            return Json(pollVotes, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetGameInfometricsPieChart(string gameTitle, string gameSystem)
        {
            try
            {
                var infoRepository = new InfoRepository();

                // Retreive the relevant articles
                var gameArticles = infoRepository.GetGameArticles(1, gameTitle, gameSystem).ToList();
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
                .SetTitle(new Title { Text = String.Format("{0} News Articles By Source - Last 6 Months", gameTitle.Replace("'", "\\\'")) })
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

        public ActionResult GetGameInfometricsLineChart(string GameTitle, string GameSystem)
        {
            InfoRepository ir = new InfoRepository();
            
            string[] categories = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            
            // Retreive the relevant articles
            List<Articles> gameArticles = ir.GetGameArticles(1, GameTitle, GameSystem).ToList(); // 1 = News Section
            List<Articles> gameArticlesBySource = gameArticles.GroupBy(u => u.DatePublished.Month).Select(u => u.FirstOrDefault()).OrderBy(u => u.DatePublished).ToList();
            
            int count = gameArticlesBySource.Count();
            string[] monthsUsed = new string[count];
            object[,] chartData = new object[count, 2];
            
            // Populate the data series
            for (int i = 0; i < count; i++)
            {
                Articles article = gameArticlesBySource[i];
                monthsUsed[i] = String.Format("{0} {1}", categories[article.DatePublished.Month - 1], article.DatePublished.Year);
                chartData.SetValue(monthsUsed[i], i, 0);
                chartData.SetValue(gameArticles.Count(u => u.DatePublished.Month == article.DatePublished.Month), i, 1);
            }
            
            Highcharts chart = new Highcharts("infometricslinechart")
                .InitChart(new Chart { DefaultSeriesType = ChartTypes.Line })
                .SetLegend(new Legend { Enabled = false })
                .SetTitle(new Title { Text = String.Format("News Articles Per Month for {0}", GameTitle) })
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
