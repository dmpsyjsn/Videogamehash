using System.Collections.Generic;
using VideoGameHash.Models.TheGamesDB;

namespace VideoGameHash.Messages.Games.Commands
{
    public class AddGameInfo
    {
        public List<Game> Games { get; set; }
        public string GameSystem { get; set; }
        public List<IdNameMapping> Publishers { get; set; }
        public List<IdNameMapping> Developers { get; set; }
    }
}