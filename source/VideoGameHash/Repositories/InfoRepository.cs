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
    public interface IInfoRepository
    {
        IEnumerable<InfoType> GetInfoTypes();
        InfoType GetInfoType(int id);
        IEnumerable<InfoSource> GetSources();
        InfoSource GetInfoSource(int id);
        IEnumerable<InfoSourceRssUrls> GetRssUrls();
        InfoSourceRssUrls GetRssUrl(int id);
        int AddInfoType(string name);
        int AddInfoSource(string name);
        void AddUrl(AddUrlModel model);
        void AddUrl(int typeId, int sourceId, int gameSystemId, string url);
        void EditInfo(string section, EditModel model);
        void EditSectionInfo(EditSectionModel model);
        IEnumerable<Articles> GetGameArticles(Games game, string source, string system);
        IEnumerable<InfoTypeSortOrder> GetInfoTypeSortOrder();
        IEnumerable<InfoSourceSortOrder> GetInfoSourceSortOrder();
        void UpdateOrder(InfoTypeSortOrder order);
        void UpdateOrder(InfoSourceSortOrder order);
        Task<int> AddFeedItems();

        void DeleteOldArticles();
        void MakeTrending();
        void MakePopular();
        void DeleteInfoType(int id);
        void DeleteInfoSource(int id);
        void AddPoll(AddPollModel model);
        void EditPoll(EditPollModel model);
        List<Poll> GetPolls();
        Poll GetPoll(int id);
        void UpdatePoll(int pollId, int pollValue);
        void DeleteUrl(int id);
        void ReplaceGameSystemAll();
    }

    public class InfoRepository : IInfoRepository
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

        public IEnumerable<InfoSource> GetSources()
        {
            return _db.InfoSources.OrderBy(x => x.InfoSourceSortOrder.SortOrder);
        }

        public InfoSource GetInfoSource(int id)
        {
            return _db.InfoSources.SingleOrDefault(u => u.Id == id);
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
                _db.InfoTypes.Add(infoType);
                _db.SaveChanges();

                int? maxValue = _db.InfoTypeSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
                var order = new InfoTypeSortOrder();
                infoType = GetInfoType(name);
                order.Id = infoType.Id;
                order.InfoType = infoType;
                order.SortOrder = (int) (maxValue + 1);

                _db.InfoTypeSortOrders.Add(order);
                _db.SaveChanges();

                return infoType.Id;
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                _db.InfoSources.Add(infoSource);
                _db.SaveChanges();

                int? maxValue = _db.InfoSourceSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
                var order = new InfoSourceSortOrder();
                infoSource = GetInfoSource(name);
                order.Id = infoSource.Id;
                order.InfoSource = infoSource;
                order.SortOrder = (int) (maxValue + 1);

                _db.InfoSourceSortOrders.Add(order);
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

                _db.InfoSourceRssUrls.Add(url);
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

                _db.InfoSourceRssUrls.Add(infoSourceRssUrl);
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

        public IEnumerable<Articles> GetGameArticles(Games game, string source, string system)
        {
            try
            {
                var searchTerm = new Regex($@"\b{game.GameTitle}\b", RegexOptions.IgnoreCase);

                if (source.Equals("All") && system.Equals("All"))
                {
                    return _db.Articles.AsEnumerable()
                        .Where(u => searchTerm.IsMatch(u.Title) && 
                                    game.GameInfoes.Select(x => x.GameSystem.GameSystemName).Contains(u.GameSystem.GameSystemName))
                        .OrderByDescending(u => u.DatePublished);
                }

                if (source.Equals("All"))
                {
                    return _db.Articles.AsEnumerable()
                        .Where(u => searchTerm.IsMatch(u.Title) &&
                                    u.GameSystem.GameSystemName.Equals(system))
                        .OrderByDescending(u => u.DatePublished);
                }
                
                if (system.Equals("All"))
                {
                    return _db.Articles.AsEnumerable()
                        .Where(u => searchTerm.IsMatch(u.Title) &&
                                    u.InfoSource.InfoSourceName.Equals(source) && game.GameInfoes.Select(x => x.GameSystem.GameSystemName).Contains(u.GameSystem.GameSystemName))
                        .OrderByDescending(u => u.DatePublished);
                }

                return _db.Articles.AsEnumerable()
                    .Where(u => searchTerm.IsMatch(u.Title) &&
                                u.InfoSource.InfoSourceName.Equals(source) &&
                                u.GameSystem.GameSystemName.Equals(system))
                    .OrderByDescending(u => u.DatePublished); 
            }
            catch
            {
                return null;
            }
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

        public void DeleteOldArticles()
        {
            try
            {
                var cutoff = DateTime.Now.AddDays(-180);

                var articlesToDelete = _db.Articles.Where(u => u.DatePublished < cutoff).ToList();

                foreach (var article in articlesToDelete)
                {
                    _db.Articles.Remove(article);
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
                            _db.GameInfoes.Remove(gameInfo);
                        }
                        _db.SaveChanges();

                        _db.Games.Remove(game);
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
                _db.TrendingGames.Remove(game);
            }

            _db.SaveChanges();

            // Get games
            var games = _db.Games.ToList();

            var articles = _db.Articles.ToList();

            // See if any articles contain game title
            foreach (var game in games)
            {
                var searchTerm = new Regex($@"\b{game.GameTitle}\b", RegexOptions.IgnoreCase);
                
                var matchingArticles = articles.Where(d => d.DatePublished >= DateTime.Now.AddDays(-7) && searchTerm.IsMatch(d.Title)).ToList();

                if (!matchingArticles.Any()) continue;

                var trendingGame = new TrendingGames
                {
                    GamesId = game.Id,
                    ArticleHits = matchingArticles.Count
                };

                _db.TrendingGames.Add(trendingGame);
            }

            _db.SaveChanges();
        }

        public void MakePopular()
        {
            foreach (var game in _db.PopularGames)
            {
                _db.PopularGames.Remove(game);
            }

            _db.SaveChanges();

            // Get games
            var games = _db.Games.ToList();
            
            var articles = _db.Articles.ToList();

            // See if any articles contain game title
            foreach (var game in games)
            {
                var searchTerm = new Regex($@"\b{game.GameTitle}\b", RegexOptions.IgnoreCase);
                
                var matchingArticles = articles.Where(d => searchTerm.IsMatch(d.Title)).ToList();

                if (!matchingArticles.Any()) continue;

                var popularGame = new PopularGames
                {
                    GamesId = game.Id,
                    ArticleHits = matchingArticles.Count
                };

                _db.PopularGames.Add(popularGame);
            }

            _db.SaveChanges();
        }

        public void DeleteInfoType(int id)
        {
            var infoType = GetInfoType(id);

            foreach (var article in GetArticles(id))
            {
                _db.Articles.Remove(article);
            }
            _db.SaveChanges();

            foreach (var url in GetUrlsByInfoType(id))
            {
                _db.InfoSourceRssUrls.Remove(url);
            }
            _db.SaveChanges();

            var sortOrder = _db.InfoTypeSortOrders.SingleOrDefault(u => u.InfoType.Id == id);
            if (sortOrder != null)
            {
                _db.InfoTypeSortOrders.Remove(sortOrder);
                _db.SaveChanges();
            }
            
            _db.InfoTypes.Remove(infoType);
            _db.SaveChanges();
        }

        public void DeleteInfoSource(int id)
        {
            var infoSource = GetInfoSource(id);

            foreach (var article in GetArticlesBySourceId(id))
            {
                _db.Articles.Remove(article);
            }

            foreach (var url in GetUrlsBySourceId(id))
            {
                _db.InfoSourceRssUrls.Remove(url);
            }
            _db.SaveChanges();

            var sortOrder = _db.InfoSourceSortOrders.SingleOrDefault(u => u.InfoSource.Id == id);
            if (sortOrder != null)
            {
                _db.InfoSourceSortOrders.Remove(sortOrder);
                _db.SaveChanges();
            }

            _db.InfoSources.Remove(infoSource);
            _db.SaveChanges();
        }

        public void AddPoll(AddPollModel model)
        {
            var poll = new Poll
            {
                Title = model.Title,
                DateCreated = DateTime.Now
            };
            _db.Polls.Add(poll);

            _db.SaveChanges();

            var items = model.Answers.Split('\n');
            var randomNumber = new Random(DateTime.Now.Millisecond);

            foreach (var item in items)
            {
                var answers = new PollAnswers
                {
                    PollId = poll.Id,

                    Answer = item.TrimEnd('\r'),
                    NumVotes = randomNumber.Next(0, 1000)
                };

                _db.PollAnswers.Add(answers);
            }

            _db.SaveChanges();
        }

        public void EditPoll(EditPollModel model)
        {
            var poll = GetPoll(model.Id);

            if (poll != null)
            {
                poll.Title = model.Title;
                poll.DateCreated = DateTime.Now;

                var items = model.Answers.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var randomNumber = new Random(DateTime.Now.Millisecond);

                DeletePollAnswerByPollId(poll.Id);

                foreach (var item in items)
                {
                    var answers = new PollAnswers
                    {
                        PollId = poll.Id,
                        Answer = item.TrimEnd('\r'),
                        NumVotes = randomNumber.Next(0, 1000)
                    };

                    _db.PollAnswers.Add(answers);
                }

                _db.SaveChanges();
            }
        }

        public List<Poll> GetPolls()
        {
            return _db.Polls.OrderByDescending(x => x.DateCreated).Take(3).ToList();
        }

        public Poll GetPoll(int id)
        {
            return _db.Polls.SingleOrDefault(u => u.Id == id);
        }

        public void UpdatePoll(int pollId, int pollValue)
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

        public void DeleteUrl(int id)
        {
            var url = GetRssUrl(id);

            _db.InfoSourceRssUrls.Remove(url);
            _db.SaveChanges();
        }

        public void ReplaceGameSystemAll()
        {
            var games = _db.Games.ToList();

            var articlesToReAdd = new List<Articles>();

            foreach (var game in games)
            {
                var searchTerm = new Regex($@"\b{game.GameTitle}\b", RegexOptions.IgnoreCase);
                
                var articles = _db.Articles.AsEnumerable().Where(x => searchTerm.IsMatch(x.Title) && x.GameSystem.GameSystemName.Equals("All")).ToList();

                foreach (var article in articles)
                {
                    var gameSystems = game.GameInfoes.Select(x => x.GameSystem).Where(x => !x.GameSystemName.Equals("All")).Distinct().ToList();
                    
                    foreach (var system in gameSystems)
                    {
                        var gameSystemArticle = new Articles
                        {
                            DatePublished = article.DatePublished,
                            GameSystemId = system.Id,
                            InfoSourceId = article.InfoSourceId,
                            Title = article.Title,
                            InfoTypeId = article.InfoTypeId,
                            Link = article.Link
                        };

                        if (!articlesToReAdd.Any(x =>
                             x.DatePublished.Equals(gameSystemArticle.DatePublished)
                            && x.GameSystemId.Equals(gameSystemArticle.GameSystemId)
                            && x.InfoSourceId.Equals(gameSystemArticle.InfoSourceId)
                            && x.Title.Equals(gameSystemArticle.Title)
                            && x.InfoTypeId.Equals(gameSystemArticle.InfoTypeId)
                            && x.Link.Equals(gameSystemArticle.Link)))
                        {
                            articlesToReAdd.Add(gameSystemArticle);
                        }
                    }
                }
            }
            
            _db.SaveChanges();

            // re-add articles (now by an actual gamesystem)
            foreach (var article in articlesToReAdd)
            {
                _db.Articles.Add(article);
            }

            _db.SaveChanges();

            // Now delete all articles with All as the gamesystem
            var articlesToDelete = _db.Articles.Where(x => x.GameSystem.GameSystemName.Equals("All")).ToList();

            foreach (var article in articlesToDelete)
            {
                _db.Articles.Remove(article);
            }

            _db.SaveChanges();
        }

        #region Private Methods

        private int GetInfoTypeId(string infoType)
        {
            return _db.InfoTypes.Single(u => u.InfoTypeName == infoType).Id;
        }

        private InfoType GetInfoType(string name)
        {
            return _db.InfoTypes?.SingleOrDefault(u => u.InfoTypeName == name);
        }

        private InfoSource GetInfoSource(string name)
        {
            return _db.InfoSources.SingleOrDefault(u => u.InfoSourceName == name);
        }

        private int GetInfoSourceId(string source)
        {
            return _db.InfoSources.Single(u => u.InfoSourceName == source).Id;
        }

        private int GetGameSystemId(string gameSystem)
        {
            return _db.GameSystems.Single(u => u.GameSystemName == gameSystem).Id;
        }

        private IQueryable<Articles> GetArticles(int section)
        {
            return _db.Articles.Where(u => u.InfoTypeId == section);
        }

        private async Task<int> AddRssFeed(InfoSourceRssUrls model)
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
                        var gameSystems = rssUrl.GameSystem.GameSystemName.Equals("All")
                            ? _db.GameSystems.Where(x => !x.GameSystemName.Equals("All")).Select(x => x.Id).ToList()
                            : new List<int> {rssUrl.GameSystemId};

                        foreach (var system in gameSystems)
                        {
                            foreach (var item in feed.Items)
                            {
                                var article = new Articles
                                {
                                    InfoTypeId = rssUrl.InfoTypeId,
                                    InfoSourceId = rssUrl.InfoSourceId,
                                    GameSystemId = system,
                                    DatePublished = item.DatePublished,
                                    Link = item.Link,
                                    Title = item.Title
                                };

                                if (!IsDuplicateArticle(article))
                                {
                                    _db.Articles.Add(article);
                                    _db.SaveChanges();
                                    i++;
                                }
                                else
                                    break;
                            }
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
                            var gameSystems = rssUrl.GameSystem.GameSystemName.Equals("All")
                                ? _db.GameSystems.Where(x => !x.GameSystemName.Equals("All")).Select(x => x.Id).ToList()
                                : new List<int> {rssUrl.GameSystemId};

                            foreach (var system in gameSystems)
                            {
                                foreach (var item in feed.Items)
                                {
                                    var article = new Articles
                                    {
                                        InfoTypeId = rssUrl.InfoTypeId,
                                        InfoSourceId = rssUrl.InfoSourceId,
                                        GameSystemId = system,
                                        DatePublished = item.PublishingDate ?? DateTime.Today,
                                        Link = item.Link,
                                        Title = item.Title
                                    };

                                    if (!IsDuplicateArticle(article))
                                    {
                                        _db.Articles.Add(article);
                                        _db.SaveChanges();
                                        i++;
                                    }
                                    else
                                        break;
                                }
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

        private bool IsDuplicateArticle(Articles article)
        {
            try
            {
                return _db.Articles.Any(u => article.InfoSourceId == u.InfoSourceId &&
                                             article.GameSystemId == u.GameSystemId &&
                                             article.Title == u.Title &&
                                             article.Link == u.Link);
            }
            catch (Exception)
            {
                return false;
            }
        }

        private IEnumerable<InfoSourceRssUrls> GetUrlsByInfoType(int infoTypeId)
        {
            return _db.InfoSourceRssUrls.Where(u => u.InfoTypeId == infoTypeId);
        }

        private IEnumerable<InfoSourceRssUrls> GetUrlsBySourceId(int sourceId)
        {
            return _db.InfoSourceRssUrls.Where(u => u.InfoSourceId == sourceId);
        }

        private IEnumerable<Articles> GetArticlesBySourceId(int id)
        {
            return _db.Articles.Where(u => u.InfoSourceId == id);
        }

        private void DeletePollAnswerByPollId(int pollId)
        {
            var answers = _db.PollAnswers.Where(x => x.PollId == pollId).ToList();

            foreach (var answer in answers)
            {
                _db.PollAnswers.Remove(answer);
            }

            _db.SaveChanges();
        }

        #endregion
    }
}