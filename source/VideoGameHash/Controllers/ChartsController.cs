using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using VideoGameHash.Handlers;
using VideoGameHash.Messages.Games.Queries;
using VideoGameHash.Models.HighchartModels;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class ChartsController : Controller
    {
        private readonly IInfoRepository _infoRepository;
        private readonly IQueryProcessor _queryProcessor;

        public ChartsController(IInfoRepository infoRepository, IQueryProcessor queryProcessor)
        {
            _infoRepository = infoRepository;
            _queryProcessor = queryProcessor;
        }

        #region controller methods

        public async Task<ActionResult> GetLineChart(string gameTitle, LineChartTimeRange range = LineChartTimeRange.LastYear, LineChartTickRate tick = LineChartTickRate.Daily)
        {
            var game = await _queryProcessor.Process(new GetGameByTitle
            {
                Title = gameTitle
            });
            var model = new LineChartModel();

            // Retrieve the relevant articles
            var gameArticles = await _infoRepository.GetGameArticles(game, "All", "All", range, false);

            if (!gameArticles.Any()) return PartialView("LineChartView", model);

            var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

            // Generate the Categories
            var gameArticlesBySource = tick == LineChartTickRate.Daily
                ? gameArticles
                    .GroupBy(u => u.DatePublished.ToShortDateString())
                    .ToDictionary(x => x.Key, x => x.ToList().Count)
                : gameArticles
                    .GroupBy(u => $"{months[u.DatePublished.Month - 1]} {u.DatePublished.Year}")
                    .ToDictionary(x => x.Key, x => x.ToList().Count);

            model.Categories = gameArticlesBySource.Keys.ToList();

            // Generate the data for all info sources
            var allSources = gameArticles.Select(x => x.InfoSource).Distinct().ToList();
            var sourcesByRateAltTotals = new Dictionary<string, List<KeyValuePair<string, int>>>();
            foreach (var source in allSources)
            {
                var articleListAlt = new List<KeyValuePair<string, int>>();
                foreach (var date in gameArticlesBySource.Keys)
                {
                    var articlesBySource = tick == LineChartTickRate.Daily
                        ? gameArticles.Where(x =>
                            x.InfoSourceId == source.Id && date.Equals(x.DatePublished.ToShortDateString())).ToList()
                        : gameArticles.Where(x =>
                            x.InfoSourceId == source.Id &&
                            date.Equals($"{months[x.DatePublished.Month - 1]} {x.DatePublished.Year}")).ToList();

                    articleListAlt.Add(new KeyValuePair<string, int>(date, articlesBySource.Count));
                }

                sourcesByRateAltTotals[source.InfoSourceName] = articleListAlt;
            }

            model.AlternateSources = sourcesByRateAltTotals;
            
            return PartialView("LineChartView", model);
        }

        public async Task<ActionResult> GetPieChart(string gameTitle)
        {
            var game = await _queryProcessor.Process(new GetGameByTitle
            {
                Title = gameTitle
            });
            var model = new PieChartModel();

            // Retrieve the relevant articles
            var gameArticles = await _infoRepository.GetGameArticles(game, "All", "All");

            if (!gameArticles.Any()) return PartialView("PieChartView", model);

            var articlesBySource = gameArticles.GroupBy(x => x.InfoSource).ToDictionary(x => x.Key, x => x.ToList().Count);

            foreach (var infoSource in articlesBySource)
            {
                model.PieSeriesData.Add(new PieSeriesData{Name = infoSource.Key.InfoSourceName, Y = infoSource.Value});
            }

            return PartialView("PieChartView", model);
        }

        #endregion
    }
}
