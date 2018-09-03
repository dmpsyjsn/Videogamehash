using System.Collections.Generic;
using VideoGameHash.Handlers;
using VideoGameHash.Models;

namespace VideoGameHash.Messages.GameSystems.Queries
{
    public class GetGameSystemId : IQuery<int>
    {
        public GetGameSystemId(string gameSystemName)
        {
            GameSystemName = gameSystemName;
        }

        public string GameSystemName { get; }
    }

    public class GetGameSystems : IQuery<IEnumerable<GameSystemViewModel>>
    {

    }

    public class GetGameSystemForms : IQuery<GameFormViewModel>
    {

    }

    public class GetGameSystemSortOrder : IQuery<GameSystemSortOrderEdit>
    {

    }
}