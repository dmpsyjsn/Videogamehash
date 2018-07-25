using System.Collections.Generic;

namespace VideoGameHash.Models.TheGamesDB
{

    public class Game
    {
        public int id { get; set; }
        public string game_title { get; set; }
        public string release_date { get; set; }
        public int platform { get; set; }
        public List<int?> developers { get; set; }
    }

    public class Data
    {
        public int count { get; set; }
        public List<Game> games { get; set; }
    }

    public class Pages
    {
        public object previous { get; set; }
        public string current { get; set; }
        public string next { get; set; }
    }

    public class RootObject
    {
        public int code { get; set; }
        public string status { get; set; }
        public Data data { get; set; }
        public Pages pages { get; set; }
        public int remaining_monthly_allowance { get; set; }
        public int extra_allowance { get; set; }
    }
    
}