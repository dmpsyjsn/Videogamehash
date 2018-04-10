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


        public ActionResult GetGameInfometricsLineChart(string gameTitle, LineChartTimeRange range = LineChartTimeRange.AllTime, LineChartTickRate tick = LineChartTickRate.Daily)
        {
            var model = new LinechartModel();

            var months = new List<string> { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
            
            // Retreive the relevant articles
            var gameArticles = _infoRepository.GetGameArticles(gameTitle).ToList();
            var gameArticlesBySource = tick == LineChartTickRate.Daily
                ? gameArticles
                    .Where(x => InTimeRange(x.DatePublished, range)).OrderBy(u => u.DatePublished)
                    .GroupBy(u => u.DatePublished.ToShortDateString())
                    .ToDictionary(x => x.Key, x => x.ToList().Count)
                : gameArticles
                    .Where(x => InTimeRange(x.DatePublished, range)).OrderBy(u => u.DatePublished).GroupBy(u =>
                        $"{months[u.DatePublished.Month - 1]} {u.DatePublished.Year}")
                    .ToDictionary(x => x.Key, x => x.ToList().Count);

            model.Categories = gameArticlesBySource.Keys.ToList();

            var chart = new List<LineSeriesData>();

            foreach (var data in gameArticlesBySource.Values)
            {
                chart.Add(new LineSeriesData
                {
                    Id = "infometricslinechart",
                    Y = data
                });
            }

            model.ChartData = chart;
            
            return PartialView("LineChartView", model);
        }

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
    }
}
