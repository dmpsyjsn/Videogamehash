using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Messages.GameSystems.Commands;
using VideoGameHash.Models;

namespace VideoGameHash.Handlers.GameSystems.Commands
{
    public class AddGameSystemHandler : ICommandHandler<AddGameSystem>,
        ICommandHandler<AddGameSystemSortOrder>,
        ICommandHandler<DeleteGameSystem>,
        ICommandHandler<UpdateGameSystemOrder>
    {
        private readonly VGHDatabaseContainer _db;

        public AddGameSystemHandler(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task Handle(AddGameSystem command)
        {
            var existingGameSystem = await GetGameSystemByGameSystemName(command.GameSystemName);
            if (existingGameSystem != null) return;

            var gameSystem = new GameSystem
            {
                GameSystemName = command.GameSystemName
            };

            _db.GameSystems.Add(gameSystem);
        }

        public async Task Handle(AddGameSystemSortOrder command)
        {
            var maxValue = _db.GameSystemSortOrders.Max(u => u.SortOrder) + 1;
            var order = new GameSystemSortOrder();
            var gameSystem = await GetGameSystemByGameSystemName(command.GameSystemName);

            if (gameSystem.GameSystemSortOrder?.SortOrder <= 0)
            {
                order.Id = gameSystem.Id;
                order.GameSystem = gameSystem;
                order.SortOrder = maxValue;

                _db.GameSystemSortOrders.Add(order);
            }
        }

        public async Task Handle(DeleteGameSystem command)
        {
            var gameSystem = await GetGameSystemById(command.Id);

            if (gameSystem != null)
            {
                foreach (var article in await GetArticlesByGameSystemId(command.Id))
                {
                    _db.Articles.Remove(article);
                }

                foreach (var url in await GetUrlsByGameSystemId(command.Id))
                {
                    _db.InfoSourceRssUrls.Remove(url);
                }

                foreach (var gameInfo in await GetGameInfoByGameSystemId(command.Id))
                {
                    _db.GameInfoes.Remove(gameInfo);
                }

                var sortOrder = await _db.GameSystemSortOrders.SingleOrDefaultAsync(u => u.GameSystem.Id == command.Id);
                if (sortOrder != null)
                {
                    _db.GameSystemSortOrders.Remove(sortOrder);
                }

                _db.GameSystems.Remove(gameSystem);
            }
        }

        public async Task Handle(UpdateGameSystemOrder command)
        {
            if (command.GameSystemSortOrders != null)
            {
                foreach (var order in command.GameSystemSortOrders)
                {
                    var dbOrder = await _db.GameSystemSortOrders.SingleOrDefaultAsync(t => t.Id == order.Id);

                    if (dbOrder != null) dbOrder.SortOrder = order.SortOrder;
                }
            }
        }

        #region Private Methods

        private async Task<GameSystem> GetGameSystemById(int id)
        {
            return await _db.GameSystems.SingleOrDefaultAsync(u => u.Id == id);
        }

        private async Task<GameSystem> GetGameSystemByGameSystemName(string gameSystemName)
        {
            return await _db.GameSystems.SingleOrDefaultAsync(u => u.GameSystemName == gameSystemName);
        }

        private async Task<IEnumerable<Articles>> GetArticlesByGameSystemId(int gameSystemId)
        {
            return await _db.Articles.Where(u => u.GameSystemId == gameSystemId).ToListAsync();
        }

        private async Task<IEnumerable<InfoSourceRssUrls>> GetUrlsByGameSystemId(int gameSystemId)
        {
            return await _db.InfoSourceRssUrls.Where(u => u.GameSystemId == gameSystemId).ToListAsync();
        }

        private async Task<IEnumerable<GameInfo>> GetGameInfoByGameSystemId(int gameSystemId)
        {
            return await _db.GameInfoes.Where(u => u.GameSystemId == gameSystemId).ToListAsync();
        }

        #endregion
    }
}