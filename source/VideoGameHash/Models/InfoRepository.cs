using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QDFeedParser;
using VideoGameHash.Helpers;

namespace VideoGameHash.Models
{
    public class InfoRepository
    {
        private VGHDatabaseContainer db = new VGHDatabaseContainer();

        public IEnumerable<InfoType> GetInfoTypes()
        {
            return db.InfoTypes;
        }

        public InfoType GetInfoType(int Id)
        {
            return db.InfoTypes.SingleOrDefault(u => u.Id == Id);
        }

        public string GetInfoTypeName(int id)
        {
            try
            {
                return db.InfoTypes.SingleOrDefault(u => u.Id == id).InfoTypeName;
            }
            catch
            {
                return String.Empty;
            }
        }

        public InfoType GetInfoType(string name)
        {
            return db.InfoTypes.SingleOrDefault(u => u.InfoTypeName == name);
        }

        public IEnumerable<InfoSource> GetSources()
        {
            return db.InfoSources;
        }

        public IEnumerable<InfoSource> GetSources(int section)
        {
            return db.InfoSources;
        }

        public InfoSource GetInfoSource(int Id)
        {
            return db.InfoSources.SingleOrDefault(u => u.Id == Id);
        }

        public string GetInfoSourceName(int Id)
        {
            try
            {
                return db.InfoSources.SingleOrDefault(u => u.Id == Id).InfoSourceName;
            }
            catch
            {
                return String.Empty;
            }
        }

        public InfoSource GetInfoSource(string name)
        {
            return db.InfoSources.SingleOrDefault(u => u.InfoSourceName == name);
        }

        public IEnumerable<InfoSourceRssUrls> GetRssUrls()
        {
            return db.InfoSourceRssUrls;
        }

        public InfoSourceRssUrls GetRssUrl(int Id)
        {
            return db.InfoSourceRssUrls.SingleOrDefault(u => u.Id == Id);
        }

        public IEnumerable<FeaturedArticles> GetFeaturedArticles()
        {
            return db.FeaturedArticles;
        }

        public IEnumerable<FeaturedArticles> GetFeaturedArticles(int sectionId)
        {
            return db.FeaturedArticles.Where(u => u.Article.InfoTypeId == sectionId);
        }

        public IEnumerable<TrendingGames> GetTrendingGames()
        {
            return db.TrendingGames;
        }

        public IEnumerable<TrendingGames> GetTrendingGames(int sectionId)
        {
            return db.TrendingGames.Where(u => u.InfoTypeId == sectionId);
        }

        public IEnumerable<TrendingArticles> GetTrendingArticles(int sectionId)
        {
            return db.TrendingArticles.Where(u => u.Article.InfoTypeId == sectionId);
        }

        public IEnumerable<TrendingArticles> GetTrendingArticles(int sectionId, int trendingGameId)
        {
            return db.TrendingArticles.Where(u => u.Article.InfoTypeId == sectionId && u.TrendingGamesId == trendingGameId).GroupBy(u => u.Article.Title).Select(u => u.FirstOrDefault()).OrderBy(u => u.Article.InfoSourceId).ThenByDescending(u => u.Article.DatePublished);
        }

        public IEnumerable<TrendingArticles> GetTrendingArticles(int sectionId, int trendingGameId, int gameSystemId)
        {
            return db.TrendingArticles.Where(u => u.Article.InfoTypeId == sectionId && u.TrendingGamesId == trendingGameId && (u.Article.GameSystemId == gameSystemId || u.Article.GameSystem.GameSystemName == "All")).OrderBy(u => u.Article.InfoSourceId).ThenByDescending(u => u.Article.DatePublished);
        }

        public void AddInfoType(AddInfoModel model)
        {
            try
            {
                InfoType infoType = new InfoType();
                infoType.InfoTypeName = model.Name;
                infoType.UseGameSystem = true;
                db.InfoTypes.AddObject(infoType);
                db.SaveChanges();

                int? maxValue = db.InfoTypeSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
                InfoTypeSortOrder order = new InfoTypeSortOrder();
                infoType = GetInfoType(model.Name);
                order.Id = infoType.Id;
                order.InfoType = infoType;
                order.SortOrder = maxValue + 1 ?? 1;

                db.InfoTypeSortOrders.AddObject(order);
                db.SaveChanges();
            }
            catch
            {
                // Do nothing
            }
        }

        public void AddInfoSource(AddInfoModel model)
        {
            try
            {
                InfoSource infoSource = new InfoSource();
                infoSource.InfoSourceName = model.Name;
                db.InfoSources.AddObject(infoSource);
                db.SaveChanges();

                int? maxValue = db.InfoSourceSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
                InfoSourceSortOrder order = new InfoSourceSortOrder();
                infoSource = GetInfoSource(model.Name);
                order.Id = infoSource.Id;
                order.InfoSource = infoSource;
                order.SortOrder = maxValue + 1 ?? 1;

                db.InfoSourceSortOrders.AddObject(order);
                db.SaveChanges();
            }
            catch
            {
                // Do nothing
            }
        }

        public void AddUrl(AddUrlModel model)
        {
            try
            {
                InfoSourceRssUrls url = new InfoSourceRssUrls();
                url.InfoTypeId = GetInfoTypeId(model.Section);
                url.InfoSourceId = GetInfoSourceId(model.Source);
                url.GameSystemId = GetGameSystemId(model.GameSystem);
                url.URL = model.Url;

                db.InfoSourceRssUrls.AddObject(url);
                db.SaveChanges();
            }
            catch
            {
                // Do Nothing
            }
        }

        public void EditInfo(string section, EditModel model)
        {
            try
            {
                if (section == "Source")
                {
                    InfoSource infoSource = GetInfoSource(model.Id);
                    infoSource.InfoSourceName = model.Name;
                }
                else // section == "Url"
                {
                    InfoSourceRssUrls url = GetRssUrl(model.Id);
                    url.URL = model.Name;
                }

                db.SaveChanges();
            }
            catch
            {
                // Do nothing
            }
        }

        public void EditSectionInfo(EditSectionModel model)
        {
            try
            {
                InfoType infoType = GetInfoType(model.Id);
                infoType.InfoTypeName = model.Name;
                infoType.UseGameSystem = model.UseGameSystem;
                db.SaveChanges();
            }
            catch
            {
                // Do nothing
            }
        }

        public int GetInfoTypeId(string infoType)
        {
            return db.InfoTypes.SingleOrDefault(u => u.InfoTypeName == infoType).Id;
        }

        public int GetInfoSourceId(string source)
        {
            return db.InfoSources.SingleOrDefault(u => u.InfoSourceName == source).Id;
        }

        public int GetGameSystemId(string gameSystem)
        {
            return db.GameSystems.SingleOrDefault(u => u.GameSystemName == gameSystem).Id;
        }

        internal int GetNumSourceEntriesByGameSystem(int Section, int Source, int GameSystem)
        {
            int count = (from tempDb in db.Articles
                         where tempDb.InfoTypeId == Section && tempDb.InfoSourceId == Source && tempDb.GameSystemId == GameSystem
                         select tempDb).Count();

            return count;
        }

        internal int GetNumEntriesBySectionAndSource(int Section, string source)
        {
            int sourceId = GetInfoSourceId(source);
            int count = (from tempDb in db.Articles
                         where tempDb.InfoTypeId == Section && tempDb.InfoSourceId == sourceId
                         select tempDb).Count();

            return count;
        }

        internal int GetNumSourceEntriesByGameSystem(int section, int gameSystem)
        {
            int count = (from tempDb in db.Articles
                         where tempDb.InfoTypeId == section && tempDb.GameSystemId == gameSystem
                         select tempDb).Count();

            return count;
        }

        public IQueryable<Articles> GetArticles(int section)
        {
            return db.Articles.Where(u => u.InfoTypeId == section);
        }

        public IQueryable<Articles> GetArticles(int section, int gameSystem)
        {
            return db.Articles.Where(u => u.InfoTypeId == section && u.GameSystemId == gameSystem);
        }

        public IQueryable<Articles> GetArticles(int Section, int Source, int GameSystem)
        {
            if (Source < 0 && GameSystem == GetGameSystemId("All"))
                return GetArticles(Section).OrderByDescending(d => d.DatePublished).Take(210);
            else
            {
                if (Source < 0)
                    return GetArticles(Section).Where(u => u.GameSystemId == GameSystem).OrderByDescending(u => u.DatePublished).Take(210);
                else if (GameSystem == GetGameSystemId("All"))
                    return GetArticles(Section).Where(u => u.InfoSourceId == Source).OrderByDescending(u => u.DatePublished).Take(210);
                else
                    return GetArticles(Section).Where(u => u.InfoSourceId == Source && u.GameSystemId == GameSystem).OrderByDescending(u => u.DatePublished).Take(210);
            }
        }

        public IQueryable<Articles> GetArticles(int Section, int Source, int GameSystem, string Search)
        {
            if (Source < 0 && GameSystem == GetGameSystemId("All"))
                return GetArticles(Section).Where(u => u.Title.Contains(Search)).OrderByDescending(d => d.DatePublished).Take(210);
            else
            {
                if (Source < 0)
                    return GetArticles(Section).Where(u => u.GameSystemId == GameSystem && u.Title.Contains(Search)).OrderByDescending(u => u.DatePublished).Take(210);
                else if (GameSystem == GetGameSystemId("All"))
                    return GetArticles(Section).Where(u => u.InfoSourceId == Source && u.Title.Contains(Search)).OrderByDescending(u => u.DatePublished).Take(210);
                else
                    return GetArticles(Section).Where(u => u.InfoSourceId == Source && u.GameSystemId == GameSystem && u.Title.Contains(Search)).OrderByDescending(u => u.DatePublished).Take(210);
            }
        }

        public IEnumerable<Articles> GetGameArticles(int section, string gameTitle)
        {
            try
            {
                return db.Articles.Where(u => u.InfoTypeId == section && u.Title.Contains(gameTitle)).GroupBy(u => u.Title).Select(u => u.FirstOrDefault()).OrderByDescending(u => u.DatePublished);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<Articles> GetTrendingGameArticles(int section, string gameTitle)
        {
            try
            {
                if (section != 2)
                    return db.Articles.Where(u => u.InfoTypeId == section && u.Title.ToUpper().Contains(gameTitle.ToUpper())).GroupBy(u => u.Title).Select(u => u.FirstOrDefault()).OrderByDescending(u => u.DatePublished);
                else
                    return db.Articles.Where(u => u.InfoTypeId == section && u.Title.ToUpper().Contains(gameTitle.ToUpper())).OrderByDescending(u => u.DatePublished);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<Articles> GetGameArticles(int section, string gameTitle, string gameSystem)
        {
            int gameSystemId = GetGameSystemId(gameSystem);
            try
            {
                return db.Articles.Where(u => u.InfoTypeId == section && u.Title.Contains(gameTitle) && u.GameSystemId == gameSystemId).OrderByDescending(u => u.DatePublished);
            }
            catch
            {
                return null;
            }
        }

        public bool ContainsArticles(string gameTitle)
        {
            var temp = from tempArticles in db.Articles
                       where tempArticles.Title.Contains(gameTitle)
                       select tempArticles;
            bool success;
            try
            {
                System.Text.RegularExpressions.Regex searchTerm =
                    new System.Text.RegularExpressions.Regex(@"\b" + gameTitle + @"\b", System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                var matched = temp.AsEnumerable().Where(d => searchTerm.IsMatch(d.Title)).ToList();

                success = matched != null && matched.Count() > 0;
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public bool ContainsArticles(string gameTitle, string gameSystem)
        {
            int gameSystemId = GetGameSystemId(gameSystem);

            var temp = from tempArticles in db.Articles
                       where tempArticles.Title.Contains(gameTitle) && tempArticles.GameSystemId == gameSystemId
                       select tempArticles;
            return temp != null && temp.Count() > 0;
        }

        public bool ContainsArticles(int section, string gameTitle, string gameSystem)
        {
            int gameSystemId = GetGameSystemId(gameSystem);

            var temp = from tempArticles in db.Articles
                       where tempArticles.Title.Contains(gameTitle) && tempArticles.GameSystemId == gameSystemId && tempArticles.InfoTypeId == section
                       select tempArticles;
            return temp != null && temp.Count() > 0;
        }

        public IEnumerable<InfoTypeSortOrder> GetInfoTypeSortOrder()
        {
            return db.InfoTypeSortOrders;
        }

        public IEnumerable<InfoSourceSortOrder> GetInfoSourceSortOrder()
        {
            return db.InfoSourceSortOrders;
        }

        public void UpdateOrder(InfoTypeSortOrder order)
        {
            InfoTypeSortOrder dbOrder = (from t in db.InfoTypeSortOrders
                                       where t.Id == order.Id
                                       select t).SingleOrDefault();

            dbOrder.SortOrder = order.SortOrder;

            db.SaveChanges();
        }

        public void UpdateOrder(InfoSourceSortOrder order)
        {
            InfoSourceSortOrder dbOrder = (from t in db.InfoSourceSortOrders
                                         where t.Id == order.Id
                                         select t).SingleOrDefault();

            dbOrder.SortOrder = order.SortOrder;

            db.SaveChanges();
        }

        public bool UseGameSystem(string section)
        {
            return db.InfoTypes.SingleOrDefault(u => u.InfoTypeName == section).UseGameSystem;
        }

        public int AddRssFeed(InfoSourceRssUrls model)
        {
            InfoSourceRssUrls rssUrl = GetRssUrl(model.Id);
            int i = 0;

            if (rssUrl != null)
            {
                try
                {
                    Uri feedUri = new Uri(rssUrl.URL);
                    IFeedFactory feedFactory = new HttpFeedFactory();
                    IFeed feed = feedFactory.CreateFeed(feedUri);

                    if (feed.Items.Count > 0)
                    {
                        foreach (BaseFeedItem item in feed.Items)
                        {
                            Articles article = new Articles();

                            article.InfoTypeId = rssUrl.InfoTypeId;
                            article.InfoSourceId = rssUrl.InfoSourceId;
                            article.GameSystemId = rssUrl.GameSystemId;
                            article.Content = item.Content;
                            article.DatePublished = item.DatePublished;
                            article.Link = item.Link;
                            article.Title = item.Title;

                            if (!IsDuplicateArticle(article) && SourceSpecificBypass(model.InfoTypeId, model.InfoSourceId, article))
                            {
                                db.Articles.AddObject(article);
                                db.SaveChanges();
                                i++;
                            }
                            else
                                break;
                        }
                    }
                }
                catch
                {
                    i = 0;
                }
            }

            return i;
        }

        private bool SourceSpecificBypass(int infoTypeId, int infoSourceId, Articles article)
        {
            bool byPass = true;

            if (GetInfoSourceName(infoSourceId) == "N4G")
            {
                string title = article.Title.ToUpper();
                
                if (GetInfoTypeName(infoTypeId) == "Reviews")
                {
                    if (title.Contains("PREVIEW") || !title.Contains("REVIEW"))
                        byPass = false;
                }
                else if (GetInfoTypeName(infoTypeId) == "News")
                {
                    if (!title.Contains("PREVIEW") && title.Contains("REVIEW"))
                        byPass = false;
                }
            }

            return byPass;
        }

        public int AddFeedItems(int SectionId)
        {
            int i = 0;

            IList<InfoSourceRssUrls> RssList = GetRssUrls(SectionId).ToList();
            foreach (InfoSourceRssUrls model in RssList)
            {
                i += AddRssFeed(model);
            }

            return i;
        }

        public int AddFeedItems()
        {
            int i = 0;

            IList<InfoSourceRssUrls> RssList = GetRssUrls().ToList();
            foreach (InfoSourceRssUrls model in RssList)
            {
                i += AddRssFeed(model);
            }

            return i;
        }

        public IEnumerable<InfoSourceRssUrls> GetRssUrls(int SectionId)
        {
            return db.InfoSourceRssUrls.Where(u => u.InfoTypeId == SectionId);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="article"></param>
        /// <returns></returns>
        public bool IsDuplicateArticle(Articles article)
        {
            try
            {
                return db.Articles.Where(u => article.InfoTypeId == u.InfoTypeId &&
                                              article.InfoSourceId == u.InfoSourceId &&
                                              article.GameSystemId == u.GameSystemId &&
                                              article.Title == u.Title &&
                                              article.Link == u.Link).Count() > 0;
            }
            catch (Exception ex)
            {
                LogError(ex);
                return false;
            }
        }

        private void LogError(Exception ex)
        {
            throw new NotImplementedException();
        }

        public void DeleteOldArticles()
        {
            try
            {
                DateTime cutoff = DateTime.Now.AddDays(-180);

                List<Articles> articlesToDelete = db.Articles.Where(u => u.DatePublished < cutoff).ToList();

                foreach (Articles article in articlesToDelete)
                {
                    foreach (TrendingArticles trendingArticles in db.TrendingArticles.ToList())
                    {
                        if (trendingArticles.ArticlesId == article.Id)
                        {
                            db.TrendingArticles.DeleteObject(trendingArticles);
                            db.SaveChanges();
                        }
                    }

                    foreach (FeaturedArticles featuredArticle in db.FeaturedArticles.ToList())
                    {
                        if (featuredArticle.Article.Id == article.Id)
                        {
                            db.FeaturedArticles.DeleteObject(featuredArticle);
                            db.SaveChanges();
                        }
                    }

                    db.Articles.DeleteObject(article);
                    db.SaveChanges();
                }

                db.SaveChanges();

                foreach (Games game in db.Games.ToList())
                {
                    List<Articles> gameArticles = db.Articles.Where(u => u.Title.ToUpper().Contains(game.GameTitle.ToUpper())).ToList();

                    if (gameArticles.Count() == 0)
                    {
                        IEnumerable<GameInfo> gameInfoes = db.GameInfoes.Where(u => u.GamesId == game.Id);

                        foreach (GameInfo gameInfo in gameInfoes.ToList())
                        {
                            db.GameInfoes.DeleteObject(gameInfo);
                        }
                        db.SaveChanges();

                        db.Games.DeleteObject(game);
                    }
                }

                db.SaveChanges();
            }
            catch
            {

            }
        }

        public void MakeFeatured(int section)
        {
            var index = 0;
            var featuredStoryCount = 0;
            var limitReached = false;
            var featuredArticles = new List<FeaturedClass>();

            var articleDb = GetArticlesWithImagesOnly(section).GroupBy(u => u.Title).Select(u => u.FirstOrDefault()).OrderByDescending(d => d.DatePublished).ToList();

            while (!limitReached)
            {
                try
                {
                    var articles = articleDb.Skip(index * 50).Take(50).ToList();

                    if (articles.Count() < 50)
                        limitReached = true;

                    foreach (var dbItem in articles)
                    {
                        if (featuredStoryCount > 4)
                        {
                            limitReached = true;
                            break;
                        }

                        string image = GetImage(dbItem.InfoSource.InfoSourceName, dbItem.Content);
                        if (image.Length > 0)
                        {
                            featuredStoryCount++;
                            var featuredClass = new FeaturedClass {Id = dbItem.Id, Article = dbItem, Image = image};
                            featuredArticles.Add(featuredClass);
                        }
                    }
                    index++;
                }
                catch
                {
                    break;
                }
            }

            if (featuredArticles.Any())
            {
                // Delete previous entries for this section
                foreach (FeaturedArticles oldArticle in db.FeaturedArticles.Where(u => u.Article.InfoTypeId == section))
                    db.FeaturedArticles.DeleteObject(oldArticle);

                db.SaveChanges();

                // Add in the new entries
                foreach (FeaturedClass newArticle in featuredArticles)
                {
                    var story = new FeaturedArticles();
                    story.Id = newArticle.Id;
                    story.Article = newArticle.Article;
                    story.ImageLink = newArticle.Image;
                    db.FeaturedArticles.AddObject(story);
                }

                db.SaveChanges();
            }
        }

        public void MakeTrending(int section)
        {
            foreach (TrendingArticles article in db.TrendingArticles.Where(u => u.Article.InfoTypeId == section))
            {
                db.TrendingArticles.DeleteObject(article);
            }

            db.SaveChanges();

            foreach (TrendingGames game in db.TrendingGames.Where(u => u.InfoTypeId == section))
            {
                foreach (TrendingArticles article in db.TrendingArticles.Where(u => u.TrendingGamesId == game.Id))
                    db.TrendingArticles.DeleteObject(article);

                db.TrendingGames.DeleteObject(game);
            }

            db.SaveChanges();

            List<Games> gameList = new List<Games>();
            foreach (Games game in db.Games)
            {
                bool oldGame = false;
                foreach (GameInfo info in game.GameInfoes)
                {
                    if (info != null)
                    {
                        if (info.USReleaseDate < DateTime.Now.AddDays(-31))
                        {
                            oldGame = true;
                            break;
                        }
                        if (!oldGame)
                        {
                            gameList.Add(game);
                        }
                    }
                }
            }

            Dictionary<Games, int> gameInfos = new Dictionary<Games, int>();

            int cutoffDays = (section == 1) ? -7 : -28;
            DateTime cutoff = DateTime.Now.AddDays(cutoffDays);
            
            foreach (Games game in gameList)
            {
                List<Articles> recentArticles = GetTrendingGameArticles(section, game.GameTitle).Where(u => u.DatePublished >= cutoff).ToList();
                int count = recentArticles.Count();
                foreach (Articles article in recentArticles)
                {
                    if (article.Title.ToUpper().Contains(game.GameTitle.ToUpper()))
                    {
                        if (gameInfos.ContainsKey(game))
                        {
                            gameInfos[game]++;
                        }
                        else
                        {
                            gameInfos.Add(game, 0);
                        }
                    }
                }
            }

            gameInfos = gameInfos.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            int counter = 0;
            foreach (Games game in gameInfos.Keys)
            {
                if (counter > 5)
                    break;

                if (gameInfos[game] < 5)
                    continue;

                TrendingGames trendingGame = new TrendingGames();
                trendingGame.InfoTypeId = section;
                trendingGame.GamesId = game.Id;

                db.TrendingGames.AddObject(trendingGame);

                counter++;
            }
            db.SaveChanges();

            foreach (TrendingGames game in db.TrendingGames)
            {
                string gameTitle = GetGameTitle(game.GamesId).ToUpper();
                List<Articles> articleList = GetArticles(section).Where(u => u.Title.ToUpper().Contains(gameTitle.ToUpper())).OrderByDescending(u => u.DatePublished).ToList();

                foreach (Articles article in articleList)
                {
                    TrendingArticles trendingArticle = new TrendingArticles();
                    trendingArticle.TrendingGamesId = game.Id;
                    trendingArticle.ArticlesId = article.Id;

                    db.TrendingArticles.AddObject(trendingArticle);
                }
            }
            db.SaveChanges();
        }

        public IEnumerable<Articles> GetArticlesWithImagesOnly(int section)
        {
            var temp = (from tempDb in db.Articles
                        where tempDb.InfoTypeId == section &&
                              (tempDb.InfoSourceId != 1 /*IGN*/ && tempDb.InfoSourceId != 2 /*Gamespot*/ && tempDb.InfoSourceId != 5 /*CGV*/ && tempDb.InfoSourceId != 8 /*N4G*/)
                        select tempDb);

            return temp;
        }

        public string GetImage(string source, string content)
        {
            int imageIndex = content.IndexOf("img");
            string image = "";
            // Add companies that have images, but are not display-able
            if (!NewsHelper.BadImageCompany(source))
            {
                if (imageIndex > 0)
                {
                    // Get the dimensions of the image
                    int srcIndex = content.IndexOf("src=\"", imageIndex);
                    if (srcIndex > 0)
                    {
                        int srcEndIndex = content.IndexOf("\"", srcIndex + 5);

                        if (srcEndIndex > 0)
                        {
                            image = content.Substring(srcIndex + 5, srcEndIndex - (srcIndex + 5));
                        }
                    }

                    // Add company hardcodes here
                }
            }

            return image;
        }

        private string GetGameTitle(int gameId)
        {
            return db.Games.SingleOrDefault(u => u.Id == gameId).GameTitle;
        }

        public string GetUrl(int sectionId, int sourceId, int gameSystemId)
        {
            try
            {
                return db.InfoSourceRssUrls.SingleOrDefault(u => u.InfoTypeId == sectionId && u.InfoSourceId == sourceId && u.GameSystemId == gameSystemId).URL;
            }
            catch
            {
                return String.Empty;
            }
        }

        public string GetGameSystemName(int gameSystemId)
        {
            try
            {
                return db.GameSystems.SingleOrDefault(u => u.Id == gameSystemId).GameSystemName;
            }
            catch
            {
                return String.Empty;
            }
        }

        public bool HasArticles(int sectionId, int gameSystemId)
        {
            try
            {
                var count = from tempArticle in db.Articles
                            where tempArticle.InfoTypeId == sectionId &&
                                  tempArticle.GameSystemId == gameSystemId
                            select tempArticle;

                return count != null && count.Count() > 0;
            }
            catch
            {
                return false;
            }
        }

        internal void DeleteInfoType(int id)
        {
            try
            {
                InfoType infoType = GetInfoType(id);

                foreach (Articles article in GetArticles(id))
                {
                    FeaturedArticles featured = db.FeaturedArticles.SingleOrDefault(u => u.Id == article.Id);
                    if (featured != null)
                    {
                        db.FeaturedArticles.DeleteObject(featured);
                    }

                    db.Articles.DeleteObject(article);
                }
                db.SaveChanges();

                foreach (InfoSourceRssUrls url in GetUrlsByInfoType(id))
                {
                    db.InfoSourceRssUrls.DeleteObject(url);
                }
                db.SaveChanges();

                InfoTypeSortOrder sortOrder = db.InfoTypeSortOrders.SingleOrDefault(u => u.InfoType.Id == id);
                if (sortOrder != null)
                {
                    db.InfoTypeSortOrders.DeleteObject(sortOrder);
                    db.SaveChanges();
                }
                
                db.InfoTypes.DeleteObject(infoType);
                db.SaveChanges();
            }
            catch
            {
                // Do Nothing
            }
        }

        private IEnumerable<InfoSourceRssUrls> GetUrlsByInfoType(int infoTypeId)
        {
            return db.InfoSourceRssUrls.Where(u => u.InfoTypeId == infoTypeId);
        }

        internal void DeleteInfoSource(int id)
        {
            try
            {
                InfoSource infoSource = GetInfoSource(id);

                foreach (Articles article in GetArticlesBySourceId(id))
                {
                    FeaturedArticles featured = db.FeaturedArticles.SingleOrDefault(u => u.Id == article.Id);
                    if (featured != null)
                    {
                        db.FeaturedArticles.DeleteObject(featured);
                    }
                    db.Articles.DeleteObject(article);
                }

                foreach (InfoSourceRssUrls url in GetUrlsBySourceId(id))
                {
                    db.InfoSourceRssUrls.DeleteObject(url);
                }
                db.SaveChanges();

                InfoSourceSortOrder sortOrder = db.InfoSourceSortOrders.SingleOrDefault(u => u.InfoSource.Id == id);
                if (sortOrder != null)
                {
                    db.InfoSourceSortOrders.DeleteObject(sortOrder);
                    db.SaveChanges();
                }

                db.InfoSources.DeleteObject(infoSource);
                db.SaveChanges();
            }
            catch
            {
                // Do Nothing
            }
        }

        private IEnumerable<InfoSourceRssUrls> GetUrlsBySourceId(int sourceId)
        {
            return db.InfoSourceRssUrls.Where(u => u.InfoSourceId == sourceId);
        }

        private IEnumerable<Articles> GetArticlesBySourceId(int id)
        {
            return db.Articles.Where(u => u.InfoSourceId == id);
        }

        internal void AddPoll(AddPollModel model)
        {
            Poll poll = new Poll();
            

            poll.Title = model.Title;
            db.Polls.AddObject(poll);

            db.SaveChanges();

            string[] items = model.Answers.Split('\n');

            foreach (string item in items)
            {
                PollAnswers answers = new PollAnswers();
                answers.PollId = poll.Id;

                answers.Answer = item.TrimEnd('\r');
                answers.NumVotes = 0;

                db.PollAnswers.AddObject(answers);
            }

            db.SaveChanges();
        }

        internal IEnumerable<Poll> GetPolls()
        {
            return db.Polls;
        }

        public Poll GetPoll(int Id)
        {
            return db.Polls.SingleOrDefault(u => u.Id == Id);
        }

        internal void UpdatePoll(int PollId, int PollValue)
        {
            Poll poll = GetPoll(PollId);

            foreach (PollAnswers answer in poll.PollAnswers)
            {
                if (answer.Id == PollValue)
                {
                    answer.NumVotes++;
                }
            }

            db.SaveChanges();
        }

        internal void DeleteUrl(int id)
        {
            try
            {
                InfoSourceRssUrls url = GetRssUrl(id);

                db.InfoSourceRssUrls.DeleteObject(url);
                db.SaveChanges();
            }
            catch
            {

            }
        }
    }
}