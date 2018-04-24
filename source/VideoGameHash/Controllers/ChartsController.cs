using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Highsoft.Web.Mvc.Charts;
using VideoGameHash.Models.HighchartModels;
using VideoGameHash.Repositories;

namespace VideoGameHash.Controllers
{
    public class ChartsController : Controller
    {
        private readonly InfoRepository _infoRepository;

        public ChartsController(InfoRepository infoRepository)
        {
            _infoRepository = infoRepository;
        }

        #region controller methods

        public ActionResult GetLineChart(string gameTitle, LineChartTimeRange range = LineChartTimeRange.Last3Months, LineChartTickRate tick = LineChartTickRate.Daily)
        {   
            var model = new LineChartModel();

            // Retrieve the relevant articles
            var gameArticles = _infoRepository.GetGameArticles(gameTitle, "All", "All").Where(x => InTimeRange(x.DatePublished, range)).OrderBy(x => x.DatePublished).ToList();

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
            var sourcesByDayTotals = new Dictionary<string, List<int>>();
            foreach (var source in allSources)
            {
                var articleList = new List<int>();
                foreach (var date in gameArticlesBySource.Keys)
                {
                    var articlesBySource = tick == LineChartTickRate.Daily
                        ? gameArticles.Where(x =>
                            x.InfoSourceId == source.Id && date.Equals(x.DatePublished.ToShortDateString())).ToList()
                        : gameArticles.Where(x =>
                            x.InfoSourceId == source.Id &&
                            date.Equals($"{months[x.DatePublished.Month - 1]} {x.DatePublished.Year}")).ToList();

                    articleList.Add(articlesBySource.Count);
                }

                sourcesByDayTotals[source.InfoSourceName] = articleList;
            }


            // Generate the data series for each info source
            var series = new List<Series>();

            foreach (var source in sourcesByDayTotals)
            {
                var subchart = new List<LineSeriesData>();
                foreach (var data in source.Value)
                {
                    subchart.Add(new LineSeriesData
                    {
                        Y = data
                    });
                }

                series.Add(new LineSeries
                {
                    Name = source.Key,
                    Data = subchart
                });
            }

            // Generate the series that sums up all info sources
            var chart = new List<LineSeriesData>();
            foreach (var data in gameArticlesBySource.Values)
            {
                chart.Add(new LineSeriesData
                {
                    Y = data
                });
            }

            series.Add(new LineSeries
            {
                Name = "All",
                Data = chart
            });

            model.ChartSeries = series;
            
            return PartialView("LineChartView", model);
        }

        public ActionResult GetPieChart(string gameTitle)
        {
            var model = new PieChartModel();

            // Retrieve the relevant articles
            var gameArticles = _infoRepository.GetGameArticles(gameTitle, "All", "All").OrderBy(x => x.DatePublished).ToList();

            if (!gameArticles.Any()) return PartialView("PieChartView", model);

            var articlesBySource = gameArticles.GroupBy(x => x.InfoSource).ToDictionary(x => x.Key, x => x.ToList().Count);

            foreach (var infoSource in articlesBySource)
            {
                model.PieSeriesData.Add(new PieSeriesData{Name = infoSource.Key.InfoSourceName, Y = infoSource.Value});
            }

            return PartialView("PieChartView", model);
        }

        #endregion
        
        #region private methods

        private static bool InTimeRange(DateTime datePublished, LineChartTimeRange range)
        {
            if (range == LineChartTimeRange.AllTime)
                return true;

            DateTime cutoff;
            if (range == LineChartTimeRange.LastMonth)
            {
                cutoff = DateTime.Now.AddDays(-30);
            }
            else if (range == LineChartTimeRange.Last3Months)
            {
                cutoff = DateTime.Now.AddDays(-90);
            }
            else if (range == LineChartTimeRange.Last6Months)
            {
                cutoff = DateTime.Now.AddDays(-180);
            }
            else // range == LastYear
            {
                cutoff = DateTime.Now.AddDays(-365);
            }

            return datePublished >= cutoff;
        }

        #endregion

    }
}
