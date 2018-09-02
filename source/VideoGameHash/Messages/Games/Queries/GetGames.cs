using System.Collections.Generic;
using VideoGameHash.Handlers;
using VideoGameHash.Models;

namespace VideoGameHash.Messages.Games.Queries
{
    public class GetGames : IQuery<IEnumerable<GameViewModel>>
    {
    }

    public class GetGameById : IQuery<GameViewModel>
    {
        public int Id { get; set; }
    }

    public class GetGameByTitle : IQuery<GameViewModel>
    {
        public string Title { get; set; }
    }

    public class SearchGames : IQuery<IEnumerable<string>>
    {
        public string GameTitle { get; set; }
    }

    public class GetGameDetailsByGameId : IQuery<GameDetailsModel>
    {
        public int GameId { get; set; }
        public bool UseInfoMetrics { get; set; }
    }

    public class GetGameDetailsByGameTitle : IQuery<GameDetailsModel>
    {
        public string GameTitle { get; set; }
        public bool UseInfoMetrics { get; set; }
    }

    public class GetTrendingAndPopularGames : IQuery<TrendingAndPopularGamesViewModel>
    {
        public GetTrendingAndPopularGames(int count)
        {
            Count = count;
        }

        public int Count { get; }
    }
}