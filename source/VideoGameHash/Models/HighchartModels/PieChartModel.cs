using System.Collections.Generic;

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

    public class PieSeriesData
    {
        public string Name { get; set; }
        public int Y { get; set; }
    }
}