using System.Collections.Generic;
using Highsoft.Web.Mvc.Charts;

namespace VideoGameHash.Models.HighchartModels
{
    public class PieChartModel
    {
        public PieChartModel()
        {
            PieSeriesData = new List<PieSeriesData>();
        }

        public List<PieSeriesData> PieSeriesData { get; set; }
    }
}