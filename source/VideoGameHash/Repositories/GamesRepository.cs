using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Routing;
using Newtonsoft.Json;
using VideoGameHash.Models;
using VideoGameHash.Models.TheGamesDB;

namespace VideoGameHash.Repositories
{
    public interface IGamesRepository
    {
        Task<IEnumerable<Games>> GetGames();
        Task<IEnumerable<string>> SearchGameTitles(string search);
        Task AddGame(string gameSystem);
        Task<Games> GetGame(int id);
        Task<Games> GetGame(string gameTitle);
        Task<Dictionary<int, string>> GetTrendingGames(int count);
        Task<Dictionary<int, string>> GetPopularGames(int count);
        Task DeleteGame(int id);
        Task<GameDetailsModel> GetGameDetailsViewModel(Games game, bool useInfometrics);
    }

    public class GamesRepository : IGamesRepository
    {
        private readonly VGHDatabaseContainer _db;

        public GamesRepository(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Games>> GetGames()
        {
            return await _db.Games.OrderBy(u => u.GameTitle).ToListAsync();
        }

        public async Task<IEnumerable<string>> SearchGameTitles(string search)
        {
            var searchTerm = new Regex($@"{search}", RegexOptions.IgnoreCase);

            var games = (await _db.Games.ToListAsync()).Where(d => searchTerm.IsMatch(d.GameTitle)).Take(10).Select(x => x.GameTitle).ToList();

            return games;
        }

        public async Task AddGame(string gameSystem)
        {
            if (gameSystem == "All") return;

            var platformId = GetGamesDbPlatformId(gameSystem);

            if (string.IsNullOrEmpty(platformId)) throw new InvalidOperationException("Unable to determine platform id!");

            await ProcessGamesFromWebService(platformId, gameSystem);

        }

        public async Task<Games> GetGame(int id)
        {
            return await _db.Games.SingleOrDefaultAsync(x => x.Id.Equals(id));
        }

        public async Task<Games> GetGame(string gameTitle)
        {
            return await _db.Games.SingleOrDefaultAsync(u => u.GameTitle.Equals(gameTitle, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<Dictionary<int, string>> GetTrendingGames(int count)
        {
            var trendingGames = await _db.TrendingGames.ToListAsync();
            return trendingGames.OrderByDescending(x => x.ArticleHits).Take(10).ToDictionary(x => x.Game.Id, x => x.Game.GameTitle);
        }

        public async Task<Dictionary<int, string>> GetPopularGames(int count)
        {
            var popularGames = await _db.PopularGames.ToListAsync();
            return popularGames.OrderByDescending(x => x.ArticleHits).Take(10).ToDictionary(x => x.Game.Id, x => x.Game.GameTitle);
        }

        public async Task DeleteGame(int id)
        {
            var game = await GetGame(id);

            if (game != null)
            {
                var gameIgnore = new GameIgnore
                {
                    GameTitle = game.GameTitle
                };

                if (!await IsDuplicateIgnoredGame(game))
                    _db.GameIgnores.Add(gameIgnore);

                // Delete from GameInfo first
                await DeleteGameInfo(game.Id);

                // Delete from Trending Games
                var trendingGame = await _db.TrendingGames.SingleOrDefaultAsync(x => x.GamesId.Equals(game.Id));
                if (trendingGame != null)
                    _db.TrendingGames.Remove(trendingGame);

                // Delete from all time games list
                var allTimeGame = await _db.PopularGames.SingleOrDefaultAsync(x => x.GamesId.Equals(game.Id));
                if (allTimeGame != null)
                    _db.PopularGames.Remove(allTimeGame);

                _db.Games.Remove(game);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<GameDetailsModel> GetGameDetailsViewModel(Games game, bool useInfometrics)
        {          
            var model = new GameDetailsModel
            {
                Game = game,
                UseInfoMetrics = useInfometrics,
                AvailableGameSystems = await GetGameSystemsForThisGame(game)
            };
            
            // Image Links
            var links = new Dictionary<string, string>();
            foreach (var system in model.AvailableGameSystems)
            {
                var systemId = await GetGameSystemId(system);

                var link = await _db.GameInfoes.SingleOrDefaultAsync(u => u.GamesId == game.Id && u.GameSystemId == systemId);

                links[system] = link?.GameImage ?? string.Empty;
            }

            model.ImageLinks = links;
            var currentGameSystem = model.AvailableGameSystems[0];
            var gameSystemId = await GetGameSystemId(currentGameSystem);
            var gameInfo = await _db.GameInfoes.SingleOrDefaultAsync(u => u.GamesId == game.Id && u.GameSystemId == gameSystemId);
            
            model.Publisher = gameInfo?.Publisher ?? string.Empty;
            model.Developer = gameInfo?.Developer ?? string.Empty;
            model.Overview = gameInfo?.Overview ?? string.Empty;
            model.GamesDbNetId = gameInfo?.GamesDbNetId ?? -1;
            model.UsReleaseDate = gameInfo?.USReleaseDate ?? DateTime.MinValue;

            return model;
        }

        #region Private methods
        
        private async Task<int> GetGameSystemId(string gameSystem)
        {
            return (await _db.GameSystems.SingleOrDefaultAsync(u => u.GameSystemName == gameSystem))?.Id ?? -1;
        }

        private static string GetGamesDbPlatformId(string gameSystem)
        {
            switch (gameSystem)
            {
                case "Xbox 360":
                    return "15";
                case "Xbox One":
                    return "4920";
                case "Wii U":
                    return "38";
                case "PS3":
                    return "12";
                case "PS4":
                    return "4919";
                case "Switch":
                    return"4971";
                case "PC":
                    return "1";
            }

            return string.Empty;
        }

        #region Request methods

        private async Task ProcessGamesFromWebService(string platformId, string gameSystem)
        {
            var developers = await GetDevelopers();
            var publishers = await GetPublishers();

            var index = 1;
            while (true)
            {
                if (/*gameSystem.Equals("PC") &&*/ index > 20) break; // I have a monthly request limit...

                var url = $"{ConfigurationManager.AppSettings["TheGamesDBApiUrl"]}Games/ByPlatformID?id={platformId}&fields=publishers&apikey={ConfigurationManager.AppSettings["TheGamesDBApiKey"]}&page={index++}";

                var request = WebRequest.Create(url);
                using (var response = (HttpWebResponse)await request.GetResponseAsync())
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null) throw new InvalidOperationException("Bad response!");

                    string responseString;

                    using (var sr = new StreamReader(responseStream))
                    {
                        responseString = sr.ReadToEnd();
                    }
                    
                    if (string.IsNullOrEmpty(responseString)) throw new InvalidOperationException("Bad response!");

                    var gameResponse = JsonConvert.DeserializeObject<RootObject>(responseString);

                    var cutoff = DateTime.Now.AddMonths(-3);
                    var items = gameResponse.data.games.Where(u => !string.IsNullOrEmpty(u.release_date) && u.release_date.IndexOf('-') > 0 && Convert.ToDateTime(u.release_date) >= cutoff).ToArray();
                    foreach (var game in items)
                    {
                        var gameDb = new Games();
                        var usReleaseDate = Convert.ToDateTime(game.release_date);

                        if (game.game_title != null)
                            gameDb.GameTitle = game.game_title;

                        if (gameDb.GameTitle == null || usReleaseDate == DateTime.MinValue || IgnoreThisGame(gameDb)) continue;

                        if (!await IsDuplicateGame(gameDb))
                        {
                            _db.Games.Add(gameDb);
                            await _db.SaveChangesAsync();
                        }
                        else
                        {
                            gameDb = await GetGame(gameDb.GameTitle);
                        }

                        var gameInfo = new GameInfo
                        {
                            GamesId = gameDb.Id,
                            GameSystemId = await GetGameSystemId(gameSystem),
                            USReleaseDate = usReleaseDate,
                            GamesDbNetId = Convert.ToInt32(game.id),
                            GameImage = $"{ConfigurationManager.AppSettings["TheGamesDBImageUrl"]}{ConfigurationManager.AppSettings["TheGamesDBImageFileName"]}{game.id}-1.jpg",
                            Publisher = game.publishers == null ? string.Empty : GetRelated(game.publishers, publishers),
                            Developer = game.developers == null ? string.Empty : GetRelated(game.developers, developers)
                        };

                        if (await IsDuplicateGameInfo(gameInfo)) continue;

                        _db.GameInfoes.Add(gameInfo);
                        _db.SaveChanges();
                    }

                    if (gameResponse.pages == null || string.IsNullOrEmpty(gameResponse.pages.next)) break;
                }
            }
        }

        private string GetRelated(List<int?> baseList, List<IdNameMapping> publishers)
        {
            return string.Join(",", publishers.Where(x => baseList.Contains(x.Id)).Select(x => x.Name));
        }

        private static async Task<List<IdNameMapping>> GetDevelopers()
        {
            var url = $"{ConfigurationManager.AppSettings["TheGamesDBApiUrl"]}/Developers?apikey={ConfigurationManager.AppSettings["TheGamesDBApiKey"]}";
            var request = WebRequest.Create(url);
            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream == null) throw new InvalidOperationException("Bad response!");

                string responseString;

                using (var sr = new StreamReader(responseStream))
                {
                    responseString = sr.ReadToEnd();
                }

                if (string.IsNullOrEmpty(responseString)) throw new InvalidOperationException("Bad response!");

                var gameResponse = JsonConvert.DeserializeObject<dynamic>(responseString);

                var developers = new List<IdNameMapping>();
                
                if (gameResponse.status != "Success") return developers;
                
                var result = new RouteValueDictionary(gameResponse.data.developers);
                var developerJson = JsonConvert.SerializeObject(result.Values);
                developers.AddRange(JsonConvert.DeserializeObject<List<IdNameMapping>>(developerJson));

                return developers;
            }
        }

        private static async Task<List<IdNameMapping>> GetPublishers()
        {
            var url = $"{ConfigurationManager.AppSettings["TheGamesDBApiUrl"]}/Publishers?apikey={ConfigurationManager.AppSettings["TheGamesDBApiKey"]}";
            var request = WebRequest.Create(url);
            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream == null) throw new InvalidOperationException("Bad response!");

                string responseString;

                using (var sr = new StreamReader(responseStream))
                {
                    responseString = sr.ReadToEnd();
                }

                if (string.IsNullOrEmpty(responseString)) throw new InvalidOperationException("Bad response!");

                var gameResponse = JsonConvert.DeserializeObject<dynamic>(responseString);

                var publishers = new List<IdNameMapping>();
                
                if (gameResponse.status != "Success") return publishers;
                
                var result = new RouteValueDictionary(gameResponse.data.publishers);
                var developerJson = JsonConvert.SerializeObject(result.Values);
                publishers.AddRange(JsonConvert.DeserializeObject<List<IdNameMapping>>(developerJson));

                return publishers;
            }
        }

        #endregion

        private bool IgnoreThisGame(Games game)
        {
            return _db.GameIgnores.Any(x => x.GameTitle.Equals(game.GameTitle, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> IsDuplicateGame(Games game)
        {
            return await _db.Games.AnyAsync(x => x.GameTitle.Equals(game.GameTitle));
        }

        private async Task<bool> IsDuplicateGameInfo(GameInfo gameInfo)
        {
            return await _db.GameInfoes.AnyAsync(x => x.GamesId.Equals(gameInfo.GamesId) && x.GameSystemId.Equals(gameInfo.GameSystemId));
        }

        private async Task<bool> IsDuplicateIgnoredGame(Games game)
        {
            return await _db.GameIgnores.AnyAsync(x => x.GameTitle.Equals(game.GameTitle, StringComparison.OrdinalIgnoreCase));
        }

        private async Task DeleteGameInfo(int id)
        {
            var gameInfos = await _db.GameInfoes.Where(u => u.GamesId == id).ToListAsync();
            foreach (var gameInfo in gameInfos)
                _db.GameInfoes.Remove(gameInfo);
            await _db.SaveChangesAsync();
        }

        private async Task<List<string>> GetGameSystemsForThisGame(Games game)
        {
            var gameSystem = new List<string>();
            var gameSystemList = await _db.GameSystems
                .Where(x => !x.GameSystemName.Equals("All", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.GameSystemSortOrder.SortOrder).ToListAsync();
            foreach (var system in gameSystemList)
            {
                var hasGameInfos = await _db.GameInfoes.AnyAsync(x => x.GameSystemId.Equals(system.Id) && x.GamesId.Equals(game.Id));

                if (hasGameInfos)
                    gameSystem.Add(system.GameSystemName);
            }

            return gameSystem;
        }

        #endregion
    }
}