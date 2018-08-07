using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Messages.Games.Queries;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Handlers.Games.Queries
{
    public class GetGamesHandler : IQueryHandler<GetGames, IEnumerable<Models.Games>>
    {
        private readonly VGHDatabaseContainer _db;

        public GetGamesHandler(VGHDatabaseContainer db)
        {
            _db = db;
        }


        public async Task<IEnumerable<Models.Games>> Handle(GetGames query)
        {
            return await _db.Games.OrderBy(u => u.GameTitle).ToListAsync();
        }
    }
}