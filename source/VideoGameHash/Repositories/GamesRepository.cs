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
        Task<Games> GetGame(int id);
        Task<Games> GetGame(string gameTitle);
        Task<Dictionary<int, string>> GetTrendingGames(int count);
        Task<Dictionary<int, string>> GetPopularGames(int count);
        Task<GameDetailsModel> GetGameDetailsViewModel(Games game, bool useInfometrics);
    }

    public class GamesRepository : IGamesRepository
    {
        private readonly VGHDatabaseContainer _db;

        public GamesRepository(VGHDatabaseContainer db)
        {
            _db = db;
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
            return await _db.TrendingGames.OrderByDescending(x => x.ArticleHits).Take(count).ToDictionaryAsync(x => x.Game.Id, x => x.Game.GameTitle);
        }

        public async Task<Dictionary<int, string>> GetPopularGames(int count)
        {
           
            return await _db.PopularGames.OrderByDescending(x => x.ArticleHits).Take(count).ToDictionaryAsync(x => x.Game.Id, x => x.Game.GameTitle);
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

                var gameInfoDb = await _db.GameInfoes.SingleOrDefaultAsync(u => u.GamesId == game.Id && u.GameSystemId == systemId);

                links[system] = gameInfoDb?.GameImage ?? string.Empty;
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