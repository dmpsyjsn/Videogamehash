using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using System.Xml.Serialization;
using VideoGameHash.Models;
using VideoGameHash.Models.TheGamesDB;

namespace VideoGameHash.Repositories
{
    public class GamesRepository
    {
        private readonly VGHDatabaseContainer _db;

        public GamesRepository(VGHDatabaseContainer db)
        {
            _db = db;
        }

        public IEnumerable<Games> GetGames()
        {
            return _db.Games.OrderBy(u => u.GameTitle);
        }

        public IEnumerable<string> SearchGameTitles(string search)
        {
            var searchTerm = new Regex($@"{search}", RegexOptions.IgnoreCase);

            var games = _db.Games.AsEnumerable().Where(d => searchTerm.IsMatch(d.GameTitle)).Take(10).Select(x => x.GameTitle).ToList();

            return games;
        }

        public void AddGame(string gameSystem)
        {
            if (gameSystem == "All") return;

            var url = $"http://thegamesdb.net/api/PlatformGames.php?platform={GetPlatform(gameSystem)}";

            ProcessGamesFromWebService(url, gameSystem);

            ProcessAdditionalDetailsFromWebService(gameSystem);
        }

        public Games GetGame(int id)
        {
            return _db.Games.SingleOrDefault(u => u.Id == id);
        }

        public Games GetGame(string gameTitle)
        {
            return _db.Games.SingleOrDefault(u => u.GameTitle == gameTitle);
        }

        public int GetGameSystemId(string gameSystem)
        {
            return _db.GameSystems.SingleOrDefault(u => u.GameSystemName == gameSystem)?.Id ?? -1;
        }

        public string GetPlatform(string gameSystem)
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

        public void ProcessGamesFromWebService(string url, string gameSystem)
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

                    if (!IsDuplicateGame(gameDb))
                    {
                        _db.Games.AddObject(gameDb);
                        _db.SaveChanges();
                    }
                    else
                    {
                        gameDb = GetGame(gameDb.GameTitle);
                    }

                    var gameInfo = new GameInfo
                    {
                        GamesId = gameDb.Id,
                        GameSystemId = GetGameSystemId(gameSystem),
                        USReleaseDate = usReleaseDate,
                        GamesDbNetId = Convert.ToInt32(game.Id),
                        GameImage = "http://thegamesdb.net/banners/" + game.Thumb
                    };

                    if (IsDuplicateGameInfo(gameInfo)) continue;

                    _db.GameInfoes.AddObject(gameInfo);
                    _db.SaveChanges();
                }
            }
        }

        private void DeleteGameFromGameInfoes(int gameId)
        {
            IEnumerable<GameInfo> infos = _db.GameInfoes.Where(u => u.GamesId == gameId);

            foreach (var info in infos)
                _db.GameInfoes.DeleteObject(info);

            _db.SaveChanges();

            var game = GetGame(gameId);

            if (game != null)
                _db.Games.DeleteObject(game);
        }

        private void ProcessAdditionalDetailsFromWebService(string gameSystem)
        {
            var gameInfos = GetGameInfoBySystem(gameSystem);
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

            _db.SaveChanges();

            foreach (var game in deleteTheseGames)
                DeleteGameFromGameInfoes(game);
        }

        private IEnumerable<GameInfo> GetGameInfoBySystem(string gameSystem)
        {
            var gameSystemId = GetGameSystemId(gameSystem);
            return _db.GameInfoes.Where(u => u.GameSystemId == gameSystemId);
        }

        private bool IsDuplicateGameInfo(GameInfo gameInfo)
        {
            var temp = from tempDb in _db.GameInfoes
                where tempDb.GamesId == gameInfo.GamesId && tempDb.GameSystemId == gameInfo.GameSystemId
                select tempDb;

            return temp.Any();
        }

        public bool IgnoreThisGame(Games game)
        {
            return _db.GameIgnores.Any(x => x.GameTitle.Equals(game.GameTitle, StringComparison.OrdinalIgnoreCase));
        }

        public IQueryable<Games> GetGamesQuery()
        {
            return _db.Games;
        }

        public List<KeyValuePair<int, string>> GetTrendingGames(int count)
        {
            return _db.TrendingGames.OrderByDescending(x => x.ArticleHits).Take(10).AsEnumerable().Select(x => new KeyValuePair<int, string>(x.Game.Id, x.Game.GameTitle)).ToList();
        }

        public List<KeyValuePair<int, string>> GetPopularGames(int count)
        {
            return _db.PopularGames.OrderByDescending(x => x.ArticleHits).Take(10).AsEnumerable().Select(x => new KeyValuePair<int, string>(x.Game.Id, x.Game.GameTitle)).ToList();
        }

        public bool IsDuplicateGame(Games game)
        {
            var isDuplicate = from tempGameItem in GetGamesQuery()
                where game.GameTitle == tempGameItem.GameTitle
                select tempGameItem;

            return isDuplicate.Any();
        }

        public void DeleteGame(int id)
        {
            var game = GetGame(id);

            if (game != null)
            {
                var gameIgnore = new GameIgnore
                {
                    GameTitle = game.GameTitle
                };

                if (!IsDuplicateIgnoredGame(game))
                    _db.GameIgnores.AddObject(gameIgnore);

                // Delete from GameInfo first
                DeleteGameInfo(game.Id);

                // Delete from Trending Games
                var trendingGame = _db.TrendingGames.SingleOrDefault(x => x.GamesId.Equals(game.Id));
                if (trendingGame != null)
                    _db.TrendingGames.DeleteObject(trendingGame);

                // Delete from all time games list
                var allTimeGame = _db.PopularGames.SingleOrDefault(x => x.GamesId.Equals(game.Id));
                if (allTimeGame != null)
                    _db.PopularGames.DeleteObject(allTimeGame);

                _db.Games.DeleteObject(game);
                _db.SaveChanges();
            }
        }

        private bool IsDuplicateIgnoredGame(Games game)
        {
            return _db.GameIgnores.Any(x => x.GameTitle.Equals(game.GameTitle, StringComparison.OrdinalIgnoreCase));
        }

        private void DeleteGameInfo(int id)
        {
            var gameInfos = _db.GameInfoes.Where(u => u.GamesId == id);
            foreach (var gameInfo in gameInfos)
                _db.GameInfoes.DeleteObject(gameInfo);
            _db.SaveChanges();
        }

        internal Dictionary<string, string> GetImages(int id, List<string> gameSystems)
        {
            try
            {
                var links = new Dictionary<string, string>();
                foreach (var system in gameSystems)
                {
                    var gameSystemId = GetGameSystemId(system);

                    var link = _db.GameInfoes.SingleOrDefault(u => u.GamesId == id && u.GameSystemId == gameSystemId);

                    links[system] = link?.GameImage ?? string.Empty;
                }
                
                return links;
            }
            catch
            {
                return new Dictionary<string, string>();
            }
        }

        internal string GetPublisher(int id, string gameSystem)
        {
            try
            {
                var gameSystemId = GetGameSystemId(gameSystem);
                return _db.GameInfoes.Single(u => u.GamesId == id && u.GameSystemId == gameSystemId).Publisher;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal string GetDeveloper(int id, string gameSystem)
        {
            try
            {
                var gameSystemId = GetGameSystemId(gameSystem);
                return _db.GameInfoes.Single(u => u.GamesId == id && u.GameSystemId == gameSystemId).Developer;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal string GetOverview(int id, string gameSystem)
        {
            try
            {
                var gameSystemId = GetGameSystemId(gameSystem);
                return _db.GameInfoes.Single(u => u.GamesId == id && u.GameSystemId == gameSystemId).Overview;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal int GetGamesDbNetId(int id, string gameSystem)
        {
            try
            {
                var gameSystemId = GetGameSystemId(gameSystem);
                return _db.GameInfoes.Single(u => u.GamesId == id && u.GameSystemId == gameSystemId)
                    .GamesDbNetId;
            }
            catch
            {
                return -1;
            }
        }

        internal DateTime GetReleaseDate(int id, string gameSystem)
        {
            try
            {
                var gameSystemId = GetGameSystemId(gameSystem);
                return _db.GameInfoes.Single(u => u.GamesId == id && u.GameSystemId == gameSystemId)
                    .USReleaseDate;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        internal List<string> GetGameSystemsForThisGame(Games game)
        {
            var gameSystem = new List<string>();
            var gameSystemList = _db.GameSystems
                .Where(x => !x.GameSystemName.Equals("All", StringComparison.OrdinalIgnoreCase))
                .OrderBy(x => x.GameSystemSortOrder.SortOrder).ToList();
            foreach (var system in gameSystemList)
            {
                var hasGameInfos = _db.GameInfoes.Any(x => x.GameSystemId.Equals(system.Id) && x.GamesId.Equals(game.Id));

                if (hasGameInfos)
                    gameSystem.Add(system.GameSystemName);
            }

            return gameSystem;
        }
    }


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
}