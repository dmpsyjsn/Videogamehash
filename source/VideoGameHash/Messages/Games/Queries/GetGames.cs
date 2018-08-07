using System.Collections.Generic;
using VideoGameHash.Repositories;

namespace VideoGameHash.Messages.Games.Queries
{
    public class GetGames: IQuery<IEnumerable<Models.Games>>
    {
    }
}