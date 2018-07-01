﻿//------------------------------------------------------------------------------
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
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    
    public partial class VGHDatabaseContainer : DbContext
    {
        public VGHDatabaseContainer()
            : base("name=VGHDatabaseContainer")
        {
        }
    
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            throw new UnintentionalCodeFirstException();
        }
    
        public virtual DbSet<Membership> Memberships { get; set; }
        public virtual DbSet<Roles> Roles { get; set; }
        public virtual DbSet<UsersInRoles> UsersInRoles { get; set; }
        public virtual DbSet<GameSystem> GameSystems { get; set; }
        public virtual DbSet<Games> Games { get; set; }
        public virtual DbSet<GameSystemSortOrder> GameSystemSortOrders { get; set; }
        public virtual DbSet<UserProfile> UserProfiles { get; set; }
        public virtual DbSet<GameIgnore> GameIgnores { get; set; }
        public virtual DbSet<GameInfo> GameInfoes { get; set; }
        public virtual DbSet<InfoType> InfoTypes { get; set; }
        public virtual DbSet<InfoSource> InfoSources { get; set; }
        public virtual DbSet<Articles> Articles { get; set; }
        public virtual DbSet<InfoSourceRssUrls> InfoSourceRssUrls { get; set; }
        public virtual DbSet<InfoSourceSortOrder> InfoSourceSortOrders { get; set; }
        public virtual DbSet<InfoTypeSortOrder> InfoTypeSortOrders { get; set; }
        public virtual DbSet<Poll> Polls { get; set; }
        public virtual DbSet<PollAnswers> PollAnswers { get; set; }
        public virtual DbSet<TrendingGames> TrendingGames { get; set; }
        public virtual DbSet<PopularGames> PopularGames { get; set; }
        public virtual DbSet<Errors> Errors { get; set; }
    }
}
