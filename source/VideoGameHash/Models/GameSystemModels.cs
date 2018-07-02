using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;
using VideoGameHash.Repositories;


namespace VideoGameHash.Models
{
    public class GameSystemModel
    {
        [Required]
        [Display(Name = "Game System")]
        public string GameSystem { get; set; }
    }

    public class GameSystemSortOrderEdit
    {
        public IEnumerable<GameSystemSortOrder> GameSystemSortOrders { get; set; }
    }

    public class GameDetailsModel
    {
        public Games Game { get; set; }

        public List<string> AvailableGameSystems { get; set; }

        public Dictionary<string, string> ImageLinks { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Overview { get; set; }
        public int GamesDbNetId { get; set; }
        public DateTime UsReleaseDate { get; set; }
        public bool UseInfoMetrics { get; set; }
    }

    public class GameFormViewModel
    {
        public string ActionName { get; set; }
        public SelectList GameSystems { get; private set; }

        public GameFormViewModel(IEnumerable<string> gameSystemNames)
        {
            var gameSystemList = gameSystemNames.Where(gs => gs != "All").ToList();

            GameSystems = new SelectList(gameSystemList);
        }

        public GameFormViewModel(string sourceType, IEnumerable<string> gameSystemNames)
        {
            var gameSystemList = gameSystemNames.Where(gs => gs != "All" && !(sourceType == "Wikipedia" && gs == "PC")).ToList();

            GameSystems = new SelectList(gameSystemList);
        }
    }
}