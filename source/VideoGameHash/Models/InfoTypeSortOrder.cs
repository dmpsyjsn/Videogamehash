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
    
    public partial class InfoTypeSortOrder
    {
        public int Id { get; set; }
        public int SortOrder { get; set; }
    
        public virtual InfoType InfoType { get; set; }
    }
}
