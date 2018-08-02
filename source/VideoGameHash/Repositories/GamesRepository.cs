using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Models;

namespace VideoGameHash.Repositories
{
    public interface IGamesRepository
    {
        Task<IEnumerable<Games>> GetGames();
        Task<IEnumerable<string>> SearchGameTitles(string search);
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
            return await _db.Games.AsQueryable().Where(d => d.GameTitle.ToLower().Contains(search.ToLower())).Take(10).Select(x => x.GameTitle).ToListAsync();
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