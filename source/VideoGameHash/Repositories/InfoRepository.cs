using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CodeHollow.FeedReader;
using QDFeedParser;
using VideoGameHash.Models;
using VideoGameHash.Models.HighchartModels;

namespace VideoGameHash.Repositories
{
    public interface IInfoRepository
    {
        Task<IEnumerable<InfoType>> GetInfoTypes();
        Task<InfoType> GetInfoType(int id);
        Task<IEnumerable<InfoSource>> GetSources();
        Task<InfoSource> GetInfoSource(int id);
        Task<IEnumerable<InfoSourceRssUrls>> GetRssUrls();
        Task<InfoSourceRssUrls> GetRssUrl(int id);
        Task<int> AddInfoType(string name);
        Task<int> AddInfoSource(string name);
        Task AddUrl(AddUrlModel model);
        Task AddUrl(int typeId, int sourceId, int gameSystemId, string url);
        Task EditInfo(string section, EditModel model);
        Task EditSectionInfo(EditSectionModel model);
        Task<List<Articles>> GetGameArticles(Games game, string source, string system, LineChartTimeRange range = LineChartTimeRange.AllTime, bool useDesc = true);
        Task<IEnumerable<InfoTypeSortOrder>> GetInfoTypeSortOrder();
        Task<IEnumerable<InfoSourceSortOrder>> GetInfoSourceSortOrder();
        Task UpdateOrder(InfoTypeSortOrder order);
        Task UpdateOrder(InfoSourceSortOrder order);
        Task<int> AddFeedItems();

        Task DeleteOldArticles();
        Task MakeTrending();
        Task MakePopular();
        Task DeleteInfoType(int id);
        Task DeleteInfoSource(int id);
        Task AddPoll(AddPollModel model);
        Task EditPoll(EditPollModel model);
        Task<List<Poll>> GetPolls();
        Task<Poll> GetPoll(int id);
        Task UpdatePoll(int pollId, int pollValue);
        Task DeleteUrl(int id);
        Task ReplaceGameSystemAll();
    }

    public class InfoRepository : IInfoRepository
    {
        private readonly VGHDatabaseContainer _db;

        public InfoRepository(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task<IEnumerable<InfoType>> GetInfoTypes()
        {
            return await _db.InfoTypes.ToListAsync();
        }

        public async Task<InfoType> GetInfoType(int id)
        {
            return await _db.InfoTypes.SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<InfoSource>> GetSources()
        {
            return await _db.InfoSources.OrderBy(x => x.InfoSourceSortOrder.SortOrder).ToListAsync();
        }

        public async Task<InfoSource> GetInfoSource(int id)
        {
            return await _db.InfoSources.SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<InfoSourceRssUrls>> GetRssUrls()
        {
            return await _db.InfoSourceRssUrls.ToListAsync();
        }

        public async Task<InfoSourceRssUrls> GetRssUrl(int id)
        {
            return await _db.InfoSourceRssUrls.SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<int> AddInfoType(string name)
        {
            var type = await GetInfoType(name);
            if (type != null) return type.Id;

            var infoType = new InfoType
            {
                InfoTypeName = name,
                UseGameSystem = true
            };
            _db.InfoTypes.Add(infoType);
            await _db.SaveChangesAsync();

            int? maxValue = _db.InfoTypeSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
            var order = new InfoTypeSortOrder();
            infoType = await GetInfoType(name);
            order.Id = infoType.Id;
            order.InfoType = infoType;
            order.SortOrder = (int) (maxValue + 1);

            _db.InfoTypeSortOrders.Add(order);
            await _db.SaveChangesAsync();

            return infoType.Id;
        }

        public async Task<int> AddInfoSource(string name)
        {
            var source = await GetInfoSource(name);
            if (source != null) return source.Id;

            var infoSource = new InfoSource
            {
                InfoSourceName = name
            };
            _db.InfoSources.Add(infoSource);
            await _db.SaveChangesAsync();

            int? maxValue = _db.InfoSourceSortOrders.Max(u => (int?)u.SortOrder) ?? 0;
            var order = new InfoSourceSortOrder();
            infoSource = await GetInfoSource(name);
            order.Id = infoSource.Id;
            order.InfoSource = infoSource;
            order.SortOrder = (int) (maxValue + 1);

            _db.InfoSourceSortOrders.Add(order);
            await _db.SaveChangesAsync();

            return infoSource.Id;
        }

        public async Task AddUrl(AddUrlModel model)
        {
            var url = new InfoSourceRssUrls
            {
                InfoTypeId = await GetInfoTypeId(model.Section),
                InfoSourceId = await GetInfoSourceId(model.Source),
                GameSystemId = await GetGameSystemId(model.GameSystem),
                URL = model.Url
            };

            _db.InfoSourceRssUrls.Add(url);
            await _db.SaveChangesAsync();
        }

        public async Task AddUrl(int typeId, int sourceId, int gameSystemId, string url)
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
            await _db.SaveChangesAsync();
        }

        public async Task EditInfo(string section, EditModel model)
        {
            if (section == "Source")
            {
                var infoSource = await GetInfoSource(model.Id);
                infoSource.InfoSourceName = model.Name;
            }
            else // section == "Url"
            {
                var url = await GetRssUrl(model.Id);
                url.URL = model.Name;
            }

            await _db.SaveChangesAsync();
        }

        public async Task EditSectionInfo(EditSectionModel model)
        {
            var infoType = await GetInfoType(model.Id);
            infoType.InfoTypeName = model.Name;
            infoType.UseGameSystem = model.UseGameSystem;
            await _db.SaveChangesAsync();
        }

        public async Task<List<Articles>> GetGameArticles(Games game, string source, string system, LineChartTimeRange range, bool useDesc = true)
        {
            var relatedGameInfos = game.GameInfoes.Select(x => x.GameSystem.GameSystemName).ToList();

            DateTime cutoff;
            switch (range)
            {
                case LineChartTimeRange.AllTime:
                    cutoff = DateTime.MinValue;
                    break;
                case LineChartTimeRange.LastMonth:
                    cutoff = DateTime.Now.AddDays(-30);
                    break;
                case LineChartTimeRange.Last3Months:
                    cutoff = DateTime.Now.AddDays(-90);
                    break;
                // range == LastYear
                case LineChartTimeRange.Last6Months:
                    cutoff = DateTime.Now.AddDays(-180);
                    break;
                default:
                    cutoff = DateTime.Now.AddDays(-365);
                    break;
            }

            var searchTerm = $"{game.GameTitle}";
            var articleQuery = _db.Articles.AsQueryable();
            if (source.Equals("All") && system.Equals("All"))
            {
                articleQuery = articleQuery
                    .Where(u => u.Title.Contains(searchTerm) &&
                                relatedGameInfos.Contains(u.GameSystem.GameSystemName) &&
                                u.DatePublished >= cutoff);
            }

            else if (source.Equals("All"))
            {
                articleQuery = articleQuery.Where(u => u.Title.Contains(game.GameTitle) &&
                                u.GameSystem.GameSystemName.Equals(system) &&
                                u.DatePublished >= cutoff);
            }
            
            else if (system.Equals("All"))
            {
                articleQuery = articleQuery.Where(u => u.Title.Contains(game.GameTitle) &&
                                u.InfoSource.InfoSourceName.Equals(source) && relatedGameInfos.Contains(u.GameSystem.GameSystemName) &&
                                u.DatePublished >= cutoff);
            }

            else
            {
                articleQuery = articleQuery.Where(u => u.Title.Contains(game.GameTitle) &&
                                        u.InfoSource.InfoSourceName.Equals(source) &&
                                        u.GameSystem.GameSystemName.Equals(system) &&
                                        u.DatePublished >= cutoff); 
            }

            return useDesc
                ? await articleQuery.OrderByDescending(x => x.DatePublished).ToListAsync()
                : await articleQuery.OrderBy(x => x.DatePublished).ToListAsync();
        }

        public async Task<IEnumerable<InfoTypeSortOrder>> GetInfoTypeSortOrder()
        {
            return await _db.InfoTypeSortOrders.ToListAsync();
        }

        public async Task<IEnumerable<InfoSourceSortOrder>> GetInfoSourceSortOrder()
        {
            return await _db.InfoSourceSortOrders.ToListAsync();
        }

        public async Task UpdateOrder(InfoTypeSortOrder order)
        {
            var dbOrder = await _db.InfoTypeSortOrders.SingleOrDefaultAsync(t => t.Id == order.Id);

            if (dbOrder != null) dbOrder.SortOrder = order.SortOrder;

            await _db.SaveChangesAsync();
        }

        public async Task UpdateOrder(InfoSourceSortOrder order)
        {
            var dbOrder = await _db.InfoSourceSortOrders.SingleOrDefaultAsync(t => t.Id == order.Id);

            if (dbOrder != null) dbOrder.SortOrder = order.SortOrder;

            await _db.SaveChangesAsync();
        }

        public async Task<int> AddFeedItems()
        {
            var i = 0;

            foreach (var model in await GetRssUrls())
            {
                i += await AddRssFeed(model);
            }

            return i;
        }

        public async Task DeleteOldArticles()
        {
            var cutoff = DateTime.Now.AddDays(-180);

            foreach (var article in await _db.Articles.Where(u => u.DatePublished < cutoff).ToListAsync())
            {
                _db.Articles.Remove(article);
            }

            await _db.SaveChangesAsync();

            var gameList = await _db.Games.ToListAsync();
            foreach (var game in gameList)
            {
                var gameArticles = await _db.Articles.Where(u => u.Title.ToUpper().Contains(game.GameTitle.ToUpper())).ToListAsync();

                if (!gameArticles.Any())
                {
                    var gameInfoes = await _db.GameInfoes.Where(u => u.GamesId == game.Id).ToListAsync();

                    foreach (var gameInfo in gameInfoes)
                    {
                        _db.GameInfoes.Remove(gameInfo);
                    }
                    await _db.SaveChangesAsync();

                    _db.Games.Remove(game);
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task MakeTrending()
        {
            var trendingGames = await _db.TrendingGames.ToListAsync();
            foreach (var game in trendingGames)
            {
                _db.TrendingGames.Remove(game);
            }

            await _db.SaveChangesAsync();

            // Get games
            var games = await _db.Games.ToListAsync();

            // ToDo: See if there's a way we don't have to query the whole table
            var articles = await _db.Articles.Where(d => d.DatePublished >= DbFunctions.AddDays(DateTime.Now, -7)).Select(x => x.Title).ToListAsync();
            
            // See if any articles contain game title
            foreach (var game in games)
            {
                var searchTerm = new Regex($@"\b{game.GameTitle}\b", RegexOptions.IgnoreCase);
                
                var matchingArticles = articles.Count(d => searchTerm.IsMatch(d));

                if (matchingArticles <= 0) continue;

                var trendingGame = new TrendingGames
                {
                    GamesId = game.Id,
                    ArticleHits = matchingArticles
                };

                _db.TrendingGames.Add(trendingGame);
            }

            await _db.SaveChangesAsync();
        }

        public async Task MakePopular()
        {
            foreach (var game in await _db.PopularGames.ToListAsync())
            {
                _db.PopularGames.Remove(game);
            }

            _db.SaveChanges();

            // Get games
            var games = await _db.Games.ToListAsync();
            
            // ToDo: See if there's a way we don't have to query the whole table
            var articles = await _db.Articles.Select(x => x.Title).ToListAsync();

            // See if any articles contain game title
            foreach (var game in games)
            {
                var searchTerm = new Regex($@"\b{game.GameTitle}\b", RegexOptions.IgnoreCase);
                
                var matchingArticles = articles.Count(d => searchTerm.IsMatch(d));

                if (matchingArticles <= 0) continue;

                var popularGame = new PopularGames
                {
                    GamesId = game.Id,
                    ArticleHits = matchingArticles
                };

                _db.PopularGames.Add(popularGame);
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteInfoType(int id)
        {
            var infoType = await GetInfoType(id);

            foreach (var article in await GetArticles(id))
            {
                _db.Articles.Remove(article);
            }
            await _db.SaveChangesAsync();

            foreach (var url in await GetUrlsByInfoType(id))
            {
                _db.InfoSourceRssUrls.Remove(url);
            }
            await _db.SaveChangesAsync();

            var sortOrder = await _db.InfoTypeSortOrders.SingleOrDefaultAsync(u => u.InfoType.Id == id);
            if (sortOrder != null)
            {
                _db.InfoTypeSortOrders.Remove(sortOrder);
                await _db.SaveChangesAsync();
            }
            
            _db.InfoTypes.Remove(infoType);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteInfoSource(int id)
        {
            var infoSource = await GetInfoSource(id);

            foreach (var article in await GetArticlesBySourceId(id))
            {
                _db.Articles.Remove(article);
            }

            foreach (var url in await GetUrlsBySourceId(id))
            {
                _db.InfoSourceRssUrls.Remove(url);
            }
            await _db.SaveChangesAsync();

            var sortOrder = await _db.InfoSourceSortOrders.SingleOrDefaultAsync(u => u.InfoSource.Id == id);
            if (sortOrder != null)
            {
                _db.InfoSourceSortOrders.Remove(sortOrder);
                await _db.SaveChangesAsync();
            }

            _db.InfoSources.Remove(infoSource);
            await _db.SaveChangesAsync();
        }

        public async Task AddPoll(AddPollModel model)
        {
            var poll = new Poll
            {
                Title = model.Title,
                DateCreated = DateTime.Now
            };
            _db.Polls.Add(poll);

            await _db.SaveChangesAsync();

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

            await _db.SaveChangesAsync();
        }

        public async Task EditPoll(EditPollModel model)
        {
            var poll = await GetPoll(model.Id);

            if (poll != null)
            {
                poll.Title = model.Title;
                poll.DateCreated = DateTime.Now;

                var items = model.Answers.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                var randomNumber = new Random(DateTime.Now.Millisecond);

                await DeletePollAnswerByPollId(poll.Id);

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

                await _db.SaveChangesAsync();
            }
        }

        public async Task<List<Poll>> GetPolls()
        {
            return await _db.Polls.OrderByDescending(x => x.DateCreated).Take(6).ToListAsync();
        }

        public async Task<Poll> GetPoll(int id)
        {
            return await _db.Polls.SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdatePoll(int pollId, int pollValue)
        {
            var poll = await GetPoll(pollId);

            foreach (var answer in poll.PollAnswers)
            {
                if (answer.Id == pollValue)
                {
                    answer.NumVotes++;
                }
            }

            await _db.SaveChangesAsync();
        }

        public async Task DeleteUrl(int id)
        {
            var url = await GetRssUrl(id);

            _db.InfoSourceRssUrls.Remove(url);
            await _db.SaveChangesAsync();
        }

        public async Task ReplaceGameSystemAll()
        {
            var games = await _db.Games.ToListAsync();

            var articlesToReAdd = new List<Articles>();

            var articles = await _db.Articles.ToListAsync();

            foreach (var game in games)
            {
                var searchTerm = new Regex($@"\b{game.GameTitle}\b", RegexOptions.IgnoreCase);
                
                var matchingArticles = articles.Where(x => searchTerm.IsMatch(x.Title) && x.GameSystem.GameSystemName.Equals("All")).ToList();

                foreach (var article in matchingArticles)
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
            
            await _db.SaveChangesAsync();

            // re-add articles (now by an actual gamesystem)
            foreach (var article in articlesToReAdd)
            {
                _db.Articles.Add(article);
            }

            await _db.SaveChangesAsync();

            // Now delete all articles with All as the gamesystem
            var articlesToDelete = await _db.Articles.Where(x => x.GameSystem.GameSystemName.Equals("All")).ToListAsync();

            foreach (var article in articlesToDelete)
            {
                _db.Articles.Remove(article);
            }

            await _db.SaveChangesAsync();
        }

        #region Private Methods

        private async Task<int> GetInfoTypeId(string infoType)
        {
            return (await _db.InfoTypes.SingleAsync(u => u.InfoTypeName == infoType)).Id;
        }

        private async Task<InfoType> GetInfoType(string name)
        {          
            return await _db.InfoTypes.SingleOrDefaultAsync(u => u.InfoTypeName == name);
        }

        private async Task<InfoSource> GetInfoSource(string name)
        {
            return await _db.InfoSources.SingleOrDefaultAsync(u => u.InfoSourceName == name);
        }

        private async Task<int> GetInfoSourceId(string source)
        {
            return (await _db.InfoSources.SingleAsync(u => u.InfoSourceName == source)).Id;
        }

        private async Task<int> GetGameSystemId(string gameSystem)
        {
            return (await _db.GameSystems.SingleAsync(u => u.GameSystemName == gameSystem)).Id;
        }

        private async Task<IEnumerable<Articles>> GetArticles(int section)
        {
            return await _db.Articles.Where(u => u.InfoTypeId == section).ToListAsync();
        }

        private async Task<int> AddRssFeed(InfoSourceRssUrls model)
        {
            var rssUrl = await GetRssUrl(model.Id);
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
                            ? await _db.GameSystems.Where(x => !x.GameSystemName.Equals("All")).Select(x => x.Id).ToListAsync()
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
                                    await _db.SaveChangesAsync();
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
                                ? await _db.GameSystems.Where(x => !x.GameSystemName.Equals("All")).Select(x => x.Id).ToListAsync()
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
                                        await _db.SaveChangesAsync();
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

        private async Task<IEnumerable<InfoSourceRssUrls>> GetUrlsByInfoType(int infoTypeId)
        {
            return await _db.InfoSourceRssUrls.Where(u => u.InfoTypeId == infoTypeId).ToListAsync();
        }

        private async Task<IEnumerable<InfoSourceRssUrls>> GetUrlsBySourceId(int sourceId)
        {
            return await _db.InfoSourceRssUrls.Where(u => u.InfoSourceId == sourceId).ToListAsync();
        }

        private async Task<IEnumerable<Articles>> GetArticlesBySourceId(int id)
        {
            return await _db.Articles.Where(u => u.InfoSourceId == id).ToListAsync();
        }

        private async Task DeletePollAnswerByPollId(int pollId)
        {
            var answers = await _db.PollAnswers.Where(x => x.PollId == pollId).ToListAsync();

            foreach (var answer in answers)
            {
                _db.PollAnswers.Remove(answer);
            }

            await _db.SaveChangesAsync();
        }

        private static bool InTimeRange(DateTime datePublished, LineChartTimeRange range)
        {
            if (range == LineChartTimeRange.AllTime)
                return true;

            DateTime cutoff;
            if (range == LineChartTimeRange.LastMonth)
            {
                cutoff = DateTime.Now.AddDays(-30);
            }
            else if (range == LineChartTimeRange.Last3Months)
            {
                cutoff = DateTime.Now.AddDays(-90);
            }
            else if (range == LineChartTimeRange.Last6Months)
            {
                cutoff = DateTime.Now.AddDays(-180);
            }
            else // range == LastYear
            {
                cutoff = DateTime.Now.AddDays(-365);
            }

            return datePublished >= cutoff;
        }

        #endregion
    }
}