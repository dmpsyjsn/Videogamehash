using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Messages.Games.Queries;
using VideoGameHash.Models;

namespace VideoGameHash.Handlers.Games.Queries
{
    public class GetGamesHandler : IQueryHandler<GetGames, IEnumerable<GameViewModel>>,
        IQueryHandler<GetGameById, GameViewModel>,
        IQueryHandler<GetGameByTitle, GameViewModel>,
        IQueryHandler<SearchGames, IEnumerable<string>>,
        IQueryHandler<GetGameDetailsByGameId, GameDetailsModel>,
        IQueryHandler<GetGameDetailsByGameTitle, GameDetailsModel>,
        IQueryHandler<GetTrendingAndPopularGames, TrendingAndPopularGamesViewModel>
    {
        private readonly VGHDatabaseContainer _db;

        public GetGamesHandler(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task<IEnumerable<GameViewModel>> Handle(GetGames query)
        {
            return await _db.Games.OrderBy(u => u.GameTitle).Select(x => new GameViewModel(x)).ToListAsync();
        }

        public async Task<GameViewModel> Handle(GetGameById query)
        {
            var game = await _db.Games.SingleOrDefaultAsync(x => x.Id.Equals(query.Id));

            return game != null ? new GameViewModel(game) : null;
        }

        public async Task<GameViewModel> Handle(GetGameByTitle query)
        {
            var game = await _db.Games.SingleOrDefaultAsync(x => x.GameTitle.Equals(query.Title));

            return game != null ? new GameViewModel(game) : null;
        }

        public async Task<IEnumerable<string>> Handle(SearchGames query)
        {
            return await _db.Games.AsQueryable().Where(d => d.GameTitle.ToLower().Contains(query.GameTitle.ToLower())).Take(10).Select(x => x.GameTitle).ToListAsync();
        }

        public async Task<GameDetailsModel> Handle(GetGameDetailsByGameId query)
        {
            var game = await _db.Games.SingleOrDefaultAsync(x => x.Id.Equals(query.GameId));

            if (game == null) return null;

            var gameViewModel = new GameViewModel(game);

            return await GetGameDetailsViewModel(gameViewModel, query.UseInfoMetrics);
        }

        public async Task<GameDetailsModel> Handle(GetGameDetailsByGameTitle query)
        {
            var game = await _db.Games.SingleOrDefaultAsync(x => x.GameTitle.Equals(query.GameTitle));

            if (game == null) return null;

            var gameViewModel = new GameViewModel(game);

            return await GetGameDetailsViewModel(gameViewModel, query.UseInfoMetrics);
        }

        public async Task<TrendingAndPopularGamesViewModel> Handle(GetTrendingAndPopularGames query)
        {
            return new TrendingAndPopularGamesViewModel
            {
                TrendingGames = await _db.TrendingGames.OrderByDescending(x => x.ArticleHits).Take(query.Count).ToDictionaryAsync(x => x.Game.Id, x => x.Game.GameTitle),
                PopularGames = await _db.PopularGames.OrderByDescending(x => x.ArticleHits).Take(query.Count).ToDictionaryAsync(x => x.Game.Id, x => x.Game.GameTitle)
            };
        }

        #region Private Methods

        private async Task<int> GetGameSystemId(string gameSystem)
        {
            return (await _db.GameSystems.SingleOrDefaultAsync(u => u.GameSystemName == gameSystem))?.Id ?? -1;
        }

        private async Task<List<string>> GetGameSystemsForThisGame(GameViewModel game)
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

        private async Task<GameDetailsModel> GetGameDetailsViewModel(GameViewModel gameViewModel, bool useInfoMetrics)
        {
            var model = new GameDetailsModel
            {
                Game = gameViewModel,
                UseInfoMetrics = useInfoMetrics,
                AvailableGameSystems = await GetGameSystemsForThisGame(gameViewModel)
            };
            
            // Image Links
            var links = new Dictionary<string, string>();
            foreach (var system in model.AvailableGameSystems)
            {
                var systemId = await GetGameSystemId(system);

                var gameInfoDb = await _db.GameInfoes.SingleOrDefaultAsync(u => u.GamesId == gameViewModel.Id && u.GameSystemId == systemId);

                links[system] = gameInfoDb?.GameImage ?? string.Empty;
            }

            model.ImageLinks = links;
            var currentGameSystem = model.AvailableGameSystems[0];
            var gameSystemId = await GetGameSystemId(currentGameSystem);
            var gameInfo = await _db.GameInfoes.SingleOrDefaultAsync(u => u.GamesId == gameViewModel.Id && u.GameSystemId == gameSystemId);
            
            model.Publisher = gameInfo?.Publisher ?? string.Empty;
            model.Developer = gameInfo?.Developer ?? string.Empty;
            model.Overview = gameInfo?.Overview ?? string.Empty;
            model.GamesDbNetId = gameInfo?.GamesDbNetId ?? -1;
            model.UsReleaseDate = gameInfo?.USReleaseDate ?? DateTime.MinValue;

            return model;
        }

        #endregion
    }
}