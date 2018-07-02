using System.Collections.Generic;
using System.Linq;
using VideoGameHash.Models;

namespace VideoGameHash.Repositories
{
    public interface IGameSystemsRepository
    {
        IEnumerable<GameSystem> GetGameSystems();
        int AddGameSystem(string gameSystemName);
        GameSystem GetGameSystemById(int id);
        void DeleteGameSystem(int id);
        IEnumerable<GameSystemSortOrder> GetGameSystemSortOrder();
        void UpdateOrder(GameSystemSortOrder order);
    }

    public class GameSystemsRepository : IGameSystemsRepository
    {
        private readonly VGHDatabaseContainer _db;

        public GameSystemsRepository(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public IEnumerable<GameSystem> GetGameSystems()
        {
            var systems = _db.GameSystems.OrderBy(u => u.GameSystemSortOrder.SortOrder);
            return systems;
        }

        public int AddGameSystem(string gameSystemName)
        {
            try
            {
                var existingGameSystem = GetGameSystemByGameSystemName(gameSystemName);
                if (existingGameSystem != null) return existingGameSystem.Id;

                var gameSystem = new GameSystem
                {
                    GameSystemName = gameSystemName
                };
                _db.GameSystems.Add(gameSystem);
                _db.SaveChanges();

                int? maxValue = _db.GameSystemSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
                var order = new GameSystemSortOrder();
                gameSystem = GetGameSystemByGameSystemName(gameSystemName);
                order.Id = gameSystem.Id;
                order.GameSystem = gameSystem;
                order.SortOrder = maxValue + 1 ?? 1;

                _db.GameSystemSortOrders.Add(order);
                _db.SaveChanges();

                return gameSystem.Id;
            }
            catch
            {
                // Do Nothing
            }

            return -1;
        }

        public GameSystem GetGameSystemById(int id)
        {
            return _db.GameSystems.SingleOrDefault(u => u.Id == id);
        }

        public void DeleteGameSystem(int id)
        {
            try
            {
                var gameSystem = GetGameSystemById(id);

                if (gameSystem != null)
                {
                    foreach (var article in GetArticlesByGameSystemId(id))
                    {
                        _db.Articles.Remove(article);
                    }
                    _db.SaveChanges();

                    foreach (var url in GetUrlsByGameSystemId(id))
                    {
                        _db.InfoSourceRssUrls.Remove(url);
                    }
                    _db.SaveChanges();

                    foreach (var gameInfo in GetGameInfoByGameSystemId(id))
                    {
                        _db.GameInfoes.Remove(gameInfo);
                    }
                    _db.SaveChanges();

                    var sortOrder = _db.GameSystemSortOrders.SingleOrDefault(u => u.GameSystem.Id == id);
                    if (sortOrder != null)
                    {
                        _db.GameSystemSortOrders.Remove(sortOrder);
                        _db.SaveChanges();
                    }

                    _db.GameSystems.Remove(gameSystem);
                    _db.SaveChanges();
                }
            }
            catch
            {
                // Do Nothing
            }
        }

        public IEnumerable<GameSystemSortOrder> GetGameSystemSortOrder()
        {
            return _db.GameSystemSortOrders;
        }

        public void UpdateOrder(GameSystemSortOrder order)
        {
            var dbOrder = (from t in _db.GameSystemSortOrders
                                           where t.Id == order.Id
                                           select t).SingleOrDefault();

            dbOrder.SortOrder = order.SortOrder;

            _db.SaveChanges();
        }

        #region Private methods

        private GameSystem GetGameSystemByGameSystemName(string gameSystemName)
        {
            return _db.GameSystems.SingleOrDefault(u => u.GameSystemName == gameSystemName);
        }

        private IEnumerable<Articles> GetArticlesByGameSystemId(int gameSystemId)
        {
            return _db.Articles.Where(u => u.GameSystemId == gameSystemId);
        }

        private IEnumerable<InfoSourceRssUrls> GetUrlsByGameSystemId(int gameSystemId)
        {
            return _db.InfoSourceRssUrls.Where(u => u.GameSystemId == gameSystemId);
        }

        private IEnumerable<GameInfo> GetGameInfoByGameSystemId(int gameSystemId)
        {
            return _db.GameInfoes.Where(u => u.GameSystemId == gameSystemId);
        }

        #endregion
    }
}