
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 07/28/2013 19:59:56
-- Generated from EDMX file: C:\Users\PB202\documents\visual studio 2010\Projects\VideoGameHash\VideoGameHash\Models\VGHDatabase.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [VGHDatabase];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[FK_RolesUsersInRoles]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UsersInRoles] DROP CONSTRAINT [FK_RolesUsersInRoles];
GO
IF OBJECT_ID(N'[dbo].[FK_GameSystemGameSystemSortOrder]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GameSystemSortOrders] DROP CONSTRAINT [FK_GameSystemGameSystemSortOrder];
GO
IF OBJECT_ID(N'[dbo].[FK_GameSystemGameInfo]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GameInfoes] DROP CONSTRAINT [FK_GameSystemGameInfo];
GO
IF OBJECT_ID(N'[dbo].[FK_GamesGameInfo]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[GameInfoes] DROP CONSTRAINT [FK_GamesGameInfo];
GO
IF OBJECT_ID(N'[dbo].[FK_UserProfileUsersInRoles]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[UsersInRoles] DROP CONSTRAINT [FK_UserProfileUsersInRoles];
GO
IF OBJECT_ID(N'[dbo].[FK_InfoTypeArticles]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Articles] DROP CONSTRAINT [FK_InfoTypeArticles];
GO
IF OBJECT_ID(N'[dbo].[FK_InfoSourceArticles]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Articles] DROP CONSTRAINT [FK_InfoSourceArticles];
GO
IF OBJECT_ID(N'[dbo].[FK_InfoSourceInfoSourceRssUrls]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InfoSourceRssUrls] DROP CONSTRAINT [FK_InfoSourceInfoSourceRssUrls];
GO
IF OBJECT_ID(N'[dbo].[FK_GameSystemInfoSourceRssUrls]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InfoSourceRssUrls] DROP CONSTRAINT [FK_GameSystemInfoSourceRssUrls];
GO
IF OBJECT_ID(N'[dbo].[FK_GameSystemArticles]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[Articles] DROP CONSTRAINT [FK_GameSystemArticles];
GO
IF OBJECT_ID(N'[dbo].[FK_InfoTypeInfoSourceRssUrls]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InfoSourceRssUrls] DROP CONSTRAINT [FK_InfoTypeInfoSourceRssUrls];
GO
IF OBJECT_ID(N'[dbo].[FK_InfoTypeInfoTypeSortOrder]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InfoTypeSortOrders] DROP CONSTRAINT [FK_InfoTypeInfoTypeSortOrder];
GO
IF OBJECT_ID(N'[dbo].[FK_InfoSourceInfoSourceSortOrder]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[InfoSourceSortOrders] DROP CONSTRAINT [FK_InfoSourceInfoSourceSortOrder];
GO
IF OBJECT_ID(N'[dbo].[FK_ArticlesFeaturedArticles]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[FeaturedArticles] DROP CONSTRAINT [FK_ArticlesFeaturedArticles];
GO
IF OBJECT_ID(N'[dbo].[FK_TrendingGamesTrendingArticles]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TrendingArticles] DROP CONSTRAINT [FK_TrendingGamesTrendingArticles];
GO
IF OBJECT_ID(N'[dbo].[FK_GamesTrendingGames]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TrendingGames] DROP CONSTRAINT [FK_GamesTrendingGames];
GO
IF OBJECT_ID(N'[dbo].[FK_InfoTypeTrendingGames]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TrendingGames] DROP CONSTRAINT [FK_InfoTypeTrendingGames];
GO
IF OBJECT_ID(N'[dbo].[FK_ArticlesTrendingArticles]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[TrendingArticles] DROP CONSTRAINT [FK_ArticlesTrendingArticles];
GO
IF OBJECT_ID(N'[dbo].[FK_PollPollAnswers]', 'F') IS NOT NULL
    ALTER TABLE [dbo].[PollAnswers] DROP CONSTRAINT [FK_PollPollAnswers];
GO

-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[Memberships]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Memberships];
GO
IF OBJECT_ID(N'[dbo].[Roles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Roles];
GO
IF OBJECT_ID(N'[dbo].[UsersInRoles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UsersInRoles];
GO
IF OBJECT_ID(N'[dbo].[GameSystems]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GameSystems];
GO
IF OBJECT_ID(N'[dbo].[Games]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Games];
GO
IF OBJECT_ID(N'[dbo].[GameSystemSortOrders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GameSystemSortOrders];
GO
IF OBJECT_ID(N'[dbo].[UserProfiles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[UserProfiles];
GO
IF OBJECT_ID(N'[dbo].[GameIgnores]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GameIgnores];
GO
IF OBJECT_ID(N'[dbo].[GameInfoes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[GameInfoes];
GO
IF OBJECT_ID(N'[dbo].[InfoTypes]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InfoTypes];
GO
IF OBJECT_ID(N'[dbo].[InfoSources]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InfoSources];
GO
IF OBJECT_ID(N'[dbo].[Articles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Articles];
GO
IF OBJECT_ID(N'[dbo].[InfoSourceRssUrls]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InfoSourceRssUrls];
GO
IF OBJECT_ID(N'[dbo].[InfoSourceSortOrders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InfoSourceSortOrders];
GO
IF OBJECT_ID(N'[dbo].[InfoTypeSortOrders]', 'U') IS NOT NULL
    DROP TABLE [dbo].[InfoTypeSortOrders];
GO
IF OBJECT_ID(N'[dbo].[FeaturedArticles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[FeaturedArticles];
GO
IF OBJECT_ID(N'[dbo].[TrendingArticles]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TrendingArticles];
GO
IF OBJECT_ID(N'[dbo].[TrendingGames]', 'U') IS NOT NULL
    DROP TABLE [dbo].[TrendingGames];
GO
IF OBJECT_ID(N'[dbo].[Polls]', 'U') IS NOT NULL
    DROP TABLE [dbo].[Polls];
GO
IF OBJECT_ID(N'[dbo].[PollAnswers]', 'U') IS NOT NULL
    DROP TABLE [dbo].[PollAnswers];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'Memberships'
CREATE TABLE [dbo].[Memberships] (
    [UserId] int  NOT NULL,
    [CreateDate] datetime  NOT NULL,
    [ConfirmationToken] nvarchar(max)  NULL,
    [IsConfirmed] bit  NOT NULL,
    [LastPasswordFailureDate] datetime  NULL,
    [PasswordFailuresSinceLastSuccess] int  NULL,
    [Password] nvarchar(max)  NOT NULL,
    [PasswordChangeDate] datetime  NULL,
    [PasswordSalt] nvarchar(max)  NOT NULL,
    [PasswordVerificationToken] nvarchar(max)  NULL,
    [PasswordVerificationTokenExpirationDate] datetime  NULL,
    [Email] nvarchar(max)  NOT NULL,
    [SecurityQuestion] nvarchar(max)  NOT NULL,
    [SecurityAnswer] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Roles'
CREATE TABLE [dbo].[Roles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RoleName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'UsersInRoles'
CREATE TABLE [dbo].[UsersInRoles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [RolesId] int  NOT NULL,
    [UserProfileId] int  NOT NULL
);
GO

-- Creating table 'GameSystems'
CREATE TABLE [dbo].[GameSystems] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [GameSystemName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Games'
CREATE TABLE [dbo].[Games] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [GameTitle] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'GameSystemSortOrders'
CREATE TABLE [dbo].[GameSystemSortOrders] (
    [SortOrder] int  NOT NULL,
    [Id] int  NOT NULL,
    [GameSystem_Id] int  NOT NULL
);
GO

-- Creating table 'UserProfiles'
CREATE TABLE [dbo].[UserProfiles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [UserName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'GameIgnores'
CREATE TABLE [dbo].[GameIgnores] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [GameTitle] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'GameInfoes'
CREATE TABLE [dbo].[GameInfoes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [USReleaseDate] datetime  NOT NULL,
    [GameImage] nvarchar(max)  NULL,
    [Publisher] nvarchar(max)  NULL,
    [Developer] nvarchar(max)  NULL,
    [Overview] nvarchar(max)  NULL,
    [GamesDbNetId] int  NOT NULL,
    [GameSystemId] int  NOT NULL,
    [GamesId] int  NOT NULL
);
GO

-- Creating table 'InfoTypes'
CREATE TABLE [dbo].[InfoTypes] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [InfoTypeName] nvarchar(max)  NOT NULL,
    [UseGameSystem] bit  NOT NULL
);
GO

-- Creating table 'InfoSources'
CREATE TABLE [dbo].[InfoSources] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [InfoSourceName] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'Articles'
CREATE TABLE [dbo].[Articles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [InfoTypeId] int  NOT NULL,
    [InfoSourceId] int  NOT NULL,
    [GameSystemId] int  NOT NULL,
    [Title] nvarchar(max)  NOT NULL,
    [Content] nvarchar(max)  NOT NULL,
    [Link] nvarchar(max)  NOT NULL,
    [DatePublished] datetime  NOT NULL
);
GO

-- Creating table 'InfoSourceRssUrls'
CREATE TABLE [dbo].[InfoSourceRssUrls] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [InfoSourceId] int  NOT NULL,
    [GameSystemId] int  NOT NULL,
    [InfoTypeId] int  NOT NULL,
    [URL] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'InfoSourceSortOrders'
CREATE TABLE [dbo].[InfoSourceSortOrders] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SortOrder] int  NOT NULL,
    [InfoSource_Id] int  NOT NULL
);
GO

-- Creating table 'InfoTypeSortOrders'
CREATE TABLE [dbo].[InfoTypeSortOrders] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [SortOrder] int  NOT NULL,
    [InfoType_Id] int  NOT NULL
);
GO

-- Creating table 'FeaturedArticles'
CREATE TABLE [dbo].[FeaturedArticles] (
    [Id] int  NOT NULL,
    [ImageLink] nvarchar(max)  NOT NULL,
    [Article_Id] int  NOT NULL
);
GO

-- Creating table 'TrendingArticles'
CREATE TABLE [dbo].[TrendingArticles] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [TrendingGamesId] int  NOT NULL,
    [ArticlesId] int  NOT NULL
);
GO

-- Creating table 'TrendingGames'
CREATE TABLE [dbo].[TrendingGames] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [GamesId] int  NOT NULL,
    [InfoTypeId] int  NOT NULL
);
GO

-- Creating table 'Polls'
CREATE TABLE [dbo].[Polls] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [Title] nvarchar(max)  NOT NULL
);
GO

-- Creating table 'PollAnswers'
CREATE TABLE [dbo].[PollAnswers] (
    [Id] int IDENTITY(1,1) NOT NULL,
    [PollId] int  NOT NULL,
    [Answer] nvarchar(max)  NOT NULL,
    [NumVotes] int  NOT NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [UserId] in table 'Memberships'
ALTER TABLE [dbo].[Memberships]
ADD CONSTRAINT [PK_Memberships]
    PRIMARY KEY CLUSTERED ([UserId] ASC);
GO

-- Creating primary key on [Id] in table 'Roles'
ALTER TABLE [dbo].[Roles]
ADD CONSTRAINT [PK_Roles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UsersInRoles'
ALTER TABLE [dbo].[UsersInRoles]
ADD CONSTRAINT [PK_UsersInRoles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GameSystems'
ALTER TABLE [dbo].[GameSystems]
ADD CONSTRAINT [PK_GameSystems]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Games'
ALTER TABLE [dbo].[Games]
ADD CONSTRAINT [PK_Games]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GameSystemSortOrders'
ALTER TABLE [dbo].[GameSystemSortOrders]
ADD CONSTRAINT [PK_GameSystemSortOrders]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'UserProfiles'
ALTER TABLE [dbo].[UserProfiles]
ADD CONSTRAINT [PK_UserProfiles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GameIgnores'
ALTER TABLE [dbo].[GameIgnores]
ADD CONSTRAINT [PK_GameIgnores]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'GameInfoes'
ALTER TABLE [dbo].[GameInfoes]
ADD CONSTRAINT [PK_GameInfoes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'InfoTypes'
ALTER TABLE [dbo].[InfoTypes]
ADD CONSTRAINT [PK_InfoTypes]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'InfoSources'
ALTER TABLE [dbo].[InfoSources]
ADD CONSTRAINT [PK_InfoSources]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [PK_Articles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'InfoSourceRssUrls'
ALTER TABLE [dbo].[InfoSourceRssUrls]
ADD CONSTRAINT [PK_InfoSourceRssUrls]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'InfoSourceSortOrders'
ALTER TABLE [dbo].[InfoSourceSortOrders]
ADD CONSTRAINT [PK_InfoSourceSortOrders]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'InfoTypeSortOrders'
ALTER TABLE [dbo].[InfoTypeSortOrders]
ADD CONSTRAINT [PK_InfoTypeSortOrders]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'FeaturedArticles'
ALTER TABLE [dbo].[FeaturedArticles]
ADD CONSTRAINT [PK_FeaturedArticles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TrendingArticles'
ALTER TABLE [dbo].[TrendingArticles]
ADD CONSTRAINT [PK_TrendingArticles]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'TrendingGames'
ALTER TABLE [dbo].[TrendingGames]
ADD CONSTRAINT [PK_TrendingGames]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'Polls'
ALTER TABLE [dbo].[Polls]
ADD CONSTRAINT [PK_Polls]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- Creating primary key on [Id] in table 'PollAnswers'
ALTER TABLE [dbo].[PollAnswers]
ADD CONSTRAINT [PK_PollAnswers]
    PRIMARY KEY CLUSTERED ([Id] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- Creating foreign key on [RolesId] in table 'UsersInRoles'
ALTER TABLE [dbo].[UsersInRoles]
ADD CONSTRAINT [FK_RolesUsersInRoles]
    FOREIGN KEY ([RolesId])
    REFERENCES [dbo].[Roles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_RolesUsersInRoles'
CREATE INDEX [IX_FK_RolesUsersInRoles]
ON [dbo].[UsersInRoles]
    ([RolesId]);
GO

-- Creating foreign key on [GameSystem_Id] in table 'GameSystemSortOrders'
ALTER TABLE [dbo].[GameSystemSortOrders]
ADD CONSTRAINT [FK_GameSystemGameSystemSortOrder]
    FOREIGN KEY ([GameSystem_Id])
    REFERENCES [dbo].[GameSystems]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GameSystemGameSystemSortOrder'
CREATE INDEX [IX_FK_GameSystemGameSystemSortOrder]
ON [dbo].[GameSystemSortOrders]
    ([GameSystem_Id]);
GO

-- Creating foreign key on [GameSystemId] in table 'GameInfoes'
ALTER TABLE [dbo].[GameInfoes]
ADD CONSTRAINT [FK_GameSystemGameInfo]
    FOREIGN KEY ([GameSystemId])
    REFERENCES [dbo].[GameSystems]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GameSystemGameInfo'
CREATE INDEX [IX_FK_GameSystemGameInfo]
ON [dbo].[GameInfoes]
    ([GameSystemId]);
GO

-- Creating foreign key on [GamesId] in table 'GameInfoes'
ALTER TABLE [dbo].[GameInfoes]
ADD CONSTRAINT [FK_GamesGameInfo]
    FOREIGN KEY ([GamesId])
    REFERENCES [dbo].[Games]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GamesGameInfo'
CREATE INDEX [IX_FK_GamesGameInfo]
ON [dbo].[GameInfoes]
    ([GamesId]);
GO

-- Creating foreign key on [UserProfileId] in table 'UsersInRoles'
ALTER TABLE [dbo].[UsersInRoles]
ADD CONSTRAINT [FK_UserProfileUsersInRoles]
    FOREIGN KEY ([UserProfileId])
    REFERENCES [dbo].[UserProfiles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_UserProfileUsersInRoles'
CREATE INDEX [IX_FK_UserProfileUsersInRoles]
ON [dbo].[UsersInRoles]
    ([UserProfileId]);
GO

-- Creating foreign key on [InfoTypeId] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [FK_InfoTypeArticles]
    FOREIGN KEY ([InfoTypeId])
    REFERENCES [dbo].[InfoTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InfoTypeArticles'
CREATE INDEX [IX_FK_InfoTypeArticles]
ON [dbo].[Articles]
    ([InfoTypeId]);
GO

-- Creating foreign key on [InfoSourceId] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [FK_InfoSourceArticles]
    FOREIGN KEY ([InfoSourceId])
    REFERENCES [dbo].[InfoSources]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InfoSourceArticles'
CREATE INDEX [IX_FK_InfoSourceArticles]
ON [dbo].[Articles]
    ([InfoSourceId]);
GO

-- Creating foreign key on [InfoSourceId] in table 'InfoSourceRssUrls'
ALTER TABLE [dbo].[InfoSourceRssUrls]
ADD CONSTRAINT [FK_InfoSourceInfoSourceRssUrls]
    FOREIGN KEY ([InfoSourceId])
    REFERENCES [dbo].[InfoSources]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InfoSourceInfoSourceRssUrls'
CREATE INDEX [IX_FK_InfoSourceInfoSourceRssUrls]
ON [dbo].[InfoSourceRssUrls]
    ([InfoSourceId]);
GO

-- Creating foreign key on [GameSystemId] in table 'InfoSourceRssUrls'
ALTER TABLE [dbo].[InfoSourceRssUrls]
ADD CONSTRAINT [FK_GameSystemInfoSourceRssUrls]
    FOREIGN KEY ([GameSystemId])
    REFERENCES [dbo].[GameSystems]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GameSystemInfoSourceRssUrls'
CREATE INDEX [IX_FK_GameSystemInfoSourceRssUrls]
ON [dbo].[InfoSourceRssUrls]
    ([GameSystemId]);
GO

-- Creating foreign key on [GameSystemId] in table 'Articles'
ALTER TABLE [dbo].[Articles]
ADD CONSTRAINT [FK_GameSystemArticles]
    FOREIGN KEY ([GameSystemId])
    REFERENCES [dbo].[GameSystems]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GameSystemArticles'
CREATE INDEX [IX_FK_GameSystemArticles]
ON [dbo].[Articles]
    ([GameSystemId]);
GO

-- Creating foreign key on [InfoTypeId] in table 'InfoSourceRssUrls'
ALTER TABLE [dbo].[InfoSourceRssUrls]
ADD CONSTRAINT [FK_InfoTypeInfoSourceRssUrls]
    FOREIGN KEY ([InfoTypeId])
    REFERENCES [dbo].[InfoTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InfoTypeInfoSourceRssUrls'
CREATE INDEX [IX_FK_InfoTypeInfoSourceRssUrls]
ON [dbo].[InfoSourceRssUrls]
    ([InfoTypeId]);
GO

-- Creating foreign key on [InfoType_Id] in table 'InfoTypeSortOrders'
ALTER TABLE [dbo].[InfoTypeSortOrders]
ADD CONSTRAINT [FK_InfoTypeInfoTypeSortOrder]
    FOREIGN KEY ([InfoType_Id])
    REFERENCES [dbo].[InfoTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InfoTypeInfoTypeSortOrder'
CREATE INDEX [IX_FK_InfoTypeInfoTypeSortOrder]
ON [dbo].[InfoTypeSortOrders]
    ([InfoType_Id]);
GO

-- Creating foreign key on [InfoSource_Id] in table 'InfoSourceSortOrders'
ALTER TABLE [dbo].[InfoSourceSortOrders]
ADD CONSTRAINT [FK_InfoSourceInfoSourceSortOrder]
    FOREIGN KEY ([InfoSource_Id])
    REFERENCES [dbo].[InfoSources]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InfoSourceInfoSourceSortOrder'
CREATE INDEX [IX_FK_InfoSourceInfoSourceSortOrder]
ON [dbo].[InfoSourceSortOrders]
    ([InfoSource_Id]);
GO

-- Creating foreign key on [Article_Id] in table 'FeaturedArticles'
ALTER TABLE [dbo].[FeaturedArticles]
ADD CONSTRAINT [FK_ArticlesFeaturedArticles]
    FOREIGN KEY ([Article_Id])
    REFERENCES [dbo].[Articles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ArticlesFeaturedArticles'
CREATE INDEX [IX_FK_ArticlesFeaturedArticles]
ON [dbo].[FeaturedArticles]
    ([Article_Id]);
GO

-- Creating foreign key on [TrendingGamesId] in table 'TrendingArticles'
ALTER TABLE [dbo].[TrendingArticles]
ADD CONSTRAINT [FK_TrendingGamesTrendingArticles]
    FOREIGN KEY ([TrendingGamesId])
    REFERENCES [dbo].[TrendingGames]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_TrendingGamesTrendingArticles'
CREATE INDEX [IX_FK_TrendingGamesTrendingArticles]
ON [dbo].[TrendingArticles]
    ([TrendingGamesId]);
GO

-- Creating foreign key on [GamesId] in table 'TrendingGames'
ALTER TABLE [dbo].[TrendingGames]
ADD CONSTRAINT [FK_GamesTrendingGames]
    FOREIGN KEY ([GamesId])
    REFERENCES [dbo].[Games]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_GamesTrendingGames'
CREATE INDEX [IX_FK_GamesTrendingGames]
ON [dbo].[TrendingGames]
    ([GamesId]);
GO

-- Creating foreign key on [InfoTypeId] in table 'TrendingGames'
ALTER TABLE [dbo].[TrendingGames]
ADD CONSTRAINT [FK_InfoTypeTrendingGames]
    FOREIGN KEY ([InfoTypeId])
    REFERENCES [dbo].[InfoTypes]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_InfoTypeTrendingGames'
CREATE INDEX [IX_FK_InfoTypeTrendingGames]
ON [dbo].[TrendingGames]
    ([InfoTypeId]);
GO

-- Creating foreign key on [ArticlesId] in table 'TrendingArticles'
ALTER TABLE [dbo].[TrendingArticles]
ADD CONSTRAINT [FK_ArticlesTrendingArticles]
    FOREIGN KEY ([ArticlesId])
    REFERENCES [dbo].[Articles]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_ArticlesTrendingArticles'
CREATE INDEX [IX_FK_ArticlesTrendingArticles]
ON [dbo].[TrendingArticles]
    ([ArticlesId]);
GO

-- Creating foreign key on [PollId] in table 'PollAnswers'
ALTER TABLE [dbo].[PollAnswers]
ADD CONSTRAINT [FK_PollPollAnswers]
    FOREIGN KEY ([PollId])
    REFERENCES [dbo].[Polls]
        ([Id])
    ON DELETE NO ACTION ON UPDATE NO ACTION;

-- Creating non-clustered index for FOREIGN KEY 'FK_PollPollAnswers'
CREATE INDEX [IX_FK_PollPollAnswers]
ON [dbo].[PollAnswers]
    ([PollId]);
GO

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------