using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Web.Mvc;

namespace VideoGameHash.Models.HighchartModels
{
    public class LineChartModel
    {
        public LineChartModel()
        {
            Categories = new List<string>();
            AlternateSources = new Dictionary<string, List<KeyValuePair<string, int>>>();
        }

        public string GameTitle { get; set; }
        public List<string> Categories { get; set; }
        public Dictionary<string, List<KeyValuePair<string, int>>> AlternateSources;

        public List<string> TickRates => Enum.GetNames(typeof(LineChartTickRate)).ToList();
        public List<SelectListItem> TimeRanges => GetNameAndDescription();

        private List<SelectListItem> GetNameAndDescription()
        {
            var timeRanges = Enum.GetValues(typeof(LineChartTimeRange));

            var nameAndDesc = new List<SelectListItem>();
            foreach (var range in timeRanges)
            {
                var name = Enum.GetName(typeof(LineChartTimeRange), range);
                var desc = DescriptionAttr((LineChartTimeRange)range);

                var kvp = new SelectListItem{
                    Text = desc,
                    Value = name,
                };

                nameAndDesc.Add(kvp);
            }

            return nameAndDesc;
        }

        public static string DescriptionAttr(LineChartTimeRange value)
        {
            var type = typeof(LineChartTimeRange);
            var memInfo = type.GetMember(value.ToString());
            var mem = memInfo.FirstOrDefault(m => m.DeclaringType == type);
            if (mem == null) return string.Empty;
            var attributes = mem.GetCustomAttributes(typeof(DescriptionAttribute), false);
            var description = ((DescriptionAttribute)attributes[0]).Description;

            return description;
        }
    }

    public enum LineChartTickRate
    {
        Monthly,
        Daily
    }

    public enum LineChartTimeRange
    {
        [Description("Last Month")]
        LastMonth,
        
        [Description("Last 3 Months")]
        Last3Months,

        [Description("Last 6 Months")]
        Last6Months,

        [Description("Last Year")]
        LastYear,

        [Description("All Time")]
        AllTime
    }
}