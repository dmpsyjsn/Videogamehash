using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Messages.Games.Queries;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Handlers.Games.Queries
{
    public class SearchGamesHandler : IQueryHandler<SearchGames, IEnumerable<string>>
    {
        private readonly VGHDatabaseContainer _db;

        public SearchGamesHandler(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task<IEnumerable<string>> Handle(SearchGames query)
        {
            return await _db.Games.AsQueryable().Where(d => d.GameTitle.ToLower().Contains(query.GameTitle.ToLower())).Take(10).Select(x => x.GameTitle).ToListAsync();
        }
    }
}