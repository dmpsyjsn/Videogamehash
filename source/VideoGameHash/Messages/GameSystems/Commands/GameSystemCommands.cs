using System.Collections.Generic;
using VideoGameHash.Models;

namespace VideoGameHash.Messages.GameSystems.Commands
{

    public class AddGameSystem
    {
        public AddGameSystem(string gameSystemName)
        {
            GameSystemName = gameSystemName;
        }

        public string GameSystemName { get; }
    }

    public class AddGameSystemSortOrder
    {
        public AddGameSystemSortOrder(string gameSystemName)
        {
            GameSystemName = gameSystemName;
        }

        public string GameSystemName { get; }
    }

    public class DeleteGameSystem
    {
        public DeleteGameSystem(int id)
        {
            Id = id;
        }

        public int Id { get; }
    }

    public class UpdateGameSystemOrder
    {
        public UpdateGameSystemOrder(IEnumerable<GameSystemSortOrder> sortOrders)
        {
            GameSystemSortOrders = sortOrders;
        }

        public IEnumerable<GameSystemSortOrder> GameSystemSortOrders { get; }
    }
}