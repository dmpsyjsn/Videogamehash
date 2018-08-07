using System;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Messages.Games.Commands;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Handlers.Games.Commands
{
    public class DeleteGameHandler : ICommandHandler<DeleteGame>
    {
        private readonly VGHDatabaseContainer _db;

        public DeleteGameHandler(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task Handle(DeleteGame command)
        {
            var game = await _db.Games.SingleOrDefaultAsync(x => x.Id.Equals(command.Id));

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
            }
        }

        #region Private methods

        private async Task<bool> IsDuplicateIgnoredGame(Models.Games game)
        {
            return await _db.GameIgnores.AnyAsync(x => x.GameTitle.Equals(game.GameTitle, StringComparison.OrdinalIgnoreCase));
        }

        private async Task DeleteGameInfo(int id)
        {
            var gameInfos = await _db.GameInfoes.Where(u => u.GamesId == id).ToListAsync();
            foreach (var gameInfo in gameInfos)
                _db.GameInfoes.Remove(gameInfo);
        }

        #endregion
    }
}