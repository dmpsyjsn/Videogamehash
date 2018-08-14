using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VideoGameHash.Handlers.Games.Commands;
using VideoGameHash.Messages.Games.Commands;
using VideoGameHash.Models;
using VideoGameHash.Models.TheGamesDB;

namespace Tests.IntegrationTesting.Games
{
    [TestClass]
    public class GamesTests
    {
        private Mock<VGHDatabaseContainer> _mockContext;

        [TestInitialize]
        public void Initialize()
        {
            var mockGamesSet = MockHelpers.CreateMockSet<VideoGameHash.Models.Games>();
            var mockGameInfoSet = MockHelpers.CreateMockSet<GameInfo>();
            var mockGameIgnoresSet = MockHelpers.CreateMockSet<GameIgnore>();
            var mockGameSystemSet = MockHelpers.CreateMockSet<GameSystem>();
 
            _mockContext = new Mock<VGHDatabaseContainer>(); 
            _mockContext.Setup(m => m.Games).Returns(mockGamesSet.Object);
            _mockContext.Setup(m => m.GameInfoes).Returns(mockGameInfoSet.Object);
            _mockContext.Setup(m => m.GameIgnores).Returns(mockGameIgnoresSet.Object);
            _mockContext.Setup(m => m.GameSystems).Returns(mockGameSystemSet.Object);
        }

        [TestMethod]
        public async Task AddGame_Succeeds()
        {
            var handler = new AddGamesHandler(_mockContext.Object);

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

            var games = _mockContext.Object.Games;

            Assert.IsTrue(games.Count() == 1 && games.ElementAt(0).GameTitle.Equals(game.game_title));
        }

        [TestMethod]
        public async Task AddGame_Fails()
        {
            var handler = new AddGamesHandler(_mockContext.Object);

            var game = new Game
            {
                game_title = StringHelpers.RandomString(8),
                release_date = string.Empty
            };

            var command = new AddGames
            {
                Games = new List<Game> {game}
            };

            await handler.Handle(command);

            Assert.IsTrue(!_mockContext.Object.Games.Any());
        }

        [TestMethod]
        public async Task AddGameInfo_Succeeds()
        {
            var addGameHandler = new AddGamesHandler(_mockContext.Object);
            var addGameInfoHandler = new AddGameInfoHandler(_mockContext.Object);

            var game = new Game
            {
                game_title = StringHelpers.RandomString(8),
                release_date = DateTime.Today.AddDays(-50).ToString("yyyy-MM-dd")
            };

            var addGamesCommand = new AddGames
            {
                Games = new List<Game> {game}
            };

            var gameSystemName = StringHelpers.RandomString(8);

            var addGameInfoCommand = new AddGameInfo
            {
                Games = new List<Game> {game},
                GameSystem = gameSystemName,
            };

            _mockContext.Object.GameSystems.Add(new GameSystem
            {
                GameSystemName = gameSystemName
            });

            await addGameHandler.Handle(addGamesCommand);

            await addGameInfoHandler.Handle(addGameInfoCommand);

            var gameInfo = _mockContext.Object.GameInfoes;

            Assert.IsTrue(gameInfo.Count() == 1 && gameInfo.ElementAt(0).USReleaseDate.ToShortDateString().Equals(DateTime.Today.AddDays(-50).ToShortDateString()));
        }
    }
}
