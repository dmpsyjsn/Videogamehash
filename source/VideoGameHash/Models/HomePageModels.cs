using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace VideoGameHash.Models
{
    public class HomePageModels
    {
        public Dictionary<int, IEnumerable<FeaturedArticles>> Featured;
        public Dictionary<int, IEnumerable<TrendingGames>> TrendingGames;
        public Dictionary<int, Dictionary<int, List<GameInfo>>> TrendGamesDesc;
        public IEnumerable<Poll> Polls;
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

    public class TrendingInfoModel
    {
        public int Section { get; set; }
        public int TrendingGameId { get; set; }
        public IEnumerable<TrendingArticles> TrendingArticles { get; set; }
    }

    public class PollGraphModel
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public int NumVotes { get; set; }
    }

    public class PieChartModel
    {
        public string Type;
        public string Name;
        public List<PieChartSourceModel> Data;
    }

    public class PieChartSourceModel
    {
        public string Source;
        public int NumStories;
    }
}