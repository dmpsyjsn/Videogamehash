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
    
    public partial class PollAnswers
    {
        public int Id { get; set; }
        public int PollId { get; set; }
        public string Answer { get; set; }
        public int NumVotes { get; set; }
    
        public virtual Poll Poll { get; set; }
    }
}
