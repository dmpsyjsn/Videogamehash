//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace VideoGameHash.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GameInfo
    {
        public int Id { get; set; }
        public System.DateTime USReleaseDate { get; set; }
        public string GameImage { get; set; }
        public string Publisher { get; set; }
        public string Developer { get; set; }
        public string Overview { get; set; }
        public int GamesDbNetId { get; set; }
        public int GameSystemId { get; set; }
        public int GamesId { get; set; }
    
        public virtual GameSystem GameSystem { get; set; }
        public virtual Games Game { get; set; }
    }
}