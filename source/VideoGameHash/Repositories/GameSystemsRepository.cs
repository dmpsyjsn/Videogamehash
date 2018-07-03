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
        Task<int> AddGameSystem(string gameSystemName);
        Task<GameSystem> GetGameSystemById(int id);
        Task DeleteGameSystem(int id);
        Task<IEnumerable<GameSystemSortOrder>> GetGameSystemSortOrder();
        Task UpdateOrder(GameSystemSortOrder order);
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

        public async Task<int> AddGameSystem(string gameSystemName)
        {
            var existingGameSystem = await GetGameSystemByGameSystemName(gameSystemName);
            if (existingGameSystem != null) return existingGameSystem.Id;

            var gameSystem = new GameSystem
            {
                GameSystemName = gameSystemName
            };
            _db.GameSystems.Add(gameSystem);
            _db.SaveChanges();

            int? maxValue = _db.GameSystemSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
            var order = new GameSystemSortOrder();
            gameSystem = await GetGameSystemByGameSystemName(gameSystemName);
            order.Id = gameSystem.Id;
            order.GameSystem = gameSystem;
            order.SortOrder = maxValue + 1 ?? 1;

            _db.GameSystemSortOrders.Add(order);
            await _db.SaveChangesAsync();

            return gameSystem.Id;
        }

        public async Task<GameSystem> GetGameSystemById(int id)
        {
            return await _db.GameSystems.SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task DeleteGameSystem(int id)
        {
            var gameSystem = await GetGameSystemById(id);

            if (gameSystem != null)
            {
                foreach (var article in await GetArticlesByGameSystemId(id))
                {
                    _db.Articles.Remove(article);
                }
                await _db.SaveChangesAsync();

                foreach (var url in await GetUrlsByGameSystemId(id))
                {
                    _db.InfoSourceRssUrls.Remove(url);
                }
                await _db.SaveChangesAsync();

                foreach (var gameInfo in await GetGameInfoByGameSystemId(id))
                {
                    _db.GameInfoes.Remove(gameInfo);
                }
                await _db.SaveChangesAsync();

                var sortOrder = await _db.GameSystemSortOrders.SingleOrDefaultAsync(u => u.GameSystem.Id == id);
                if (sortOrder != null)
                {
                    _db.GameSystemSortOrders.Remove(sortOrder);
                    await _db.SaveChangesAsync();
                }

                _db.GameSystems.Remove(gameSystem);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<GameSystemSortOrder>> GetGameSystemSortOrder()
        {
            return await _db.GameSystemSortOrders.ToListAsync();
        }

        public async Task UpdateOrder(GameSystemSortOrder order)
        {
            var dbOrder = await _db.GameSystemSortOrders.SingleOrDefaultAsync(t => t.Id == order.Id);

            if (dbOrder != null) dbOrder.SortOrder = order.SortOrder;

            await _db.SaveChangesAsync();
        }

        #region Private methods

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