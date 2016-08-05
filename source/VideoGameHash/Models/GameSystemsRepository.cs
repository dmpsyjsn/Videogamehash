using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VideoGameHash.Models
{
    public class GameSystemsRepository
    {
        private VGHDatabaseContainer db = new VGHDatabaseContainer();

        public IEnumerable<GameSystem> GetGameSystems()
        {
            return db.GameSystems.OrderBy(u => u.GameSystemSortOrder.SortOrder);
        }

        public void AddGameSystem(GameSystemModel model)
        {
            GameSystem gameSystem = new GameSystem();
            gameSystem.GameSystemName = model.GameSystem;
            db.GameSystems.AddObject(gameSystem);
            db.SaveChanges();

            int? maxValue = db.GameSystemSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
            GameSystemSortOrder order = new GameSystemSortOrder();
            gameSystem = GetGameSystemByGameSystemName(model.GameSystem);
            order.Id = gameSystem.Id;
            order.GameSystem = gameSystem;
            order.SortOrder = maxValue + 1 ?? 1;

            db.GameSystemSortOrders.AddObject(order);

            db.SaveChanges();
        }

        public GameSystem GetGameSystemById(int Id)
        {
            return db.GameSystems.SingleOrDefault(u => u.Id == Id);
        }

        public GameSystem GetGameSystemByGameSystemName(string GameSystemName)
        {
            return db.GameSystems.SingleOrDefault(u => u.GameSystemName == GameSystemName);
        }

        public void DeleteGameSystem(int Id)
        {
            try
            {
                GameSystem gameSystem = GetGameSystemById(Id);

                if (gameSystem != null)
                {
                    foreach (Articles article in GetArticlesByGameSystemId(Id))
                    {
                        FeaturedArticles featured = db.FeaturedArticles.SingleOrDefault(u => u.Id == article.Id);
                        if (featured != null)
                        {
                            db.FeaturedArticles.DeleteObject(featured);
                        }
                        db.Articles.DeleteObject(article);
                    }
                    db.SaveChanges();

                    foreach (InfoSourceRssUrls url in GetUrlsByGameSystemId(Id))
                    {
                        db.InfoSourceRssUrls.DeleteObject(url);
                    }
                    db.SaveChanges();

                    foreach (GameInfo gameInfo in GetGameInfoByGameSystemId(Id))
                    {
                        db.GameInfoes.DeleteObject(gameInfo);
                    }
                    db.SaveChanges();

                    GameSystemSortOrder sortOrder = db.GameSystemSortOrders.SingleOrDefault(u => u.GameSystem.Id == Id);
                    if (sortOrder != null)
                    {
                        db.GameSystemSortOrders.DeleteObject(sortOrder);
                        db.SaveChanges();
                    }

                    db.GameSystems.DeleteObject(gameSystem);
                    db.SaveChanges();
                }
            }
            catch
            {
                // Do Nothing
            }
        }

        private IEnumerable<GameInfo> GetGameInfoByGameSystemId(int gameSystemId)
        {
            return db.GameInfoes.Where(u => u.GameSystemId == gameSystemId);
        }

        public IEnumerable<GameSystemSortOrder> GetGameSystemSortOrder()
        {
            return db.GameSystemSortOrders;
        }

        internal void UpdateOrder(GameSystemSortOrder order)
        {
            GameSystemSortOrder dbOrder = (from t in db.GameSystemSortOrders
                                           where t.Id == order.Id
                                           select t).SingleOrDefault();

            dbOrder.SortOrder = order.SortOrder;

            db.SaveChanges();
        }

        private IEnumerable<Articles> GetArticlesByGameSystemId(int gameSystemId)
        {
            return db.Articles.Where(u => u.GameSystemId == gameSystemId);
        }

        private IEnumerable<InfoSourceRssUrls> GetUrlsByGameSystemId(int gameSystemId)
        {
            return db.InfoSourceRssUrls.Where(u => u.GameSystemId == gameSystemId);
        }
    }
}