using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VideoGameHash.Handlers.Games.Commands;
using VideoGameHash.Handlers.Games.Queries;
using VideoGameHash.Messages.Games.Commands;
using VideoGameHash.Messages.Games.Queries;
using VideoGameHash.Models;
using VideoGameHash.Models.TheGamesDB;

namespace Tests.IntegrationTesting.Games
{
    [TestClass]
    public class GamesTests
    {
        [TestMethod]
        public async Task AddGame_Succeeds()
        {
            var mockGamesSet = MockHelpers.CreateMockSet<VideoGameHash.Models.Games>();
            var mockGameIgnoresSet = MockHelpers.CreateMockSet<GameIgnore>();
 
            var mockContext = new Mock<VGHDatabaseContainer>(); 
            mockContext.Setup(m => m.Games).Returns(mockGamesSet.Object);
            mockContext.Setup(m => m.GameIgnores).Returns(mockGameIgnoresSet.Object);
            
            var queryHandler = new GetGamesHandler(mockContext.Object);

            var result = (await queryHandler.Handle(new GetGames())).ToList();

            Assert.IsTrue(result.Count == 0);

            var handler = new AddGamesHandler(mockContext.Object);

            var game = new Game
            {
                game_title = "Test",
                release_date = "2018-08-08"
            };

            var command = new AddGames
            {
                Games = new List<Game> {game}
            };

            await handler.Handle(command);

            result = (await queryHandler.Handle(new GetGames())).ToList();

            Assert.IsTrue(result.Count == 1 && result[0].GameTitle.Equals(game.game_title));
        }
    }
}
