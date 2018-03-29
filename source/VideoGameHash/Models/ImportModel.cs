using System.Collections.Generic;

namespace VideoGameHash.Models
{
    public class ImportModel
    {
        public List<ImportData> Data { get; set; }
    }

    public class ImportData
    {
        public string Type { get; set; }
        public string System { get; set; }
        public string Source { get; set; }
        public string Link { get; set; }
    }
}