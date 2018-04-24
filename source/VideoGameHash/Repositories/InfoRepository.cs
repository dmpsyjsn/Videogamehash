using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using QDFeedParser;
using VideoGameHash.Models;

namespace VideoGameHash.Repositories
{
    public class InfoRepository
    {
        private readonly VGHDatabaseContainer _db;

        public InfoRepository(VGHDatabaseContainer db)
        {
            _db = db;
        }

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
            return _db.InfoSources.OrderBy(x => x.InfoSourceSortOrder.SortOrder);
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
                order.SortOrder = (int) (maxValue + 1);

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
                order.SortOrder = (int) (maxValue + 1);

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
                var entryExists = _db.InfoSourceRssUrls.SingleOrDefault(x =>
                    x.URL.Equals(url, StringComparison.OrdinalIgnoreCase)) != null;

                if (entryExists) return;
                
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
            return _db.InfoTypes.Single(u => u.InfoTypeName == infoType).Id;
        }

        public int GetInfoSourceId(string source)
        {
            return _db.InfoSources.Single(u => u.InfoSourceName == source).Id;
        }

        public int GetGameSystemId(string gameSystem)
        {
            return _db.GameSystems.Single(u => u.GameSystemName == gameSystem).Id;
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

        public IEnumerable<Articles> GetGameArticles(string gameTitle, string source, string system)
        {
            try
            {
                var searchTerm = new Regex($@"\b{gameTitle}\b", RegexOptions.IgnoreCase);

                if (source.Equals("All") && system.Equals("All"))
                {
                    return _db.Articles.AsEnumerable()
                        .Where(u => searchTerm.IsMatch(u.Title) || searchTerm.IsMatch(u.Content))
                        .OrderByDescending(u => u.DatePublished);
                }

                if (source.Equals("All"))
                {
                    return _db.Articles.AsEnumerable()
                        .Where(u => (searchTerm.IsMatch(u.Title) || searchTerm.IsMatch(u.Content)) &&
                                    u.GameSystem.GameSystemName.Equals(system))
                        .OrderByDescending(u => u.DatePublished);
                }
                
                if (system.Equals("All"))
                {
                    return _db.Articles.AsEnumerable()
                        .Where(u => (searchTerm.IsMatch(u.Title) || searchTerm.IsMatch(u.Content)) &&
                                    u.InfoSource.InfoSourceName.Equals(source))
                        .OrderByDescending(u => u.DatePublished);
                }

                return _db.Articles.AsEnumerable()
                    .Where(u => (searchTerm.IsMatch(u.Title) || searchTerm.IsMatch(u.Content)) &&
                                u.InfoSource.InfoSourceName.Equals(source) &&
                                u.GameSystem.GameSystemName.Equals(system))
                    .OrderByDescending(u => u.DatePublished); 
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

                var matched = _db.Articles.AsEnumerable().Where(d => searchTerm.IsMatch(d.Title) || searchTerm.IsMatch(d.Content)).ToList();

                success = matched.Any();
            }
            catch (Exception)
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

            if (dbOrder != null) dbOrder.SortOrder = order.SortOrder;

            _db.SaveChanges();
        }

        public void UpdateOrder(InfoSourceSortOrder order)
        {
            var dbOrder = (from t in _db.InfoSourceSortOrders
                                         where t.Id == order.Id
                                         select t).SingleOrDefault();

            if (dbOrder != null) dbOrder.SortOrder = order.SortOrder;

            _db.SaveChanges();
        }

        public bool UseGameSystem(string section)
        {
            return _db.InfoTypes.Single(u => u.InfoTypeName == section).UseGameSystem;
        }

        public async Task<int> AddRssFeed(InfoSourceRssUrls model)
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

                            if (!IsDuplicateArticle(article))
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
                catch (Exception)
                {
                    try
                    {
                        // Try secondary feed reader
                        var feed = await FeedReader.ReadAsync(rssUrl.URL);

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
                                    DatePublished = item.PublishingDate ?? DateTime.Today,
                                    Link = item.Link,
                                    Title = item.Title
                                };

                                if (!IsDuplicateArticle(article))
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
                    catch (Exception)
                    {
                        i = 0;
                    }

                }
            }

            return i;
        }

        public async Task<int> AddFeedItems(int sectionId)
        {
            var i = 0;

            IList<InfoSourceRssUrls> rssList = GetRssUrls(sectionId).ToList();
            foreach (var model in rssList)
            {
                i += await AddRssFeed(model);
            }

            return i;
        }

        public async Task<int> AddFeedItems()
        {
            var i = 0;

            IList<InfoSourceRssUrls> rssList = GetRssUrls().ToList();
            foreach (var model in rssList)
            {
                i += await AddRssFeed(model);
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
                return _db.Articles.Any(u => article.InfoTypeId == u.InfoTypeId &&
                                              article.InfoSourceId == u.InfoSourceId &&
                                              article.GameSystemId == u.GameSystemId &&
                                              article.Title == u.Title &&
                                              article.Link == u.Link);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void DeleteOldArticles()
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-180);

                var articlesToDelete = _db.Articles.Where(u => u.DatePublished < cutoff).ToList();

                foreach (var article in articlesToDelete)
                {
                    _db.Articles.DeleteObject(article);
                }

                _db.SaveChanges();

                foreach (var game in _db.Games.ToList())
                {
                    var gameArticles = _db.Articles.Where(u => u.Title.ToUpper().Contains(game.GameTitle.ToUpper())).ToList();

                    if (!gameArticles.Any())
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
                // Do Nothing
            }
        }

        public void MakeTrending()
        {
            foreach (var game in _db.TrendingGames)
            {
                _db.DeleteObject(game);
            }

            _db.SaveChanges();

            // Get games
            var games = _db.Games.ToList();

            // See if any articles contain game title
            foreach (var game in games)
            {
                var searchTerm = new Regex($@"\b{game.GameTitle}\b", RegexOptions.IgnoreCase);
                
                var matchingArticles = _db.Articles.AsEnumerable().Where(d => d.DatePublished >= DateTime.Now.AddDays(-14) && (searchTerm.IsMatch(d.Title) || searchTerm.IsMatch(d.Content))).ToList();

                if (!matchingArticles.Any()) continue;

                var trendingGame = new TrendingGames
                {
                    GamesId = game.Id,
                    ArticleHits = matchingArticles.Count
                };

                _db.TrendingGames.AddObject(trendingGame);
            }

            _db.SaveChanges();
        }

        public void MakePopular()
        {
            foreach (var game in _db.PopularGames)
            {
                _db.DeleteObject(game);
            }

            _db.SaveChanges();

            // Get games
            var games = _db.Games.ToList();

            // See if any articles contain game title
            foreach (var game in games)
            {
                var searchTerm = new Regex($@"\b{game.GameTitle}\b", RegexOptions.IgnoreCase);
                
                var matchingArticles = _db.Articles.AsEnumerable().Where(d => (searchTerm.IsMatch(d.Title) || searchTerm.IsMatch(d.Content))).ToList();

                if (!matchingArticles.Any()) continue;

                var popularGame = new PopularGames
                {
                    GamesId = game.Id,
                    ArticleHits = matchingArticles.Count
                };

                _db.PopularGames.AddObject(popularGame);
            }

            _db.SaveChanges();
        }

        public string GetUrl(int sectionId, int sourceId, int gameSystemId)
        {
            try
            {
                return _db.InfoSourceRssUrls.Single(u => u.InfoTypeId == sectionId && u.InfoSourceId == sourceId && u.GameSystemId == gameSystemId).URL;
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
                return _db.GameSystems.Single(u => u.Id == gameSystemId).GameSystemName;
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

                return count.Any();
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

        internal List<Poll> GetPolls()
        {
            return _db.Polls.ToList();
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
            var url = GetRssUrl(id);

            _db.InfoSourceRssUrls.DeleteObject(url);
            _db.SaveChanges();
        }
    }
}