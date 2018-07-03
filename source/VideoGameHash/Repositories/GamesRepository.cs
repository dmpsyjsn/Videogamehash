using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;
using VideoGameHash.Models;
using VideoGameHash.Models.TheGamesDB;

namespace VideoGameHash.Repositories
{
    public interface IGamesRepository
    {
        Task<IEnumerable<Games>> GetGames();
        Task<IEnumerable<string>> SearchGameTitles(string search);
        Task AddGame(string gameSystem);
        Task<Games> GetGame(string gameTitle);
        Task<Dictionary<int, string>> GetTrendingGames(int count);
        Task<Dictionary<int, string>> GetPopularGames(int count);
        Task DeleteGame(int id);
        Task<GameDetailsModel> GetGameDetailsViewModel(int id, bool useInfometrics);
    }

    public class GamesRepository : IGamesRepository
    {
        private readonly VGHDatabaseContainer _db;

        public GamesRepository(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public async Task<IEnumerable<Games>> GetGames()
        {
            return await _db.Games.OrderBy(u => u.GameTitle).ToListAsync();
        }

        public async Task<IEnumerable<string>> SearchGameTitles(string search)
        {
            var searchTerm = new Regex($@"{search}", RegexOptions.IgnoreCase);

            var games = (await _db.Games.ToListAsync()).Where(d => searchTerm.IsMatch(d.GameTitle)).Take(10).Select(x => x.GameTitle).ToList();

            return games;
        }

        public async Task AddGame(string gameSystem)
        {
            if (gameSystem == "All") return;

            var url = $"http://thegamesdb.net/api/PlatformGames.php?platform={GetPlatform(gameSystem)}";

            await ProcessGamesFromWebService(url, gameSystem);

            await ProcessAdditionalDetailsFromWebService(gameSystem);
        }

        public async Task<Games> GetGame(string gameTitle)
        {
            return await _db.Games.SingleOrDefaultAsync(u => u.GameTitle == gameTitle);
        }

        public async Task<Dictionary<int, string>> GetTrendingGames(int count)
        {
            var trendingGames = await _db.TrendingGames.ToListAsync();
            return trendingGames.OrderByDescending(x => x.ArticleHits).Take(10).ToDictionary(x => x.Game.Id, x => x.Game.GameTitle);
        }

        public async Task<Dictionary<int, string>> GetPopularGames(int count)
        {
            var popularGames = await _db.PopularGames.ToListAsync();
            return popularGames.OrderByDescending(x => x.ArticleHits).Take(10).ToDictionary(x => x.Game.Id, x => x.Game.GameTitle);
        }

        public async Task DeleteGame(int id)
        {
            var game = await GetGame(id);

            if (game != null)
            {
                var gameIgnore = new GameIgnore
                {
                    GameTitle = game.GameTitle
                };

                if (!await IsDuplicateIgnoredGame(game))
                    _db.GameIgnores.Add(gameIgnore);

                // Delete from GameInfo first
                await DeleteGameInfo(game.Id);

                // Delete from Trending Games
                var trendingGame = await _db.TrendingGames.SingleOrDefaultAsync(x => x.GamesId.Equals(game.Id));
                if (trendingGame != null)
                    _db.TrendingGames.Remove(trendingGame);

                // Delete from all time games list
                var allTimeGame = await _db.PopularGames.SingleOrDefaultAsync(x => x.GamesId.Equals(game.Id));
                if (allTimeGame != null)
                    _db.PopularGames.Remove(allTimeGame);

                _db.Games.Remove(game);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<GameDetailsModel> GetGameDetailsViewModel(int id, bool useInfometrics)
        {
            var game = await GetGame(id);

            if (game == null) return null;

            var model = new GameDetailsModel
            {
                Game = game,
                UseInfoMetrics = useInfometrics,
                AvailableGameSystems = await GetGameSystemsForThisGame(game)
            };
            
            // Image Links
            var links = new Dictionary<string, string>();
            foreach (var system in model.AvailableGameSystems)
            {
                var systemId = await GetGameSystemId(system);

                var link = await _db.GameInfoes.SingleOrDefaultAsync(u => u.GamesId == id && u.GameSystemId == systemId);

                links[system] = link?.GameImage ?? string.Empty;
            }

            model.ImageLinks = links;
            var currentGameSystem = model.AvailableGameSystems[0];
            var gameSystemId = await GetGameSystemId(currentGameSystem);
            var gameInfo = await _db.GameInfoes.SingleOrDefaultAsync(u => u.GamesId == id && u.GameSystemId == gameSystemId);
            
            model.Publisher = gameInfo?.Publisher ?? string.Empty;
            model.Developer = gameInfo?.Developer ?? string.Empty;
            model.Overview = gameInfo?.Overview ?? string.Empty;
            model.GamesDbNetId = gameInfo?.GamesDbNetId ?? -1;
            model.UsReleaseDate = gameInfo?.USReleaseDate ?? DateTime.MinValue;

            return model;
        }

        #region Private methods
        
        private async Task<Games> GetGame(int id)
        {
            return await _db.Games.SingleOrDefaultAsync(u => u.Id == id);
        }

        private async Task<int> GetGameSystemId(string gameSystem)
        {
            return (await _db.GameSystems.SingleOrDefaultAsync(u => u.GameSystemName == gameSystem))?.Id ?? -1;
        }

        private static string GetPlatform(string gameSystem)
        {
            if (gameSystem == "Xbox 360")
                gameSystem = "Microsoft+Xbox+360";
            else if (gameSystem == "Xbox One")
                gameSystem = "Microsoft+Xbox+One";
            else if (gameSystem == "Wii U")
                gameSystem = "Nintendo+Wii+U";
            else if (gameSystem == "PS3")
                gameSystem = "Sony+Playstation+3";
            else if (gameSystem == "PS4")
                gameSystem = "Sony+Playstation+4";
            else if (gameSystem == "Switch")
                gameSystem = "Nintendo+Switch";

            return gameSystem;
        }

        private async Task ProcessGamesFromWebService(string url, string gameSystem)
        {
            var request = WebRequest.Create(url);
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var serializer = new XmlSerializer(typeof(PlatformGamesData));

                var gameResponse = (PlatformGamesData) serializer.Deserialize(response.GetResponseStream() ?? throw new InvalidOperationException());
                var cutoff = DateTime.Now.AddMonths(-3);
                var items = gameResponse.Games.Where(u => !string.IsNullOrEmpty(u?.Thumb) && !string.IsNullOrEmpty(u.ReleaseDate) && u.ReleaseDate.IndexOf('/') > 0 && Convert.ToDateTime(u.ReleaseDate) >= cutoff).ToArray();
                foreach (var game in items)
                {
                    var gameDb = new Games();
                    var usReleaseDate = new DateTime();

                    if (game.GameTitle != null)
                        gameDb.GameTitle = game.GameTitle;

                    if (game.ReleaseDate != null && game.ReleaseDate.IndexOf('/') > 0)
                        usReleaseDate = Convert.ToDateTime(game.ReleaseDate);

                    if (gameDb.GameTitle == null || usReleaseDate == DateTime.MinValue || IgnoreThisGame(gameDb)) continue;

                    if (!await IsDuplicateGame(gameDb))
                    {
                        _db.Games.Add(gameDb);
                        _db.SaveChanges();
                    }
                    else
                    {
                        gameDb = await GetGame(gameDb.GameTitle);
                    }

                    var gameInfo = new GameInfo
                    {
                        GamesId = gameDb.Id,
                        GameSystemId = await GetGameSystemId(gameSystem),
                        USReleaseDate = usReleaseDate,
                        GamesDbNetId = Convert.ToInt32(game.Id),
                        GameImage = "http://thegamesdb.net/banners/" + game.Thumb
                    };

                    if (await IsDuplicateGameInfo(gameInfo)) continue;

                    _db.GameInfoes.Add(gameInfo);
                    _db.SaveChanges();
                }
            }
        }

        private bool IgnoreThisGame(Games game)
        {
            return _db.GameIgnores.Any(x => x.GameTitle.Equals(game.GameTitle, StringComparison.OrdinalIgnoreCase));
        }

        private async Task<bool> IsDuplicateGame(Games game)
        {
            return await _db.Games.AnyAsync(x => x.GameTitle.Equals(game.GameTitle));
        }

        private async Task<bool> IsDuplicateGameInfo(GameInfo gameInfo)
        {
            return await _db.GameInfoes.AnyAsync(x => x.GamesId.Equals(gameInfo.GamesId) && x.GameSystemId.Equals(gameInfo.GameSystemId));
        }

        private async Task ProcessAdditionalDetailsFromWebService(string gameSystem)
        {
            var gameInfos = await GetGameInfoBySystem(gameSystem);
            var deleteTheseGames = new List<int>();
            foreach (var gameInfo in gameInfos)
                if (string.IsNullOrWhiteSpace(gameInfo.Publisher)) // TO DO: Add an update boolean here
                    try
                    {
                        var url =
                            $"http://thegamesdb.net/api/GetGame.php?id={gameInfo.GamesDbNetId}&platform={GetPlatform(gameSystem)}";
                        var request = WebRequest.Create(url);
                        using (var response = (HttpWebResponse) request.GetResponse())
                        {
                            var serializer = new XmlSerializer(typeof(DataByGameId));

                            var gameResponse = (DataByGameId) serializer.Deserialize(response.GetResponseStream() ?? throw new InvalidOperationException());

                            if (!string.IsNullOrWhiteSpace(gameResponse.Game[0].Publisher) ||
                                !string.IsNullOrWhiteSpace(gameResponse.Game[0].Developer))
                            {
                                gameInfo.Publisher = gameResponse.Game[0].Publisher;
                                gameInfo.Developer = gameResponse.Game[0].Developer;
                                gameInfo.Overview = gameResponse.Game[0].Overview;
                            }
                            else
                            {
                                deleteTheseGames.Add(gameInfo.Game.Id);
                            }
                        }
                    }
                    catch
                    {
                        break;
                    }

            await _db.SaveChangesAsync();

            foreach (var game in deleteTheseGames)
                await DeleteGameFromGameInfoes(game);
        }

        private async Task<IEnumerable<GameInfo>> GetGameInfoBySystem(string gameSystem)
        {
            var gameSystemId = await GetGameSystemId(gameSystem);
            return await _db.GameInfoes.Where(u => u.GameSystemId == gameSystemId).ToListAsync();
        }

        private async Task DeleteGameFromGameInfoes(int gameId)
        {
            IEnumerable<GameInfo> infos = await _db.GameInfoes.Where(u => u.GamesId == gameId).ToListAsync();

            foreach (var info in infos)
                _db.GameInfoes.Remove(info);

            await _db.SaveChangesAsync();

            var game = await GetGame(gameId);

            if (game != null)
                _db.Games.Remove(game);

            await _db.SaveChangesAsync();
        }

        private async Task<bool> IsDuplicateIgnoredGame(Games game)
        {
            return await _db.GameIgnores.AnyAsync(x => x.GameTitle.Equals(game.GameTitle, StringComparison.OrdinalIgnoreCase));
        }

        private async Task DeleteGameInfo(int id)
        {
            var gameInfos = await _db.GameInfoes.Where(u => u.GamesId == id).ToListAsync();
            foreach (var gameInfo in gameInfos)
                _db.GameInfoes.Remove(gameInfo);
            await _db.SaveChangesAsync();
        }

        private async Task<List<string>> GetGameSystemsForThisGame(Games game)
        {
            var gameSystem = new List<string>();
            var gameSystemList = await _db.GameSystems
                .Where(x => !x.GameSystemName.Equals("All", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.GameSystemSortOrder.SortOrder).ToListAsync();
            foreach (var system in gameSystemList)
            {
                var hasGameInfos = await _db.GameInfoes.AnyAsync(x => x.GameSystemId.Equals(system.Id) && x.GamesId.Equals(game.Id));

                if (hasGameInfos)
                    gameSystem.Add(system.GameSystemName);
            }

            return gameSystem;
        }

        #endregion
    }


    #region code generated from schema
    // 
    // This source code was auto-generated by xsd, Version=2.0.50727.3038.
    // 


    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "Data")]
    public class DataByPlatform
    {
        [XmlElement("DataGame")] private DataGameByPlatform[] _itemsField;

        /// <remarks />
        [XmlElement("Game", Form = XmlSchemaForm.Unqualified)]
        public DataGameByPlatform[] Items
        {
            get => _itemsField;
            set => _itemsField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameByPlatform
    {
        private string _gameTitleField;

        private string _idField;

        private string _releaseDateField;

        private string _thumbField;

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Id
        {
            get => _idField;
            set => _idField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string GameTitle
        {
            get => _gameTitleField;
            set => _gameTitleField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ReleaseDate
        {
            get => _releaseDateField;
            set => _releaseDateField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Thumb
        {
            get => _thumbField;
            set => _thumbField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = true)]
    public class Original
    {
        private string _heightField;

        private string _valueField;

        private string _widthField;

        /// <remarks />
        [XmlAttribute]
        public string Width
        {
            get => _widthField;
            set => _widthField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string Height
        {
            get => _heightField;
            set => _heightField = value;
        }

        /// <remarks />
        [XmlText]
        public string Value
        {
            get => _valueField;
            set => _valueField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false, ElementName = "Data")]
    public class DataByGameId
    {
        private string _baseImgUrlField;

        [XmlElement("DataGame")] private DataGameByGameId[] _gameField;

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string BaseImgUrl
        {
            get => _baseImgUrlField;
            set => _baseImgUrlField = value;
        }

        /// <remarks />
        [XmlElement("Game", Form = XmlSchemaForm.Unqualified)]
        public DataGameByGameId[] Game
        {
            get => _gameField;
            set => _gameField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameByGameId
    {
        private string _coopField;

        private string _developerField;

        private string _eSrbField;

        private string _gameTitleField;

        private DataGameGenres[] _genresField;

        private string _idField;

        private DataGameImages[] _imagesField;

        private string _overviewField;

        private string _platformField;

        private string _platformIdField;

        private string _playersField;

        private string _publisherField;

        private string _ratingField;

        private string _releaseDateField;

        private string _youtubeField;

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Id
        {
            get => _idField;
            set => _idField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string GameTitle
        {
            get => _gameTitleField;
            set => _gameTitleField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string PlatformId
        {
            get => _platformIdField;
            set => _platformIdField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Platform
        {
            get => _platformField;
            set => _platformField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ReleaseDate
        {
            get => _releaseDateField;
            set => _releaseDateField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Overview
        {
            get => _overviewField;
            set => _overviewField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Esrb
        {
            get => _eSrbField;
            set => _eSrbField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Players
        {
            get => _playersField;
            set => _playersField = value;
        }

        /// <remarks />
        [XmlElement("Co-op", Form = XmlSchemaForm.Unqualified)]
        public string Coop
        {
            get => _coopField;
            set => _coopField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Youtube
        {
            get => _youtubeField;
            set => _youtubeField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Publisher
        {
            get => _publisherField;
            set => _publisherField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Developer
        {
            get => _developerField;
            set => _developerField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Rating
        {
            get => _ratingField;
            set => _ratingField = value;
        }

        /// <remarks />
        [XmlElement("Genres", Form = XmlSchemaForm.Unqualified)]
        public DataGameGenres[] Genres
        {
            get => _genresField;
            set => _genresField = value;
        }

        /// <remarks />
        [XmlElement("Images", Form = XmlSchemaForm.Unqualified)]
        public DataGameImages[] Images
        {
            get => _imagesField;
            set => _imagesField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameGenres
    {
        private string _genreField;

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Genre
        {
            get => _genreField;
            set => _genreField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameImages
    {
        private DataGameImagesBanner[] _bannerField;

        private DataGameImagesBoxart[] _boxartField;

        private DataGameImagesClearlogo[] _clearlogoField;

        private DataGameImagesFanart[] _fanartField;

        private DataGameImagesScreenshot[] _screenshotField;

        /// <remarks />
        [XmlElement("fanart", Form = XmlSchemaForm.Unqualified)]
        public DataGameImagesFanart[] Fanart
        {
            get => _fanartField;
            set => _fanartField = value;
        }

        /// <remarks />
        [XmlElement("boxart", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public DataGameImagesBoxart[] Boxart
        {
            get => _boxartField;
            set => _boxartField = value;
        }

        /// <remarks />
        [XmlElement("banner", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public DataGameImagesBanner[] Banner
        {
            get => _bannerField;
            set => _bannerField = value;
        }

        /// <remarks />
        [XmlElement("screenshot", Form = XmlSchemaForm.Unqualified)]
        public DataGameImagesScreenshot[] Screenshot
        {
            get => _screenshotField;
            set => _screenshotField = value;
        }

        /// <remarks />
        [XmlElement("clearlogo", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public DataGameImagesClearlogo[] Clearlogo
        {
            get => _clearlogoField;
            set => _clearlogoField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameImagesFanart
    {
        private Original[] _originalField;

        private string _thumbField;

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Thumb
        {
            get => _thumbField;
            set => _thumbField = value;
        }

        /// <remarks />
        [XmlElement("original", IsNullable = true)]
        public Original[] Original
        {
            get => _originalField;
            set => _originalField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameImagesBoxart
    {
        private string _heightField;

        private string _sideField;

        private string _thumbField;

        private string _valueField;

        private string _widthField;

        /// <remarks />
        [XmlAttribute]
        public string Side
        {
            get => _sideField;
            set => _sideField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string Width
        {
            get => _widthField;
            set => _widthField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string Height
        {
            get => _heightField;
            set => _heightField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string Thumb
        {
            get => _thumbField;
            set => _thumbField = value;
        }

        /// <remarks />
        [XmlText]
        public string Value
        {
            get => _valueField;
            set => _valueField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameImagesBanner
    {
        private string _heightField;

        private string _valueField;

        private string _widthField;

        /// <remarks />
        [XmlAttribute]
        public string Width
        {
            get => _widthField;
            set => _widthField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string Height
        {
            get => _heightField;
            set => _heightField = value;
        }

        /// <remarks />
        [XmlText]
        public string Value
        {
            get => _valueField;
            set => _valueField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameImagesScreenshot
    {
        private Original[] _originalField;

        private string _thumbField;

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Thumb
        {
            get => _thumbField;
            set => _thumbField = value;
        }

        /// <remarks />
        [XmlElement("original", IsNullable = true)]
        public Original[] Original
        {
            get => _originalField;
            set => _originalField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameImagesClearlogo
    {
        private string _heightField;

        private string _valueField;

        private string _widthField;

        /// <remarks />
        [XmlAttribute]
        public string Width
        {
            get => _widthField;
            set => _widthField = value;
        }

        /// <remarks />
        [XmlAttribute]
        public string Height
        {
            get => _heightField;
            set => _heightField = value;
        }

        /// <remarks />
        [XmlText]
        public string Value
        {
            get => _valueField;
            set => _valueField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class NewDataSet
    {
        private object[] _itemsField;

        /// <remarks />
        [XmlElement("Data", typeof(DataByGameId))]
        [XmlElement("original", typeof(Original), IsNullable = true)]
        public object[] Items
        {
            get => _itemsField;
            set => _itemsField = value;
        }
    }

    ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    //------------------------------------------------------------------------------
    // <auto-generated>
    //     This code was generated by a tool.
    //     Runtime Version:2.0.50727.4241
    //
    //     Changes to this file may cause incorrect behavior and will be lost if
    //     the code is regenerated.
    // </auto-generated>
    //------------------------------------------------------------------------------


    // 
    // This source code was auto-generated by xsd, Version=2.0.50727.3038.
    // 


    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Data
    {
        private string _baseImgUrlField;

        private DataGame[] _gameField;

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string BaseImgUrl
        {
            get => _baseImgUrlField;
            set => _baseImgUrlField = value;
        }

        /// <remarks />
        [XmlElement("Game", Form = XmlSchemaForm.Unqualified)]
        public DataGame[] Game
        {
            get => _gameField;
            set => _gameField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGame
    {
        private DataGameAlternateTitlesTitle[] _alternateTitlesField;

        private string _coopField;

        private string _developerField;

        private string _eSrbField;

        private string _gameTitleField;

        private DataGameGenresGenre[] _genresField;

        private string _idField;

        private DataGameImages[] _imagesField;

        private string _overviewField;

        private string _platformField;

        private string _platformIdField;

        private string _playersField;

        private string _publisherField;

        private string _ratingField;

        private string _releaseDateField;

        private string _youtubeField;

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Id
        {
            get => _idField;
            set => _idField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string GameTitle
        {
            get => _gameTitleField;
            set => _gameTitleField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string PlatformId
        {
            get => _platformIdField;
            set => _platformIdField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Platform
        {
            get => _platformField;
            set => _platformField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string ReleaseDate
        {
            get => _releaseDateField;
            set => _releaseDateField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Overview
        {
            get => _overviewField;
            set => _overviewField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Esrb
        {
            get => _eSrbField;
            set => _eSrbField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Players
        {
            get => _playersField;
            set => _playersField = value;
        }

        /// <remarks />
        [XmlElement("Co-op", Form = XmlSchemaForm.Unqualified)]
        public string Coop
        {
            get => _coopField;
            set => _coopField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Youtube
        {
            get => _youtubeField;
            set => _youtubeField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Publisher
        {
            get => _publisherField;
            set => _publisherField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Developer
        {
            get => _developerField;
            set => _developerField = value;
        }

        /// <remarks />
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string Rating
        {
            get => _ratingField;
            set => _ratingField = value;
        }

        /// <remarks />
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("title", typeof(DataGameAlternateTitlesTitle), Form = XmlSchemaForm.Unqualified)]
        public DataGameAlternateTitlesTitle[] AlternateTitles
        {
            get => _alternateTitlesField;
            set => _alternateTitlesField = value;
        }

        /// <remarks />
        [XmlArray(Form = XmlSchemaForm.Unqualified)]
        [XmlArrayItem("genre", typeof(DataGameGenresGenre), Form = XmlSchemaForm.Unqualified)]
        public DataGameGenresGenre[] Genres
        {
            get => _genresField;
            set => _genresField = value;
        }

        /// <remarks />
        [XmlElement("Images", Form = XmlSchemaForm.Unqualified)]
        public DataGameImages[] Images
        {
            get => _imagesField;
            set => _imagesField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameAlternateTitlesTitle
    {
        private string _valueField;

        /// <remarks />
        [XmlText]
        public string Value
        {
            get => _valueField;
            set => _valueField = value;
        }
    }

    /// <remarks />
    [GeneratedCode("xsd", "2.0.50727.3038")]
    [Serializable]
    [DebuggerStepThrough]
    [DesignerCategory("code")]
    [XmlType(AnonymousType = true)]
    public class DataGameGenresGenre
    {
        private string _valueField;

        /// <remarks />
        [XmlText]
        public string Value
        {
            get => _valueField;
            set => _valueField = value;
        }
    }

    #endregion
}