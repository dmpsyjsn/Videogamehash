using System;
using System.Data.Entity;
using System.Threading.Tasks;
using VideoGameHash.Messages.Games.Commands;
using VideoGameHash.Models;
using VideoGameHash.Repositories;

namespace VideoGameHash.Handlers.Games.Commands
{
    public class AddGamesHandler : ICommandHandler<AddGames>
    {
        private readonly VGHDatabaseContainer _db;

        public AddGamesHandler(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task Handle(AddGames command)
        {
            foreach (var game in command.Games)
            {
                var gameDb = new Models.Games();
                var usReleaseDate = Convert.ToDateTime(game.release_date);

                if (game.game_title != null)
                    gameDb.GameTitle = game.game_title;

                if (gameDb.GameTitle == null || usReleaseDate == DateTime.MinValue || await IgnoreThisGame(gameDb) || await IsDuplicateGame(gameDb)) continue;
                
                _db.Games.Add(gameDb);
            }
        }

        #region Private methods
        
        private async Task<bool> IsDuplicateGame(Models.Games game)
        {
            return await _db.Games.AnyAsync(x => x.GameTitle.Equals(game.GameTitle));
        }
        
        private async Task<bool> IgnoreThisGame(Models.Games game)
        {
            return await _db.GameIgnores.AnyAsync(x => x.GameTitle.Equals(game.GameTitle, StringComparison.OrdinalIgnoreCase));
        }

        #endregion
    }
}