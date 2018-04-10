using System.Collections.Generic;
using Highsoft.Web.Mvc.Charts;

namespace VideoGameHash.Models.HighchartModels
{
    public class LinechartModel
    {
        public List<string> Categories { get; set; }
        public List<LineSeriesData> ChartData { get; set; }
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