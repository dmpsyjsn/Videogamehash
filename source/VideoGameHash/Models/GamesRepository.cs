using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace VideoGameHash.Models
{
    public class GamesRepository
    {
        private readonly VGHDatabaseContainer _db;
        private readonly InfoRepository _infoRepository;

        public GamesRepository(VGHDatabaseContainer db, InfoRepository infoRepository)
        {
            _db = db;
            _infoRepository = infoRepository;
        }

        public IEnumerable<Games> GetGames()
        {
            return _db.Games.OrderBy(u => u.GameTitle);
        }

        public IEnumerable<Games> GetSortedGames()
        {
            return _db.Games.Where(u => u.GameInfoes.Count > 0).OrderBy(u => u.GameTitle);
        }

        public IEnumerable<Games> GetSortedGamesByLetter(char letter)
        {
            var returnedList = new List<Games>();
            returnedList.AddRange(letter == '0'
                ? _db.Games.Where(u => u.GameInfoes.Count > 0).OrderBy(u => u.GameTitle)
                    .Where(game => game.GameTitle[0] >= '0' && game.GameTitle[0] <= '9')
                : _db.Games.Where(u => u.GameInfoes.Count > 0).OrderBy(u => u.GameTitle)
                    .Where(x => x.GameTitle[0] == letter));

            return returnedList.AsEnumerable();
        }

        public IEnumerable<Games> SearchGames(string search)
        {
            return _db.Games.Where(u => u.GameInfoes.Count > 0 && u.GameTitle.Contains(search)).Take(20)
                .OrderBy(u => u.GameTitle);
        }

        public void AddGame(string gameSystem)
        {
            try
            {
                if (gameSystem != "All")
                {
                    var url = $"http://thegamesdb.net/api/PlatformGames.php?platform={GetPlatform(gameSystem)}";

                    ProcessGamesFromWebService(url, gameSystem);

                    ProcessAdditionalDetailsFromWebService(gameSystem);
                }
            }
            catch
            {
                // Do nothing
            }
        }

        public void AddGameWikipedia(string gameSystem)
        {
            try
            {
                if (gameSystem != "All")
                {
                    var url =
                        $"http://en.wikipedia.org/w/api.php?format=json&action=query&titles={GetWikipediaPlatform(gameSystem)}&prop=revisions&rvprop=content&rvsection=1";

                    ProcessGamesFromWikipediaWebService(url, gameSystem);

                    //ProcessAdditionalDetailsFromWikipediaWebService(GameSystem);
                }
            }
            catch
            {
                // Do nothing
            }
        }

        public Games GetGame(int id)
        {
            return _db.Games.SingleOrDefault(u => u.Id == id);
        }

        public Games GetGameByGameTitle(string gameTitle)
        {
            return _db.Games.SingleOrDefault(u => u.GameTitle == gameTitle);
        }

        public int GetGameSystemId(string gameSystem)
        {
            return _db.GameSystems.SingleOrDefault(u => u.GameSystemName == gameSystem).Id;
        }

        public bool ContainsEntries(string gameTitle)
        {
            return _infoRepository.ContainsArticles(gameTitle);
        }

        public bool ContainsEntries(string gameTitle, string gameSystem)
        {
            return _infoRepository.ContainsArticles(gameTitle, gameSystem);
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

            return gameSystem;
        }

        public string GetWikipediaPlatform(string gameSystem)
        {
            if (gameSystem == "Xbox 360")
                gameSystem = "List of Xbox 360 games";
            else if (gameSystem == "Xbox One")
                gameSystem = "List of Xbox One games";
            else if (gameSystem == "Wii U")
                gameSystem = "List of Wii U games";
            else if (gameSystem == "PS3")
                gameSystem = "List of PlayStation 3 games";
            else if (gameSystem == "PS4")
                gameSystem = "List_of_PlayStation_4_games";

            return gameSystem;
        }

        public void ProcessGamesFromWebService(string url, string gameSystem)
        {
            var request = WebRequest.Create(url);
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var serializer = new XmlSerializer(typeof(DataByPlatform));

                var gameResponse = (DataByPlatform) serializer.Deserialize(response.GetResponseStream());
                var cutoff = DateTime.Now.AddMonths(-3);
                var items = gameResponse.Items.Where(u =>
                    u.ReleaseDate != null && u.ReleaseDate.IndexOf('/') > 0 &&
                    Convert.ToDateTime(u.ReleaseDate) >= cutoff).ToArray();
                foreach (var game in items)
                {
                    var gameDb = new Games();
                    var usReleaseDate = new DateTime();

                    if (game.GameTitle != null)
                        gameDb.GameTitle = game.GameTitle;

                    if (game.ReleaseDate != null && game.ReleaseDate.IndexOf('/') > 0)
                        usReleaseDate = Convert.ToDateTime(game.ReleaseDate);

                    if (gameDb.GameTitle != null && usReleaseDate != null && usReleaseDate != DateTime.MinValue &&
                        ContainsEntries(gameDb.GameTitle) && !IgnoreThisGame(gameDb))
                    {
                        if (!IsDuplicateGame(gameDb))
                        {
                            _db.Games.AddObject(gameDb);
                            _db.SaveChanges();
                        }
                        else
                        {
                            gameDb = GetGameByGameTitle(gameDb.GameTitle);
                        }

                        var gameInfo = new GameInfo
                        {
                            GamesId = gameDb.Id,
                            GameSystemId = GetGameSystemId(gameSystem),
                            USReleaseDate = usReleaseDate,
                            GamesDbNetId = Convert.ToInt32(game.Id)
                        };

                        if (game.Thumb != null)
                            gameInfo.GameImage = "http://thegamesdb.net/banners/" + game.Thumb;

                        if (!IsDuplicateGameInfo(gameInfo))
                            if (ContainsEntries(gameDb.GameTitle, gameSystem) &&
                                !string.IsNullOrWhiteSpace(gameInfo.GameImage))
                            {
                                _db.GameInfoes.AddObject(gameInfo);
                                _db.SaveChanges();
                            }
                            else
                            {
                                DeleteGameFromGameInfoes(gameDb.Id);
                            }
                    }
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

                            var gameResponse = (DataByGameId) serializer.Deserialize(response.GetResponseStream());

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

        public void ProcessGamesFromWikipediaWebService(string url, string gameSystem)
        {
            var request = WebRequest.Create(url);
            using (var response = (HttpWebResponse) request.GetResponse())
            {
                var json = new StreamReader(response.GetResponseStream());

                // Convert the data I need to json
                var gameList = ConvertToList(json.ReadToEnd(), gameSystem);

                if (gameList != null)
                {
                    var title = string.Empty;
                    if (gameSystem == "Xbox 360" || gameSystem == "Wii U")
                        title = "Title";
                    else if (gameSystem == "PS3")
                        title = "Video Game";
                    foreach (var entry in gameList)
                    {
                        var game = new Games
                        {
                            GameTitle = entry[title].Trim()
                        };

                        if (ContainsEntries(game.GameTitle) && !IgnoreThisGame(game))
                        {
                            if (!IsDuplicateGame(game))
                            {
                                _db.Games.AddObject(game);
                                _db.SaveChanges();
                            }
                            else
                            {
                                game = GetGameByGameTitle(game.GameTitle);
                            }

                            var gameinfo = new GameInfo();

                            if (GetGameInfo(game.Id, gameSystem) != null)
                                gameinfo = GetGameInfo(game.Id, gameSystem);


                            gameinfo.GameSystemId = GetGameSystemId(gameSystem);
                            gameinfo.GamesId = game.Id;

                            if (entry.ContainsKey("Publisher"))
                                gameinfo.Publisher = entry["Publisher"];
                            if (entry.ContainsKey("Developer"))
                                gameinfo.Developer = entry["Developer"];
                            if (entry.ContainsKey("North America"))
                                gameinfo.USReleaseDate = ConvertJsonDate(entry["North America"], gameSystem);

                            // Get the boxart image from theGamesDB.net
                            if (gameinfo.GameImage == null || !gameinfo.GameImage.ToUpper().Contains("THEGAMESDB"))
                            {
                                try
                                {
                                    var dburl =
                                        $"http://thegamesdb.net/api/GetGame.php?name={game.GameTitle.Replace(' ', '+')}&platform={GetPlatform(gameSystem)}";
                                    var dbRequest = WebRequest.Create(dburl);
                                    using (var dbResponse = (HttpWebResponse) dbRequest.GetResponse())
                                    {
                                        var serializer = new XmlSerializer(typeof(Data));

                                        var data = (Data) serializer.Deserialize(dbResponse.GetResponseStream());

                                        if (data.Game.Count() >= 1)
                                            if (data.Game[0].GameTitle == game.GameTitle)
                                                foreach (var image in data.Game[0].Images)
                                                    if (image.Boxart.Count() > 0)
                                                    {
                                                        gameinfo.GameImage =
                                                            "http://thegamesdb.net/banners/" + image.Boxart[0].Thumb;
                                                        break;
                                                    }
                                    }
                                }
                                catch
                                {
                                    // Try getting image from Wikipedia
                                    try
                                    {
                                        var dbUrl =
                                            $"http://en.wikipedia.org/w/api.php?format=json&action=query&titles={game.GameTitle}&prop=images";
                                        var dbRequest = WebRequest.Create(dbUrl);
                                        using (var dbResponse = (HttpWebResponse) dbRequest.GetResponse())
                                        {
                                            var wkJson = new StreamReader(dbResponse.GetResponseStream());
                                            var text = wkJson.ReadToEnd();

                                            var first = text.IndexOf("File:");
                                            if (first > 0)
                                            {
                                                text = text.Remove(0, first);
                                                var files = text.Split(new[] {"File:"},
                                                    StringSplitOptions.RemoveEmptyEntries);
                                                var fileImage = string.Empty;
                                                if (files.Count() > 1)
                                                {
                                                    var imageFound = false;
                                                    foreach (var fileName in files)
                                                        if (fileName.ToUpper().Contains("BOX") ||
                                                            fileName.ToUpper().Contains("COVER"))
                                                        {
                                                            var second = fileName.IndexOf("\"", 0);
                                                            if (second > 0)
                                                            {
                                                                fileImage = "File:" + fileName.Substring(0, second);
                                                                imageFound = true;
                                                                break;
                                                            }
                                                        }

                                                    if (!imageFound)
                                                        foreach (var fileName in files)
                                                            if (fileName.ToUpper().Contains(game.GameTitle
                                                                    .Replace(" ", string.Empty).ToUpper()) ||
                                                                fileName.ToUpper().Contains(game.GameTitle.ToUpper()) ||
                                                                fileName.ToUpper()
                                                                    .Contains(
                                                                        game.GameTitle.Replace(" ", "_").ToUpper()))
                                                            {
                                                                var second = fileName.IndexOf("\"", 0);
                                                                if (second > 0)
                                                                {
                                                                    fileImage = "File:" + fileName.Substring(0, second);
                                                                    imageFound = true;
                                                                    break;
                                                                }
                                                            }

                                                    if (!imageFound)
                                                    {
                                                        var second = text.IndexOf("\"", 0);
                                                        if (second > 0)
                                                            fileImage = text.Substring(0, second);
                                                    }
                                                }
                                                else
                                                {
                                                    var second = text.IndexOf("\"", 0);
                                                    if (second > 0)
                                                        fileImage = text.Substring(0, second);
                                                }

                                                if (fileImage.Length > 0)
                                                {
                                                    var imageUrl =
                                                        $"http://en.wikipedia.org/w/api.php?action=query&titles={fileImage}&prop=imageinfo&iiprop=url&format=json";
                                                    var dbImageRequest = WebRequest.Create(imageUrl);
                                                    using (var dbImageResponse =
                                                        (HttpWebResponse) dbImageRequest.GetResponse())
                                                    {
                                                        var wkImageJson =
                                                            new StreamReader(dbImageResponse.GetResponseStream());
                                                        var imageLink = wkImageJson.ReadToEnd();
                                                        var urlToFind = "\"url\":\"";
                                                        first = imageLink.IndexOf(urlToFind);

                                                        if (first > 0)
                                                        {
                                                            var second = imageLink.IndexOf("\"",
                                                                first + urlToFind.Length);
                                                            if (second > first)
                                                                gameinfo.GameImage =
                                                                    imageLink.Substring(first + urlToFind.Length,
                                                                        second - (first + urlToFind.Length));
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    catch
                                    {
                                        gameinfo.GameImage = null;
                                    }
                                }

                                if (gameinfo.GameImage != null)
                                {
                                    gameinfo.GamesDbNetId = -1;
                                    if (GetGameInfo(game.Id, gameSystem) == null)
                                        _db.GameInfoes.AddObject(gameinfo);
                                }
                                else
                                {
                                    DeleteGameFromGameInfoes(game.Id);
                                }
                            }
                        }
                    }

                    _db.SaveChanges();
                }
            }
        }

        private List<Dictionary<string, string>> ConvertToList(string text, string gameSystem)
        {
            var input = text;

            var index = -1;

            if (gameSystem == "Xbox 360" || gameSystem == "Wii U")
                index = input.IndexOf("|Title", 0, StringComparison.OrdinalIgnoreCase);
            else if (gameSystem == "PS3")
                index = input.IndexOf("|Video Game", 0, StringComparison.OrdinalIgnoreCase);
            if (index > 0)
            {
                // Remove unneccessary stuff
                input = input.Remove(0, index);

                // Grab the section headers (Title, Developer, Publisher, etc)
                index = input.IndexOf("\'\'");
                if (index > 0)
                {
                    var header = input.Substring(0, index);
                    var headers = header.Remove(0, 1).Split('|');
                    for (var j = 0; j < headers.Count(); j++)
                    {
                        var subIndex = headers[j].IndexOf("\\n", StringComparison.OrdinalIgnoreCase);
                        if (subIndex > 0)
                        {
                            headers[j] = headers[j].Remove(subIndex, headers[j].Length - subIndex);
                            headers[j] = TrimEndString(headers[j], "(s)");
                        }
                    }

                    // Format the entries
                    input = input.Remove(0, index).Replace("\'\'", string.Empty).Replace("[[", string.Empty)
                        .Replace("]]", string.Empty).Replace("{{", string.Empty).Replace("}}", string.Empty);

                    // Put entries in their respective section
                    var games = input.Split(new[] {"|-\\n|"}, StringSplitOptions.RemoveEmptyEntries);
                    var gameList = new List<Dictionary<string, string>>();
                    foreach (var game in games)
                    {
                        var entry = game.Split(new[] {"\\n|"}, StringSplitOptions.RemoveEmptyEntries);
                        var gameEntry = new Dictionary<string, string>();
                        for (var subIndex = 0; subIndex < entry.Count(); subIndex++)
                        {
                            var refIndex = entry[subIndex].IndexOf("<ref>", 0, StringComparison.OrdinalIgnoreCase);
                            if (refIndex > 0)
                                entry[subIndex] = entry[subIndex].Remove(refIndex);
                            if (subIndex < headers.Count())
                                gameEntry[headers[subIndex]] = entry[subIndex];
                        }
                        gameList.Add(gameEntry);
                    }

                    var cutoff = DateTime.Now.AddMonths(-12);
                    var filteredList = gameList.Where(u => ConvertJsonDate(u["North America"], gameSystem) >= cutoff)
                        .ToList();

                    return filteredList;
                }
                return null;
            }
            return null;
        }

        public DateTime ConvertJsonDate(string date, string gameSystem)
        {
            if (date.ToLower().Contains("unreleased"))
                return DateTime.MinValue;
            var parts = date.Split('|');
            if (parts.Count() >= 4)
                try
                {
                    if (gameSystem == "Wii U")
                    {
                        int number;
                        if (!int.TryParse(parts[3], out number))
                            number = ConvertMonthTextToNumber(parts[3]);
                        return new DateTime(Convert.ToInt32(parts[2]), number, Convert.ToInt32(parts[4]));
                    }
                    else
                    {
                        int number;
                        if (!int.TryParse(parts[2], out number))
                            number = ConvertMonthTextToNumber(parts[2]);
                        return new DateTime(Convert.ToInt32(parts[1]), number, Convert.ToInt32(parts[3]));
                    }
                }
                catch
                {
                    return DateTime.MinValue;
                }
            return DateTime.MinValue;
        }

        private int ConvertMonthTextToNumber(string month)
        {
            month = month.ToUpper();

            switch (month)
            {
                case "JANUARY":
                    return 1;
                case "FEBRUARY":
                    return 2;
                case "MARCH":
                    return 3;
                case "APRIL":
                    return 4;
                case "MAY":
                    return 5;
                case "JUNE":
                    return 6;
                case "JULY":
                    return 7;
                case "AUGUST":
                    return 8;
                case "SEPTEMBER":
                    return 9;
                case "OCTOBER":
                    return 10;
                case "NOVEMBER":
                    return 11;
                default:
                    return 12;
            }
        }

        public string TrimEndString(string value, string toTrim)
        {
            if (value.EndsWith(toTrim))
            {
                var startIndex = toTrim.Length;
                return value.Substring(0, value.Length - startIndex);
            }
            return value;
        }

        private string[] GetUsReleaseDate(int index, string text)
        {
            var usReleaseDate = string.Empty;
            var dateIndex = text.IndexOf("{{", index);

            if (dateIndex > 0)
            {
                var endDateIndex = text.IndexOf("}}", dateIndex);
                usReleaseDate = text.Substring(dateIndex + 2, endDateIndex - (dateIndex + 2));
            }

            var splitDate = usReleaseDate.Split('|');

            if (splitDate.Length < 3 || splitDate[2].ToLower() == "unreleased")
                splitDate = null;
            else
                foreach (var part in splitDate)
                {
                    if (part.ToLower().Contains("dts"))
                        continue;
                    int number;
                    var result = int.TryParse(part, out number);

                    if (result == false)
                        splitDate = null;
                }

            return splitDate;
        }

        private string GetGameTitleFromWikipedia(int index, string text)
        {
            var title = string.Empty;

            var titleIndex = text.IndexOf("[[", index);

            if (titleIndex > 0)
            {
                var endTitleIndex = text.IndexOf("]]", titleIndex);
                title = text.Substring(titleIndex + 2, endTitleIndex - (titleIndex + 2));
            }

            return title;
        }

        private string[] ParseGameInfomatics(int index, string text)
        {
            var endIndex = text.IndexOf("{{", index);

            if (endIndex > 0)
                return text.Substring(index, endIndex - index).Replace("\'\'", string.Empty).Replace("[[", string.Empty)
                    .Replace("]]", string.Empty).Split('|');
            return null;
        }

        private string ParsePublisherFromWikipedia(int index, string text)
        {
            return string.Empty;
        }


        private IEnumerable<GameInfo> GetGameInfoBySystem(string gameSystem)
        {
            var gameSystemId = GetGameSystemId(gameSystem);
            return _db.GameInfoes.Where(u => u.GameSystemId == gameSystemId);
        }

        //private GameInfo GetGameInfo(int Id)
        //{
        //    return db.GameInfoes.SingleOrDefault(u => u.Id == Id);
        //}

        private bool IsDuplicateGameInfo(GameInfo gameInfo)
        {
            var temp = from tempDb in _db.GameInfoes
                where tempDb.GamesId == gameInfo.GamesId && tempDb.GameSystemId == gameInfo.GameSystemId
                select tempDb;

            return temp != null && temp.Count() > 0;
        }

        public bool IgnoreThisGame(Games game)
        {
            var temp = from tempDb in _db.GameIgnores
                where tempDb.GameTitle == game.GameTitle
                select tempDb;

            return temp != null && temp.Count() > 0;
        }

        public string GetGameSystemName(string name)
        {
            return name;
        }

        public IQueryable<Games> GetGamesQuery()
        {
            return _db.Games;
        }

        public bool IsDuplicateGame(Games game)
        {
            var isDuplicate = from tempGameItem in GetGamesQuery()
                where game.GameTitle == tempGameItem.GameTitle
                select tempGameItem;

            return isDuplicate != null && isDuplicate.Count() > 0;
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

                _db.Games.DeleteObject(game);
                _db.SaveChanges();
            }
        }

        private bool IsDuplicateIgnoredGame(Games game)
        {
            var isDuplate = from tempDb in _db.GameIgnores
                where game.GameTitle == tempDb.GameTitle
                select tempDb;
            return isDuplate != null && isDuplate.Count() > 0;
        }

        private void DeleteGameInfo(int id)
        {
            var gameInfos = _db.GameInfoes.Where(u => u.GamesId == id);
            foreach (var gameInfo in gameInfos)
                _db.GameInfoes.DeleteObject(gameInfo);
            _db.SaveChanges();
        }

        private IEnumerable<GameInfo> GetGameInfoByGameId(int id)
        {
            return _db.GameInfoes.Where(u => u.GamesId == id);
        }

        private GameInfo GetGameInfo(int gameId, string gameSystem)
        {
            var gameSystemId = GetGameSystemId(gameSystem);
            return _db.GameInfoes.SingleOrDefault(u => u.GamesId == gameId && u.GameSystemId == gameSystemId);
        }

        internal string GetImage(int id, string gameSystem)
        {
            try
            {
                var gameSystemId = GetGameSystemId(gameSystem);
                return _db.GameInfoes.SingleOrDefault(u => u.GamesId == id && u.GameSystemId == gameSystemId).GameImage;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal string GetPublisher(int id, string gameSystem)
        {
            try
            {
                var gameSystemId = GetGameSystemId(gameSystem);
                return _db.GameInfoes.SingleOrDefault(u => u.GamesId == id && u.GameSystemId == gameSystemId).Publisher;
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
                return _db.GameInfoes.SingleOrDefault(u => u.GamesId == id && u.GameSystemId == gameSystemId).Developer;
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
                return _db.GameInfoes.SingleOrDefault(u => u.GamesId == id && u.GameSystemId == gameSystemId).Overview;
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
                return _db.GameInfoes.SingleOrDefault(u => u.GamesId == id && u.GameSystemId == gameSystemId)
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
                return _db.GameInfoes.SingleOrDefault(u => u.GamesId == id && u.GameSystemId == gameSystemId)
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
            IEnumerable<GameSystem> gameSystemList = _db.GameSystems;
            foreach (var system in gameSystemList)
            {
                var temp = from tempDb in _db.GameInfoes
                    where tempDb.GameSystemId == system.Id && tempDb.GamesId == game.Id
                    select tempDb;

                if (temp != null && temp.Count() > 0 && ContainsEntries(game.GameTitle, system.GameSystemName))
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