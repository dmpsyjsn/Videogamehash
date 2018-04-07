using System.Collections.Generic;
using System.Linq;
using VideoGameHash.Models;

namespace VideoGameHash.Repositories
{
    public class GameSystemsRepository
    {
        private readonly VGHDatabaseContainer _db = new VGHDatabaseContainer();

        public IEnumerable<GameSystem> GetGameSystems()
        {
            return _db.GameSystems.OrderBy(u => u.GameSystemSortOrder.SortOrder);
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
                _db.GameSystems.AddObject(gameSystem);
                _db.SaveChanges();

                int? maxValue = _db.GameSystemSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
                var order = new GameSystemSortOrder();
                gameSystem = GetGameSystemByGameSystemName(gameSystemName);
                order.Id = gameSystem.Id;
                order.GameSystem = gameSystem;
                order.SortOrder = maxValue + 1 ?? 1;

                _db.GameSystemSortOrders.AddObject(order);
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

        public GameSystem GetGameSystemByGameSystemName(string gameSystemName)
        {
            return _db.GameSystems.SingleOrDefault(u => u.GameSystemName == gameSystemName);
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
                        _db.Articles.DeleteObject(article);
                    }
                    _db.SaveChanges();

                    foreach (var url in GetUrlsByGameSystemId(id))
                    {
                        _db.InfoSourceRssUrls.DeleteObject(url);
                    }
                    _db.SaveChanges();

                    foreach (var gameInfo in GetGameInfoByGameSystemId(id))
                    {
                        _db.GameInfoes.DeleteObject(gameInfo);
                    }
                    _db.SaveChanges();

                    var sortOrder = _db.GameSystemSortOrders.SingleOrDefault(u => u.GameSystem.Id == id);
                    if (sortOrder != null)
                    {
                        _db.GameSystemSortOrders.DeleteObject(sortOrder);
                        _db.SaveChanges();
                    }

                    _db.GameSystems.DeleteObject(gameSystem);
                    _db.SaveChanges();
                }
            }
            catch
            {
                // Do Nothing
            }
        }

        private IEnumerable<GameInfo> GetGameInfoByGameSystemId(int gameSystemId)
        {
            return _db.GameInfoes.Where(u => u.GameSystemId == gameSystemId);
        }

        public IEnumerable<GameSystemSortOrder> GetGameSystemSortOrder()
        {
            return _db.GameSystemSortOrders;
        }

        internal void UpdateOrder(GameSystemSortOrder order)
        {
            var dbOrder = (from t in _db.GameSystemSortOrders
                                           where t.Id == order.Id
                                           select t).SingleOrDefault();

            dbOrder.SortOrder = order.SortOrder;

            _db.SaveChanges();
        }

        private IEnumerable<Articles> GetArticlesByGameSystemId(int gameSystemId)
        {
            return _db.Articles.Where(u => u.GameSystemId == gameSystemId);
        }

        private IEnumerable<InfoSourceRssUrls> GetUrlsByGameSystemId(int gameSystemId)
        {
            return _db.InfoSourceRssUrls.Where(u => u.GameSystemId == gameSystemId);
        }
    }
}