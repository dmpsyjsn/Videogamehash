using System.Collections.Generic;
using PagedList;

namespace VideoGameHash.Models
{
    public class HomePageModel
    {
        public HomePageModel()
        {
            TrendingGames = new List<string>();
            Polls = new List<Poll>();
        }

        public List<string> TrendingGames { get; set; }
        public List<string> PopularGames { get; set; }
        public List<Poll> Polls { get; set; }
    }

    public class MainPageViewModel
    {
        public int Section { get; set; }
        public int Source { get; set; }
        public int GameSystem { get; set; }
        public string Search { get; set; }
        public string ViewType { get; set; }
        public int CurrentPage { get; set; }
        public List<string> SourceList { get; set; }
        public List<string> GameSystemList { get; set; }
    }

    public class ArticleModel
    {
        public int Section { get; set; }
        public int Source { get; set; }
        public int GameSystem { get; set; }
        public string Search { get; set; }
        public string ViewType { get; set; }

        public IPagedList<Articles> CurrentPage { get; set; }
    }

    public class LatestInfoModels
    {
        public string Source { get; set; }
        public bool Small { get; set; }
        public List<QDFeedParser.BaseFeedItem> Entries { get; set; }
    }

    public class PollGraphModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int NumVotes { get; set; }
    }
}