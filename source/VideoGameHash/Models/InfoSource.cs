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
    
    public partial class InfoSource
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public InfoSource()
        {
            this.Articles = new HashSet<Articles>();
            this.InfoSourceRssUrls = new HashSet<InfoSourceRssUrls>();
        }
    
        public int Id { get; set; }
        public string InfoSourceName { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<Articles> Articles { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<InfoSourceRssUrls> InfoSourceRssUrls { get; set; }
        public virtual InfoSourceSortOrder InfoSourceSortOrder { get; set; }
    }
}
