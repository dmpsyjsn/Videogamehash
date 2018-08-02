using System.Collections.Generic;
using VideoGameHash.Models.TheGamesDB;

namespace VideoGameHash.Messages.Games.Commands
{
    public class AddGames
    {
        public List<Game> Games { get; set; }
    }
}