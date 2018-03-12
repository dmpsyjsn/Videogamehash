using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using VideoGameHash.Helpers;

namespace VideoGameHash.Models
{
    public class InfoTypeViewModel
    {
        public IEnumerable<InfoType> InfoTypes;
        public IEnumerable<InfoSource> InfoSources;
        public IEnumerable<Poll> Polls;
        public IEnumerable<InfoSourceRssUrls> InfoSourceRssUrls;
    }

    public class EditModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ActionLink { get; set; }
    }

    public class EditSectionModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool UseGameSystem { get; set; }
    }

    public class AddInfoModel
    {
        public string Name { get; set; }
        public string InfoType { get; set; }
    }

    public class AddPollModel
    {
        public string Title { get; set; }
        public string Answers { get; set; }
    }

    public class AddUrlViewModel
    {
        public AddUrlModel Model { get; private set; }
        public SelectList Section { get; private set; }
        public SelectList Source { get; private set; }
        public SelectList GameSystem { get; private set; }

        // Constructor
        public AddUrlViewModel(AddUrlModel model)
        {
            var gsr = new GameSystemsRepository();
            Model = model;
            Section = new SelectList(NewsHelper.SectionList, model.Section);
            Source = new SelectList(NewsHelper.SourceList, model.Source);

            var gameSystems = new List<string>();
            foreach (var system in gsr.GetGameSystems())
            {
                gameSystems.Add(system.GameSystemName);
            }
            GameSystem = new SelectList(gameSystems, model.GameSystem);
        }

        public AddUrlViewModel()
        {
            Model = null;
            Section = null;
            Source = null;
            GameSystem = null;
        }
    }

    public class AddUrlModel
    {
        [Required]
        [Display(Name = "Section")]
        public string Section;

        [Required]
        [Display(Name = "Source")]
        public string Source;

        [Required]
        [Display(Name = "GameSystem")]
        public string GameSystem;

        [Required]
        [Display(Name = "Url")]
        public string Url;
    }

    public class InfoTypeSortOrderEdit
    {
        public IEnumerable<InfoTypeSortOrder> InfoTypeSortOrders { get; set; }
    }

    public class InfoSourceSortOrderEdit
    {
        public IEnumerable<InfoSourceSortOrder> InfoSourceSortOrders { get; set; }
    }

    public class FeaturedClass
    {
        public int Id { get; set; }
        public Articles Article { get; set; }
        public string Image { get; set; }
    }
}