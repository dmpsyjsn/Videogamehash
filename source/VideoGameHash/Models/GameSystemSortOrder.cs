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
    
    public partial class GameSystemSortOrder
    {
        public int SortOrder { get; set; }
        public int Id { get; set; }
    
        public virtual GameSystem GameSystem { get; set; }
    }
}
