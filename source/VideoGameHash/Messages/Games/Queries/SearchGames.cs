using System.Collections.Generic;
using VideoGameHash.Repositories;

namespace VideoGameHash.Messages.Games.Queries
{
    public class SearchGames : IQuery<IEnumerable<string>>
    {
        public string GameTitle { get; set; }
    }
}