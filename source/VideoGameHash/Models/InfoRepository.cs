using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
using QDFeedParser;
using VideoGameHash.Helpers;

namespace VideoGameHash.Models
{
    public class InfoRepository
    {
        private VGHDatabaseContainer _db = new VGHDatabaseContainer();

        public IEnumerable<InfoType> GetInfoTypes()
        {
            return _db.InfoTypes;
        }

        public InfoType GetInfoType(int id)
        {
            return _db.InfoTypes.SingleOrDefault(u => u.Id == id);
        }

        public string GetInfoTypeName(int id)
        {
            try
            {
                return _db.InfoTypes.SingleOrDefault(u => u.Id == id)?.InfoTypeName ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }

        public InfoType GetInfoType(string name)
        {
            return _db.InfoTypes.SingleOrDefault(u => u.InfoTypeName == name);
        }

        public IEnumerable<InfoSource> GetSources()
        {
            return _db.InfoSources;
        }

        public IEnumerable<InfoSource> GetSources(int section)
        {
            return _db.InfoSources;
        }

        public InfoSource GetInfoSource(int id)
        {
            return _db.InfoSources.SingleOrDefault(u => u.Id == id);
        }

        public string GetInfoSourceName(int id)
        {
            try
            {
                return _db.InfoSources.SingleOrDefault(u => u.Id == id)?.InfoSourceName;
            }
            catch
            {
                return String.Empty;
            }
        }

        public InfoSource GetInfoSource(string name)
        {
            return _db.InfoSources.SingleOrDefault(u => u.InfoSourceName == name);
        }

        public IEnumerable<InfoSourceRssUrls> GetRssUrls()
        {
            return _db.InfoSourceRssUrls;
        }

        public InfoSourceRssUrls GetRssUrl(int id)
        {
            return _db.InfoSourceRssUrls.SingleOrDefault(u => u.Id == id);
        }

        public IEnumerable<FeaturedArticles> GetFeaturedArticles()
        {
            return _db.FeaturedArticles;
        }

        public IEnumerable<FeaturedArticles> GetFeaturedArticles(int sectionId)
        {
            return _db.FeaturedArticles.Where(u => u.Article.InfoTypeId == sectionId);
        }

        public IEnumerable<TrendingGames> GetTrendingGames()
        {
            return _db.TrendingGames;
        }

        public IEnumerable<TrendingGames> GetTrendingGames(int sectionId)
        {
            return _db.TrendingGames.Where(u => u.InfoTypeId == sectionId);
        }

        public IEnumerable<TrendingArticles> GetTrendingArticles(int sectionId)
        {
            return _db.TrendingArticles.Where(u => u.Article.InfoTypeId == sectionId);
        }

        public IEnumerable<TrendingArticles> GetTrendingArticles(int sectionId, int trendingGameId)
        {
            return _db.TrendingArticles.Where(u => u.Article.InfoTypeId == sectionId && u.TrendingGamesId == trendingGameId).GroupBy(u => u.Article.Title).Select(u => u.FirstOrDefault()).OrderBy(u => u.Article.InfoSourceId).ThenByDescending(u => u.Article.DatePublished);
        }

        public IEnumerable<TrendingArticles> GetTrendingArticles(int sectionId, int trendingGameId, int gameSystemId)
        {
            return _db.TrendingArticles.Where(u => u.Article.InfoTypeId == sectionId && u.TrendingGamesId == trendingGameId && (u.Article.GameSystemId == gameSystemId || u.Article.GameSystem.GameSystemName == "All")).OrderBy(u => u.Article.InfoSourceId).ThenByDescending(u => u.Article.DatePublished);
        }

        public int AddInfoType(string name)
        {
            try
            {
                var type = GetInfoType(name);
                if (type != null) return type.Id;

                var infoType = new InfoType
                {
                    InfoTypeName = name,
                    UseGameSystem = true
                };
                _db.InfoTypes.AddObject(infoType);
                _db.SaveChanges();

                int? maxValue = _db.InfoTypeSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
                var order = new InfoTypeSortOrder();
                infoType = GetInfoType(name);
                order.Id = infoType.Id;
                order.InfoType = infoType;
                order.SortOrder = maxValue + 1 ?? 1;

                _db.InfoTypeSortOrders.AddObject(order);
                _db.SaveChanges();

                return infoType.Id;
            }
            catch
            {
                // Do nothing
            }

            return -1;
        }

        public int AddInfoSource(string name)
        {
            try
            {
                var source = GetInfoSource(name);
                if (source != null) return source.Id;

                var infoSource = new InfoSource
                {
                    InfoSourceName = name
                };
                _db.InfoSources.AddObject(infoSource);
                _db.SaveChanges();

                int? maxValue = _db.InfoSourceSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
                var order = new InfoSourceSortOrder();
                infoSource = GetInfoSource(name);
                order.Id = infoSource.Id;
                order.InfoSource = infoSource;
                order.SortOrder = maxValue + 1 ?? 1;

                _db.InfoSourceSortOrders.AddObject(order);
                _db.SaveChanges();

                return infoSource.Id;
            }
            catch
            {
                // Do nothing
            }

            return -1;
        }

        public void AddUrl(AddUrlModel model)
        {
            try
            {
                var url = new InfoSourceRssUrls
                {
                    InfoTypeId = GetInfoTypeId(model.Section),
                    InfoSourceId = GetInfoSourceId(model.Source),
                    GameSystemId = GetGameSystemId(model.GameSystem),
                    URL = model.Url
                };

                _db.InfoSourceRssUrls.AddObject(url);
                _db.SaveChanges();
            }
            catch
            {
                // Do Nothing
            }
        }

        public void AddUrl(int typeId, int sourceId, int gameSystemId, string url)
        {
            try
            {
                var infoSourceRssUrl = new InfoSourceRssUrls
                {
                    InfoTypeId = typeId,
                    InfoSourceId = sourceId,
                    GameSystemId = gameSystemId,
                    URL = url
                };

                _db.InfoSourceRssUrls.AddObject(infoSourceRssUrl);
                _db.SaveChanges();
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
                    var infoSource = GetInfoSource(model.Id);
                    infoSource.InfoSourceName = model.Name;
                }
                else // section == "Url"
                {
                    var url = GetRssUrl(model.Id);
                    url.URL = model.Name;
                }

                _db.SaveChanges();
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
                var infoType = GetInfoType(model.Id);
                infoType.InfoTypeName = model.Name;
                infoType.UseGameSystem = model.UseGameSystem;
                _db.SaveChanges();
            }
            catch
            {
                // Do nothing
            }
        }

        public int GetInfoTypeId(string infoType)
        {
            return _db.InfoTypes.SingleOrDefault(u => u.InfoTypeName == infoType).Id;
        }

        public int GetInfoSourceId(string source)
        {
            return _db.InfoSources.SingleOrDefault(u => u.InfoSourceName == source).Id;
        }

        public int GetGameSystemId(string gameSystem)
        {
            return _db.GameSystems.SingleOrDefault(u => u.GameSystemName == gameSystem).Id;
        }

        internal int GetNumSourceEntriesByGameSystem(int section, int source, int gameSystem)
        {
            var count = (from tempDb in _db.Articles
                         where tempDb.InfoTypeId == section && tempDb.InfoSourceId == source && tempDb.GameSystemId == gameSystem
                         select tempDb).Count();

            return count;
        }

        internal int GetNumEntriesBySectionAndSource(int section, string source)
        {
            var sourceId = GetInfoSourceId(source);
            var count = (from tempDb in _db.Articles
                         where tempDb.InfoTypeId == section && tempDb.InfoSourceId == sourceId
                         select tempDb).Count();

            return count;
        }

        internal int GetNumSourceEntriesByGameSystem(int section, int gameSystem)
        {
            var count = (from tempDb in _db.Articles
                         where tempDb.InfoTypeId == section && tempDb.GameSystemId == gameSystem
                         select tempDb).Count();

            return count;
        }

        public IQueryable<Articles> GetArticles(int section)
        {
            return _db.Articles.Where(u => u.InfoTypeId == section);
        }

        public IQueryable<Articles> GetArticles(int section, int gameSystem)
        {
            return _db.Articles.Where(u => u.InfoTypeId == section && u.GameSystemId == gameSystem);
        }

        public IQueryable<Articles> GetArticles(int section, int source, int gameSystem)
        {
            if (source < 0 && gameSystem == GetGameSystemId("All"))
                return GetArticles(section).OrderByDescending(d => d.DatePublished).Take(210);
            else
            {
                if (source < 0)
                    return GetArticles(section).Where(u => u.GameSystemId == gameSystem).OrderByDescending(u => u.DatePublished).Take(210);
                else if (gameSystem == GetGameSystemId("All"))
                    return GetArticles(section).Where(u => u.InfoSourceId == source).OrderByDescending(u => u.DatePublished).Take(210);
                else
                    return GetArticles(section).Where(u => u.InfoSourceId == source && u.GameSystemId == gameSystem).OrderByDescending(u => u.DatePublished).Take(210);
            }
        }

        public IQueryable<Articles> GetArticles(int section, int source, int gameSystem, string search)
        {
            if (source < 0 && gameSystem == GetGameSystemId("All"))
                return GetArticles(section).Where(u => u.Title.Contains(search)).OrderByDescending(d => d.DatePublished).Take(210);
            else
            {
                if (source < 0)
                    return GetArticles(section).Where(u => u.GameSystemId == gameSystem && u.Title.Contains(search)).OrderByDescending(u => u.DatePublished).Take(210);
                else if (gameSystem == GetGameSystemId("All"))
                    return GetArticles(section).Where(u => u.InfoSourceId == source && u.Title.Contains(search)).OrderByDescending(u => u.DatePublished).Take(210);
                else
                    return GetArticles(section).Where(u => u.InfoSourceId == source && u.GameSystemId == gameSystem && u.Title.Contains(search)).OrderByDescending(u => u.DatePublished).Take(210);
            }
        }

        public IEnumerable<Articles> GetGameArticles(int section, string gameTitle)
        {
            try
            {
                return _db.Articles.Where(u => u.InfoTypeId == section && u.Title.Contains(gameTitle)).GroupBy(u => u.Title).Select(u => u.FirstOrDefault()).OrderByDescending(u => u.DatePublished);
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
                    return _db.Articles.Where(u => u.InfoTypeId == section && u.Title.ToUpper().Contains(gameTitle.ToUpper())).GroupBy(u => u.Title).Select(u => u.FirstOrDefault()).OrderByDescending(u => u.DatePublished);
                else
                    return _db.Articles.Where(u => u.InfoTypeId == section && u.Title.ToUpper().Contains(gameTitle.ToUpper())).OrderByDescending(u => u.DatePublished);
            }
            catch
            {
                return null;
            }
        }

        public IEnumerable<Articles> GetGameArticles(int section, string gameTitle, string gameSystem)
        {
            var gameSystemId = GetGameSystemId(gameSystem);
            try
            {
                return _db.Articles.Where(u => u.InfoTypeId == section && u.Title.Contains(gameTitle) && u.GameSystemId == gameSystemId).OrderByDescending(u => u.DatePublished);
            }
            catch
            {
                return null;
            }
        }

        public bool ContainsArticles(string gameTitle)
        {
            bool success;
            try
            {
                var searchTerm = new Regex($@"\b{gameTitle}\b", RegexOptions.IgnoreCase);

                var matched = _db.Articles.Where(d => searchTerm.IsMatch(d.Title) || searchTerm.IsMatch(d.Content)).ToList();

                success = matched.Any();
            }
            catch
            {
                success = false;
            }

            return success;
        }

        public bool ContainsArticles(string gameTitle, string gameSystem)
        {
            var gameSystemId = GetGameSystemId(gameSystem);

            var temp = from tempArticles in _db.Articles
                       where tempArticles.Title.Contains(gameTitle) && tempArticles.GameSystemId == gameSystemId
                       select tempArticles;
            return temp.Any();
        }

        public bool ContainsArticles(int section, string gameTitle, string gameSystem)
        {
            var gameSystemId = GetGameSystemId(gameSystem);

            var articles = _db.Articles.Where(x =>
                (x.Title.Contains(gameTitle) || x.Content.Contains(gameTitle)) && x.GameSystemId.Equals(gameSystemId) &&
                x.InfoTypeId.Equals(section)).ToList();

            return articles.Any();
        }

        public IEnumerable<InfoTypeSortOrder> GetInfoTypeSortOrder()
        {
            return _db.InfoTypeSortOrders;
        }

        public IEnumerable<InfoSourceSortOrder> GetInfoSourceSortOrder()
        {
            return _db.InfoSourceSortOrders;
        }

        public void UpdateOrder(InfoTypeSortOrder order)
        {
            var dbOrder = (from t in _db.InfoTypeSortOrders
                                       where t.Id == order.Id
                                       select t).SingleOrDefault();

            dbOrder.SortOrder = order.SortOrder;

            _db.SaveChanges();
        }

        public void UpdateOrder(InfoSourceSortOrder order)
        {
            var dbOrder = (from t in _db.InfoSourceSortOrders
                                         where t.Id == order.Id
                                         select t).SingleOrDefault();

            dbOrder.SortOrder = order.SortOrder;

            _db.SaveChanges();
        }

        public bool UseGameSystem(string section)
        {
            return _db.InfoTypes.SingleOrDefault(u => u.InfoTypeName == section).UseGameSystem;
        }

        public int AddRssFeed(InfoSourceRssUrls model)
        {
            var rssUrl = GetRssUrl(model.Id);
            var i = 0;

            if (rssUrl != null)
            {
                try
                {
                    var feedUri = new Uri(rssUrl.URL);
                    IFeedFactory feedFactory = new HttpFeedFactory();
                    var feed = feedFactory.CreateFeed(feedUri);

                    if (feed.Items.Count > 0)
                    {
                        foreach (var item in feed.Items)
                        {
                            var article = new Articles
                            {
                                InfoTypeId = rssUrl.InfoTypeId,
                                InfoSourceId = rssUrl.InfoSourceId,
                                GameSystemId = rssUrl.GameSystemId,
                                Content = item.Content,
                                DatePublished = item.DatePublished,
                                Link = item.Link,
                                Title = item.Title
                            };

                            if (!IsDuplicateArticle(article) && SourceSpecificBypass(model.InfoTypeId, model.InfoSourceId, article))
                            {
                                _db.Articles.AddObject(article);
                                _db.SaveChanges();
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
            var byPass = true;

            if (GetInfoSourceName(infoSourceId) == "N4G")
            {
                var title = article.Title.ToUpper();
                
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

        public int AddFeedItems(int sectionId)
        {
            var i = 0;

            IList<InfoSourceRssUrls> rssList = GetRssUrls(sectionId).ToList();
            foreach (var model in rssList)
            {
                i += AddRssFeed(model);
            }

            return i;
        }

        public int AddFeedItems()
        {
            var i = 0;

            IList<InfoSourceRssUrls> rssList = GetRssUrls().ToList();
            foreach (var model in rssList)
            {
                i += AddRssFeed(model);
            }

            return i;
        }

        public IEnumerable<InfoSourceRssUrls> GetRssUrls(int sectionId)
        {
            return _db.InfoSourceRssUrls.Where(u => u.InfoTypeId == sectionId);
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
                return _db.Articles.Where(u => article.InfoTypeId == u.InfoTypeId &&
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
                var cutoff = DateTime.Now.AddDays(-180);

                var articlesToDelete = _db.Articles.Where(u => u.DatePublished < cutoff).ToList();

                foreach (var article in articlesToDelete)
                {
                    foreach (var trendingArticles in _db.TrendingArticles.ToList())
                    {
                        if (trendingArticles.ArticlesId == article.Id)
                        {
                            _db.TrendingArticles.DeleteObject(trendingArticles);
                            _db.SaveChanges();
                        }
                    }

                    foreach (var featuredArticle in _db.FeaturedArticles.ToList())
                    {
                        if (featuredArticle.Article.Id == article.Id)
                        {
                            _db.FeaturedArticles.DeleteObject(featuredArticle);
                            _db.SaveChanges();
                        }
                    }

                    _db.Articles.DeleteObject(article);
                    _db.SaveChanges();
                }

                _db.SaveChanges();

                foreach (var game in _db.Games.ToList())
                {
                    var gameArticles = _db.Articles.Where(u => u.Title.ToUpper().Contains(game.GameTitle.ToUpper())).ToList();

                    if (gameArticles.Count() == 0)
                    {
                        IEnumerable<GameInfo> gameInfoes = _db.GameInfoes.Where(u => u.GamesId == game.Id);

                        foreach (var gameInfo in gameInfoes.ToList())
                        {
                            _db.GameInfoes.DeleteObject(gameInfo);
                        }
                        _db.SaveChanges();

                        _db.Games.DeleteObject(game);
                    }
                }

                _db.SaveChanges();
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

                        var image = GetImage(dbItem.InfoSource.InfoSourceName, dbItem.Content);
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
                foreach (var oldArticle in _db.FeaturedArticles.Where(u => u.Article.InfoTypeId == section))
                    _db.FeaturedArticles.DeleteObject(oldArticle);

                _db.SaveChanges();

                // Add in the new entries
                foreach (var newArticle in featuredArticles)
                {
                    var story = new FeaturedArticles
                    {
                        Id = newArticle.Id,
                        Article = newArticle.Article,
                        ImageLink = newArticle.Image
                    };
                    _db.FeaturedArticles.AddObject(story);
                }

                _db.SaveChanges();
            }
        }

        public void MakeTrending(int section)
        {
            foreach (var article in _db.TrendingArticles.Where(u => u.Article.InfoTypeId == section))
            {
                _db.TrendingArticles.DeleteObject(article);
            }

            _db.SaveChanges();

            foreach (var game in _db.TrendingGames.Where(u => u.InfoTypeId == section))
            {
                foreach (var article in _db.TrendingArticles.Where(u => u.TrendingGamesId == game.Id))
                    _db.TrendingArticles.DeleteObject(article);

                _db.TrendingGames.DeleteObject(game);
            }

            _db.SaveChanges();

            var gameList = new List<Games>();
            foreach (var game in _db.Games)
            {
                var oldGame = false;
                foreach (var info in game.GameInfoes)
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

            var gameInfos = new Dictionary<Games, int>();

            var cutoffDays = (section == 1) ? -7 : -28;
            var cutoff = DateTime.Now.AddDays(cutoffDays);
            
            foreach (var game in gameList)
            {
                var recentArticles = GetTrendingGameArticles(section, game.GameTitle).Where(u => u.DatePublished >= cutoff).ToList();
                var count = recentArticles.Count();
                foreach (var article in recentArticles)
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

            var counter = 0;
            foreach (var game in gameInfos.Keys)
            {
                if (counter > 5)
                    break;

                if (gameInfos[game] < 5)
                    continue;

                var trendingGame = new TrendingGames
                {
                    InfoTypeId = section,
                    GamesId = game.Id
                };

                _db.TrendingGames.AddObject(trendingGame);

                counter++;
            }
            _db.SaveChanges();

            foreach (var game in _db.TrendingGames)
            {
                var gameTitle = GetGameTitle(game.GamesId).ToUpper();
                var articleList = GetArticles(section).Where(u => u.Title.ToUpper().Contains(gameTitle.ToUpper())).OrderByDescending(u => u.DatePublished).ToList();

                foreach (var article in articleList)
                {
                    var trendingArticle = new TrendingArticles
                    {
                        TrendingGamesId = game.Id,
                        ArticlesId = article.Id
                    };

                    _db.TrendingArticles.AddObject(trendingArticle);
                }
            }
            _db.SaveChanges();
        }

        public IEnumerable<Articles> GetArticlesWithImagesOnly(int section)
        {
            var temp = (from tempDb in _db.Articles
                        where tempDb.InfoTypeId == section &&
                              (tempDb.InfoSourceId != 1 /*IGN*/ && tempDb.InfoSourceId != 2 /*Gamespot*/ && tempDb.InfoSourceId != 5 /*CGV*/ && tempDb.InfoSourceId != 8 /*N4G*/)
                        select tempDb);

            return temp;
        }

        public string GetImage(string source, string content)
        {
            var imageIndex = content.IndexOf("img");
            var image = "";
            // Add companies that have images, but are not display-able
            if (!NewsHelper.BadImageCompany(source))
            {
                if (imageIndex > 0)
                {
                    // Get the dimensions of the image
                    var srcIndex = content.IndexOf("src=\"", imageIndex);
                    if (srcIndex > 0)
                    {
                        var srcEndIndex = content.IndexOf("\"", srcIndex + 5);

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
            return _db.Games.SingleOrDefault(u => u.Id == gameId).GameTitle;
        }

        public string GetUrl(int sectionId, int sourceId, int gameSystemId)
        {
            try
            {
                return _db.InfoSourceRssUrls.SingleOrDefault(u => u.InfoTypeId == sectionId && u.InfoSourceId == sourceId && u.GameSystemId == gameSystemId).URL;
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
                return _db.GameSystems.SingleOrDefault(u => u.Id == gameSystemId).GameSystemName;
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
                var count = from tempArticle in _db.Articles
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
                var infoType = GetInfoType(id);

                foreach (var article in GetArticles(id))
                {
                    var featured = _db.FeaturedArticles.SingleOrDefault(u => u.Id == article.Id);
                    if (featured != null)
                    {
                        _db.FeaturedArticles.DeleteObject(featured);
                    }

                    _db.Articles.DeleteObject(article);
                }
                _db.SaveChanges();

                foreach (var url in GetUrlsByInfoType(id))
                {
                    _db.InfoSourceRssUrls.DeleteObject(url);
                }
                _db.SaveChanges();

                var sortOrder = _db.InfoTypeSortOrders.SingleOrDefault(u => u.InfoType.Id == id);
                if (sortOrder != null)
                {
                    _db.InfoTypeSortOrders.DeleteObject(sortOrder);
                    _db.SaveChanges();
                }
                
                _db.InfoTypes.DeleteObject(infoType);
                _db.SaveChanges();
            }
            catch
            {
                // Do Nothing
            }
        }

        private IEnumerable<InfoSourceRssUrls> GetUrlsByInfoType(int infoTypeId)
        {
            return _db.InfoSourceRssUrls.Where(u => u.InfoTypeId == infoTypeId);
        }

        internal void DeleteInfoSource(int id)
        {
            try
            {
                var infoSource = GetInfoSource(id);

                foreach (var article in GetArticlesBySourceId(id))
                {
                    var featured = _db.FeaturedArticles.SingleOrDefault(u => u.Id == article.Id);
                    if (featured != null)
                    {
                        _db.FeaturedArticles.DeleteObject(featured);
                    }
                    _db.Articles.DeleteObject(article);
                }

                foreach (var url in GetUrlsBySourceId(id))
                {
                    _db.InfoSourceRssUrls.DeleteObject(url);
                }
                _db.SaveChanges();

                var sortOrder = _db.InfoSourceSortOrders.SingleOrDefault(u => u.InfoSource.Id == id);
                if (sortOrder != null)
                {
                    _db.InfoSourceSortOrders.DeleteObject(sortOrder);
                    _db.SaveChanges();
                }

                _db.InfoSources.DeleteObject(infoSource);
                _db.SaveChanges();
            }
            catch
            {
                // Do Nothing
            }
        }

        private IEnumerable<InfoSourceRssUrls> GetUrlsBySourceId(int sourceId)
        {
            return _db.InfoSourceRssUrls.Where(u => u.InfoSourceId == sourceId);
        }

        private IEnumerable<Articles> GetArticlesBySourceId(int id)
        {
            return _db.Articles.Where(u => u.InfoSourceId == id);
        }

        internal void AddPoll(AddPollModel model)
        {
            var poll = new Poll
            {
                Title = model.Title
            };
            _db.Polls.AddObject(poll);

            _db.SaveChanges();

            var items = model.Answers.Split('\n');

            foreach (var item in items)
            {
                var answers = new PollAnswers
                {
                    PollId = poll.Id,

                    Answer = item.TrimEnd('\r'),
                    NumVotes = 0
                };

                _db.PollAnswers.AddObject(answers);
            }

            _db.SaveChanges();
        }

        internal IEnumerable<Poll> GetPolls()
        {
            return _db.Polls;
        }

        public Poll GetPoll(int id)
        {
            return _db.Polls.SingleOrDefault(u => u.Id == id);
        }

        internal void UpdatePoll(int pollId, int pollValue)
        {
            var poll = GetPoll(pollId);

            foreach (var answer in poll.PollAnswers)
            {
                if (answer.Id == pollValue)
                {
                    answer.NumVotes++;
                }
            }

            _db.SaveChanges();
        }

        internal void DeleteUrl(int id)
        {
            try
            {
                var url = GetRssUrl(id);

                _db.InfoSourceRssUrls.DeleteObject(url);
                _db.SaveChanges();
            }
            catch
            {

            }
        }
    }
}