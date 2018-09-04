using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using VideoGameHash.Handlers.GameSystems.Commands;
using VideoGameHash.Messages.GameSystems.Commands;
using VideoGameHash.Models;

namespace Tests.IntegrationTesting.GameSystems
{
    [TestClass]
    public class GameSystemTests
    {
        private Mock<VGHDatabaseContainer> _mockContext;

        [TestInitialize]
        public void Initialize()
        {
            var mockGameSystemSet = MockHelpers.CreateMockSet<GameSystem>();
            var mockGameSystemSortOrderSet = MockHelpers.CreateMockSet<GameSystemSortOrder>();
            var mockGameInfosSet = MockHelpers.CreateMockSet<GameInfo>();
            var mockArticlesSet = MockHelpers.CreateMockSet<Articles>();
            var mockInfoSourcesSet = MockHelpers.CreateMockSet<InfoSourceRssUrls>();
 
            _mockContext = new Mock<VGHDatabaseContainer>(); 
            _mockContext.Setup(m => m.GameSystems).Returns(mockGameSystemSet.Object);
            _mockContext.Setup(m => m.GameSystemSortOrders).Returns(mockGameSystemSortOrderSet.Object);
            _mockContext.Setup(m => m.GameInfoes).Returns(mockGameInfosSet.Object);
            _mockContext.Setup(m => m.Articles).Returns(mockArticlesSet.Object);
            _mockContext.Setup(m => m.InfoSourceRssUrls).Returns(mockInfoSourcesSet.Object);
        }

        [TestMethod]
        public async Task AddGameSystem_Succeeds()
        {
            var gameSystemName = StringHelpers.RandomString(8);
            var addGameSystem = new AddGameSystem(gameSystemName);

            var handler = new GameSystemCommandHandlers(_mockContext.Object);

            await handler.Handle(addGameSystem);

            var gameSystems = _mockContext.Object.GameSystems;

            Assert.IsTrue(gameSystems.Count() == 1 && gameSystems.ElementAt(0).GameSystemName.Equals(gameSystemName));
        }

        [TestMethod]
        public async Task DeleteGameSystem_Succeeds()
        {
            var gameSystemName = StringHelpers.RandomString(8);
            var addGameSystem = new AddGameSystem(gameSystemName);

            var gameSystemHandlers = new GameSystemCommandHandlers(_mockContext.Object);

            await gameSystemHandlers.Handle(addGameSystem);

            var gameSystems = _mockContext.Object.GameSystems;

            Assert.IsTrue(gameSystems.Count() == 1 && gameSystems.ElementAt(0).GameSystemName.Equals(gameSystemName));

            var deleteGameSystem = new DeleteGameSystem(gameSystems.ElementAt(0).Id);

            await gameSystemHandlers.Handle(deleteGameSystem);

            Assert.IsTrue(!gameSystems.Any());
        }
    }
}
