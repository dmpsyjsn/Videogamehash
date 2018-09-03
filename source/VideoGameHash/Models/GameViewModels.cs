using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Mvc;


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

    public class GameViewModel
    {
        public GameViewModel(Games game)
        {
            Id = game.Id;
            GameTitle = game.GameTitle;
            GameSystems = game.GameInfoes.Select(x => x.GameSystem.GameSystemName).Distinct().ToList();
        }

        public int Id { get; }
        public string GameTitle { get; }
        public List<string> GameSystems { get; }
    }

    public class GameDetailsModel
    {
        public GameViewModel Game { get; set; }

        public List<string> AvailableGameSystems { get; set; }

        public Dictionary<string, string> ImageLinks { get; set; }
        public string Developer { get; set; }
        public string Publisher { get; set; }
        public string Overview { get; set; }
        public int GamesDbNetId { get; set; }
        public DateTime UsReleaseDate { get; set; }
        public bool UseInfoMetrics { get; set; }
    }

    public class TrendingAndPopularGamesViewModel
    {
        public Dictionary<int, string> TrendingGames { get; set; }
        public Dictionary<int, string> PopularGames { get; set; }
    }

    public class GameFormViewModel
    {
        public string ActionName { get; set; }
        public SelectList GameSystems { get; }

        public GameFormViewModel(IEnumerable<string> gameSystemNames)
        {
            var gameSystemList = gameSystemNames.Where(gs => gs != "All").ToList();

            GameSystems = new SelectList(gameSystemList);
        }
    }

    public class GameSystemViewModel
    {
        public GameSystemViewModel(int id, string name, int order)
        {
            Id = id;
            GameSystemName = name;
            GameSystemSortOrder = order;
        }

        public int Id { get; }
        public string GameSystemName { get; }
        public int GameSystemSortOrder { get; }
    }
}