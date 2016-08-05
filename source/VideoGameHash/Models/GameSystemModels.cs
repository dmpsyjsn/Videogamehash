using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;
using VideoGameHash.Helpers;


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

        public Dictionary<int, IEnumerable<Articles>> Articles { get; set; }
        public List<string> AvailableGameSystems { get; set; }

        public string ImageLink { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Overview { get; set; }
        public int GamesDBNetId { get; set; }
        public DateTime USReleaseDate { get; set; }
        public bool UseInfoMetrics { get; set; }
    }

    public class GameFormViewModel
    {
        public string ActionName { get; set; }
        public SelectList GameSystems { get; private set; }

        public GameFormViewModel()
        {
            List<string> gameSystemList = new List<string>();
            GameSystemsRepository gsr = new GameSystemsRepository();
            foreach (GameSystem gs in gsr.GetGameSystems())
            {
                if (gs.GameSystemName != "All")
                    gameSystemList.Add(gs.GameSystemName);
            }

            GameSystems = new SelectList(gameSystemList);
        }

        public GameFormViewModel(string sourceType)
        {
            List<string> gameSystemList = new List<string>();
            GameSystemsRepository gsr = new GameSystemsRepository();
            foreach (GameSystem gs in gsr.GetGameSystems())
            {
                if (gs.GameSystemName != "All" && !(sourceType == "Wikipedia" && gs.GameSystemName == "PC"))
                    gameSystemList.Add(gs.GameSystemName);
            }

            GameSystems = new SelectList(gameSystemList);
        }
    }
}