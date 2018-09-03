using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Models;

namespace VideoGameHash.Repositories
{
    public interface IGameSystemsRepository
    {
        Task<IEnumerable<GameSystem>> GetGameSystems();
    }

    public class GameSystemsRepository : IGameSystemsRepository
    {
        private readonly VGHDatabaseContainer _db;

        public GameSystemsRepository(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task<IEnumerable<GameSystem>> GetGameSystems()
        {
            return await _db.GameSystems.OrderBy(u => u.GameSystemSortOrder.SortOrder).ToListAsync();
        }
    }
}