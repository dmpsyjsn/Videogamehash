using System.Collections.Generic;
using Highsoft.Web.Mvc.Charts;

namespace VideoGameHash.Models.HighchartModels
{
    public class LineChartModel
    {
        public LineChartModel()
        {
            Categories = new List<string>();
            ChartSeries = new List<Series>();
        }

        public List<string> Categories { get; set; }
        public List<Series> ChartSeries { get; set; }
    }

    public enum LineChartTickRate
    {
        Monthly,
        Daily
    }

    public enum LineChartTimeRange
    {
        LastMonth,
        Last3Months,
        Last6Months,
        LastYear,
        AllTime
    }
}