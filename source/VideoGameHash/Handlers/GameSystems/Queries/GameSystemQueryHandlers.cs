using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Messages.GameSystems.Queries;
using VideoGameHash.Models;

namespace VideoGameHash.Handlers.GameSystems.Queries
{
    public class GameSystemQueryHandlers : IQueryHandler<GetGameSystemId, int>,
        IQueryHandler<GetGameSystems, IEnumerable<GameSystemViewModel>>,
        IQueryHandler<GetGameSystemForms, GameFormViewModel>,
        IQueryHandler<GetGameSystemSortOrder, GameSystemSortOrderEdit>
    {
        private readonly VGHDatabaseContainer _db;

        public GameSystemQueryHandlers(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task<int> Handle(GetGameSystemId query)
        {
            var gameSystem = await _db.GameSystems.SingleOrDefaultAsync(x => x.GameSystemName.Equals(query.GameSystemName));

            if (gameSystem != null) return gameSystem.Id;

            return -1;

        }

        public async Task<IEnumerable<GameSystemViewModel>> Handle(GetGameSystems query)
        {
            var gameSystems = await _db.GameSystems.OrderBy(x => x.GameSystemSortOrder.SortOrder).ToListAsync();

            return gameSystems.Select(x => new GameSystemViewModel(x.Id, x.GameSystemName, x.GameSystemSortOrder.SortOrder)).ToList();
        }

        public async Task<GameFormViewModel> Handle(GetGameSystemForms query)
        {
            var gameSystems = await _db.GameSystems.OrderBy(x => x.GameSystemSortOrder.SortOrder).ToListAsync();
            return new GameFormViewModel(gameSystems.Select(x => x.GameSystemName).ToList())
            {
                ActionName = "AddGames"
            };
        }

        public async Task<GameSystemSortOrderEdit> Handle(GetGameSystemSortOrder query)
        {
            var sortOrders = await _db.GameSystemSortOrders.ToListAsync();

            return new GameSystemSortOrderEdit
            {
                GameSystemSortOrders = sortOrders
            };
        }
    }
}