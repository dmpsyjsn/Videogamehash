using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using VideoGameHash.Helpers;
using VideoGameHash.Messages.Games.Commands;
using VideoGameHash.Models;
using VideoGameHash.Models.TheGamesDB;
using VideoGameHash.Repositories;

namespace VideoGameHash.Handlers.Games.Commands
{
    public class AddGameInfoHandler : ICommandHandler<AddGameInfo>
    {
        private readonly VGHDatabaseContainer _db;

        public AddGameInfoHandler(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task Handle(AddGameInfo command)
        {
            var publishers = await TheGamesDBHelper.GetDataByField("Publishers");
            var developers = await TheGamesDBHelper.GetDataByField("Developers");

            foreach (var game in command.Games)
            {
                var gameDb = await GetGame(game.game_title);

                if (gameDb == null) continue;

                var usReleaseDate = Convert.ToDateTime(game.release_date);

                var gameInfo = new GameInfo
                {
                    GamesId = gameDb.Id,
                    GameSystemId = await GetGameSystemId(command.GameSystem),
                    USReleaseDate = usReleaseDate,
                    GamesDbNetId = Convert.ToInt32(game.id),
                    GameImage = $"{ConfigurationManager.AppSettings["TheGamesDBImageUrl"]}{ConfigurationManager.AppSettings["TheGamesDBImageFileName"]}{game.id}-1.jpg",
                    Publisher = game.publishers == null ? string.Empty : GetRelated(game.publishers, publishers),
                    Developer = game.developers == null ? string.Empty : GetRelated(game.developers, developers)
                };

                var gameInfoDb = await GetGameInfo(gameInfo);

                if (gameInfoDb != null)
                {
                    gameInfoDb.USReleaseDate = gameInfo.USReleaseDate;
                    gameInfoDb.GamesDbNetId = gameInfo.GamesDbNetId;
                    gameInfoDb.GameImage = gameInfo.GameImage;
                    gameInfoDb.Publisher = gameInfo.Publisher;
                    gameInfoDb.Developer = gameInfo.Developer;
                }
                else
                {
                    _db.GameInfoes.Add(gameInfo);
                }
            }
        }

        #region private methods

        public async Task<Models.Games> GetGame(string gameTitle)
        {
            return await _db.Games.SingleOrDefaultAsync(u => u.GameTitle.Equals(gameTitle, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<int> GetGameSystemId(string gameSystem)
        {
            return (await _db.GameSystems.SingleOrDefaultAsync(u => u.GameSystemName == gameSystem))?.Id ?? -1;
        }

        private string GetRelated(List<int?> baseList, List<IdNameMapping> publishers)
        {
            return string.Join(",", publishers.Where(x => baseList.Contains(x.Id)).Select(x => x.Name));
        }

        private async Task<GameInfo> GetGameInfo(GameInfo gameInfo)
        {
            return await _db.GameInfoes.SingleOrDefaultAsync(x => x.GamesId.Equals(gameInfo.GamesId) && x.GameSystemId.Equals(gameInfo.GameSystemId));
        }

        #endregion
    }
}