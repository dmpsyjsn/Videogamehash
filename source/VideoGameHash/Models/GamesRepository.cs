using System;
using System.Globalization;
using System.Collections.Generic;
using System.Net;
using System.Xml.Serialization;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using VideoGameHash.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VideoGameHash.Models
{
    public class GamesRepository
    {
        private VGHDatabaseContainer db = new VGHDatabaseContainer();

        public IEnumerable<Games> GetGames()
        {
            return db.Games.OrderBy(u => u.GameTitle);
        }

        public IEnumerable<Games> GetSortedGames()
        {
            return db.Games.Where(u => u.GameInfoes.Count > 0).OrderBy(u => u.GameTitle);
        }

        public IEnumerable<Games> GetSortedGamesByLetter(char letter)
        {
            //List<Games> filteredGames = db.Games.Where(u => u.GameInfoes.Count > 0).OrderBy(u => u.GameTitle).ToList();
            List<Games> returnedList = new List<Games>();
            if (letter == '0')
            {
                foreach (Games game in db.Games.Where(u => u.GameInfoes.Count > 0).OrderBy(u => u.GameTitle))
                {
                    if (game.GameTitle[0] >= '0' && game.GameTitle[0] <= '9')
                        returnedList.Add(game);
                }
            }
            else
            {
                foreach (Games game in db.Games.Where(u => u.GameInfoes.Count > 0).OrderBy(u => u.GameTitle))
                {
                    if (game.GameTitle[0] == letter)
                        returnedList.Add(game);
                }
            }

            return returnedList.AsEnumerable();
        }

        public IEnumerable<Games> SearchGames(string search)
        {
            return db.Games.Where(u => u.GameInfoes.Count > 0 && u.GameTitle.Contains(search)).Take(20).OrderBy(u => u.GameTitle);
        }

        public void AddGame(string GameSystem)
        {
            try
            {
                if (GameSystem != "All")
                {
                    string url = string.Format("http://thegamesdb.net/api/PlatformGames.php?platform={0}", GetPlatform(GameSystem));

                    ProcessGamesFromWebService(url, GameSystem);

                    ProcessAdditionalDetailsFromWebService(GameSystem);
                }
            }
            catch
            {
                // Do nothing
            }
        }

        public void AddGameWikipedia(string GameSystem)
        {
            try
            {
                if (GameSystem != "All")
                {
                    string url = string.Format("http://en.wikipedia.org/w/api.php?format=json&action=query&titles={0}&prop=revisions&rvprop=content&rvsection=1", GetWikipediaPlatform(GameSystem));

                    ProcessGamesFromWikipediaWebService(url, GameSystem);

                    //ProcessAdditionalDetailsFromWikipediaWebService(GameSystem);
                }
            }
            catch
            {
                // Do nothing
            }
        }

        public Games GetGame(int Id)
        {
            return db.Games.SingleOrDefault(u => u.Id == Id);
        }

        public Games GetGameByGameTitle(string gameTitle)
        {
            return db.Games.SingleOrDefault(u => u.GameTitle == gameTitle);
        }

        public int GetGameSystemId(string gameSystem)
        {
            return db.GameSystems.SingleOrDefault(u => u.GameSystemName == gameSystem).Id;
        }

        public bool ContainsEntries(string gameTitle)
        {
            InfoRepository ir = new InfoRepository();

            return ir.ContainsArticles(gameTitle);
        }

        public bool ContainsEntries(string gameTitle, string gameSystem)
        {
            InfoRepository ir = new InfoRepository();
            return ir.ContainsArticles(gameTitle, gameSystem);
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
                gameSystem = "List of PlayStation 4 games";

            return gameSystem;
        }

        public void ProcessGamesFromWebService(string url, string gameSystem)
        {
            WebRequest request = WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                XmlSerializer serializer = new XmlSerializer(typeof(DataByPlatform));

                DataByPlatform gameResponse = (DataByPlatform)serializer.Deserialize(response.GetResponseStream());
                DateTime cutoff = DateTime.Now.AddMonths(-3);
                DataGameByPlatform[] Items = gameResponse.Items.Where(u => u.ReleaseDate != null && u.ReleaseDate.IndexOf('/') > 0 && Convert.ToDateTime(u.ReleaseDate) >= cutoff).ToArray();
                foreach (DataGameByPlatform game in Items)
                {
                    Games GameDb = new Games();
                    DateTime USReleaseDate = new DateTime();

                    if (game.GameTitle != null)
                        GameDb.GameTitle = game.GameTitle;

                    if (game.ReleaseDate != null && game.ReleaseDate.IndexOf('/') > 0)
                        USReleaseDate = Convert.ToDateTime(game.ReleaseDate);

                    if (GameDb.GameTitle != null && USReleaseDate != null && USReleaseDate != DateTime.MinValue &&
                        ContainsEntries(GameDb.GameTitle) && !IgnoreThisGame(GameDb))
                    {
                        if (!IsDuplicateGame(GameDb))
                        {
                            db.Games.AddObject(GameDb);
                            db.SaveChanges();
                        }
                        else
                        {
                            GameDb = GetGameByGameTitle(GameDb.GameTitle);
                        }

                        GameInfo gameInfo = new GameInfo();
                        gameInfo.GamesId = GameDb.Id;
                        gameInfo.GameSystemId = GetGameSystemId(gameSystem);
                        gameInfo.USReleaseDate = USReleaseDate;
                        gameInfo.GamesDbNetId = Convert.ToInt32(game.id);

                        if (game.thumb != null)
                        {
                            // Add box art images
                            gameInfo.GameImage = "http://thegamesdb.net/banners/" + game.thumb;
                        }

                        if (!IsDuplicateGameInfo(gameInfo))
                        {
                            if (ContainsEntries(GameDb.GameTitle, gameSystem) && !String.IsNullOrWhiteSpace(gameInfo.GameImage))
                            {
                                db.GameInfoes.AddObject(gameInfo);
                                db.SaveChanges();
                            }
                            else
                            {
                                DeleteGameFromGameInfoes(GameDb.Id);
                            }
                        }
                    }
                }
            }
        }

        private void DeleteGameFromGameInfoes(int GameId)
        {
            IEnumerable<GameInfo> infos = db.GameInfoes.Where(u => u.GamesId == GameId);

            foreach (GameInfo info in infos)
                db.GameInfoes.DeleteObject(info);

            db.SaveChanges();

            Games game = GetGame(GameId);

            if (game != null)
                db.Games.DeleteObject(game);
        }

        private bool ProcessAdditionalDetailsFromWebService(string GameSystem)
        {
            bool success = true;
            IEnumerable<GameInfo> GameInfos = GetGameInfoBySystem(GameSystem);
            List<int> DeleteTheseGames = new List<int>();
            foreach (GameInfo gameInfo in GameInfos)
            {
                if (String.IsNullOrWhiteSpace(gameInfo.Publisher)) // TO DO: Add an update boolean here
                {
                    try
                    {
                        string url = String.Format("http://thegamesdb.net/api/GetGame.php?id={0}&platform={1}", gameInfo.GamesDbNetId, GetPlatform(GameSystem));
                        WebRequest request = WebRequest.Create(url);
                        using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                        {
                            XmlSerializer serializer = new XmlSerializer(typeof(DataByGameId));

                            DataByGameId gameResponse = (DataByGameId)serializer.Deserialize(response.GetResponseStream());

                            if (!String.IsNullOrWhiteSpace(gameResponse.Game[0].Publisher) || !String.IsNullOrWhiteSpace(gameResponse.Game[0].Developer))
                            {
                                gameInfo.Publisher = gameResponse.Game[0].Publisher;
                                gameInfo.Developer = gameResponse.Game[0].Developer;
                                gameInfo.Overview = gameResponse.Game[0].Overview;
                            }
                            else
                            {
                                DeleteTheseGames.Add(gameInfo.Game.Id);
                            }
                        }
                    }
                    catch
                    {
                        success = false;
                        break;
                    }
                }
            }

            db.SaveChanges();

            foreach (int game in DeleteTheseGames)
            {
                DeleteGameFromGameInfoes(game);
            }

            return success;
        }

        public void ProcessGamesFromWikipediaWebService(string url, string gameSystem)
        {
            WebRequest request = WebRequest.Create(url);
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                StreamReader json = new StreamReader(response.GetResponseStream());

                // Convert the data I need to json
                List<Dictionary<string, string>> gameList = ConvertToList(json.ReadToEnd(), gameSystem);

                if (gameList != null)
                {
                    string title = String.Empty;
                    if (gameSystem == "Xbox 360" || gameSystem == "Wii U")
                        title = "Title";
                    else if (gameSystem == "PS3")
                        title = "Video Game";
                    foreach (Dictionary<string, string> entry in gameList)
                    {
                        Games game = new Games();

                        game.GameTitle = entry[title].Trim();

                        if (ContainsEntries(game.GameTitle) && !IgnoreThisGame(game))
                        {
                            if (!IsDuplicateGame(game))
                            {
                                db.Games.AddObject(game);
                                db.SaveChanges();
                            }
                            else
                            {
                                game = GetGameByGameTitle(game.GameTitle);
                            }

                            GameInfo gameinfo = new GameInfo();

                            if (GetGameInfo(game.Id, gameSystem) != null)
                            {
                                gameinfo = GetGameInfo(game.Id, gameSystem);
                            }


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
                                    string dburl = String.Format("http://thegamesdb.net/api/GetGame.php?name={0}&platform={1}", game.GameTitle.Replace(' ', '+'), GetPlatform(gameSystem));
                                    WebRequest dbRequest = WebRequest.Create(dburl);
                                    using (HttpWebResponse dbResponse = (HttpWebResponse)dbRequest.GetResponse())
                                    {
                                        XmlSerializer serializer = new XmlSerializer(typeof(Data));

                                        Data data = (Data)serializer.Deserialize(dbResponse.GetResponseStream());

                                        if (data.Game.Count() >= 1)
                                        {
                                            if (data.Game[0].GameTitle == game.GameTitle)
                                            {
                                                foreach (DataGameImages image in data.Game[0].Images)
                                                {
                                                    if (image.boxart.Count() > 0)
                                                    {
                                                        gameinfo.GameImage = "http://thegamesdb.net/banners/" + image.boxart[0].thumb;
                                                        break;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                catch
                                {
                                    // Try getting image from Wikipedia
                                    try
                                    {
                                        string dbUrl = string.Format("http://en.wikipedia.org/w/api.php?format=json&action=query&titles={0}&prop=images", game.GameTitle);
                                        WebRequest dbRequest = WebRequest.Create(dbUrl);
                                        using (HttpWebResponse dbResponse = (HttpWebResponse)dbRequest.GetResponse())
                                        {
                                            StreamReader wkJson = new StreamReader(dbResponse.GetResponseStream());
                                            string text = wkJson.ReadToEnd();

                                            int first = text.IndexOf("File:");
                                            if (first > 0)
                                            {
                                                text = text.Remove(0, first);
                                                string[] files = text.Split(new string[] { "File:" }, StringSplitOptions.RemoveEmptyEntries);
                                                string fileImage = string.Empty;
                                                if (files.Count() > 1)
                                                {
                                                    bool imageFound = false;
                                                    foreach (string fileName in files)
                                                    {
                                                        if (fileName.ToUpper().Contains("BOX") || fileName.ToUpper().Contains("COVER"))
                                                        {
                                                            int second = fileName.IndexOf("\"", 0);
                                                            if (second > 0)
                                                            {
                                                                fileImage = "File:" + fileName.Substring(0, second);
                                                                imageFound = true;
                                                                break;
                                                            }
                                                        }
                                                    }

                                                    if (!imageFound)
                                                    {
                                                        foreach (string fileName in files)
                                                        {
                                                            if (fileName.ToUpper().Contains(game.GameTitle.Replace(" ", String.Empty).ToUpper()) ||
                                                                fileName.ToUpper().Contains(game.GameTitle.ToUpper()) ||
                                                                fileName.ToUpper().Contains(game.GameTitle.Replace(" ", "_").ToUpper()))
                                                            {
                                                                int second = fileName.IndexOf("\"", 0);
                                                                if (second > 0)
                                                                {
                                                                    fileImage = "File:" + fileName.Substring(0, second);
                                                                    imageFound = true;
                                                                    break;
                                                                }
                                                            }
                                                        }
                                                    }

                                                    if (!imageFound)
                                                    {
                                                        int second = text.IndexOf("\"", 0);
                                                        if (second > 0)
                                                        {
                                                            fileImage = text.Substring(0, second);
                                                        }
                                                    }

                                                }
                                                else
                                                {
                                                    int second = text.IndexOf("\"", 0);
                                                    if (second > 0)
                                                    {
                                                        fileImage = text.Substring(0, second);
                                                    }
                                                }

                                                if (fileImage.Length > 0)
                                                {
                                                    string imageUrl = string.Format("http://en.wikipedia.org/w/api.php?action=query&titles={0}&prop=imageinfo&iiprop=url&format=json", fileImage);
                                                    WebRequest dbImageRequest = WebRequest.Create(imageUrl);
                                                    using (HttpWebResponse dbImageResponse = (HttpWebResponse)dbImageRequest.GetResponse())
                                                    {
                                                        StreamReader wkImageJson = new StreamReader(dbImageResponse.GetResponseStream());
                                                        string imageLink = wkImageJson.ReadToEnd();
                                                        string urlToFind = "\"url\":\"";
                                                        first = imageLink.IndexOf(urlToFind);

                                                        if (first > 0)
                                                        {
                                                            int second = imageLink.IndexOf("\"", first + urlToFind.Length);
                                                            if (second > first)
                                                                gameinfo.GameImage = imageLink.Substring(first + urlToFind.Length, second - (first + urlToFind.Length));
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
                                        db.GameInfoes.AddObject(gameinfo);
                                }
                                else
                                {
                                    DeleteGameFromGameInfoes(game.Id);
                                }
                            }
                        }
                    }

                    db.SaveChanges();
                }
            }
        }

        private List<Dictionary<string, string>> ConvertToList(string text, string gameSystem)
        {
            string input = text;

            int index = -1;

            if (gameSystem == "Xbox 360" || gameSystem == "Wii U")
                index = input.IndexOf("|Title", 0);
            else if (gameSystem == "PS3")
                index = input.IndexOf("|Video Game", 0);
            if (index > 0)
            {
                // Remove unneccessary stuff
                input = input.Remove(0, index);

                // Grab the section headers (Title, Developer, Publisher, etc)
                index = input.IndexOf("\'\'");
                if (index > 0)
                {
                    string header = input.Substring(0, index);
                    string[] headers = header.Remove(0, 1).Split('|');
                    for (int j = 0; j < headers.Count(); j++)
                    {
                        int subIndex = headers[j].IndexOf("\\n");
                        if (subIndex > 0)
                        {
                            headers[j] = headers[j].Remove(subIndex, headers[j].Length - subIndex);
                            headers[j] = TrimEndString(headers[j], "(s)");
                        }
                    }

                    // Format the entries
                    input = input.Remove(0, index).Replace("\'\'", String.Empty).Replace("[[", String.Empty).Replace("]]", String.Empty).Replace("{{", String.Empty).Replace("}}", String.Empty);

                    // Put entries in their respective section
                    string[] games = input.Split(new string[] { "|-\\n|" }, StringSplitOptions.RemoveEmptyEntries);
                    List<Dictionary<string, string>> gameList = new List<Dictionary<string, string>>();
                    foreach (string game in games)
                    {
                        string[] entry = game.Split(new string[] { "\\n|" }, StringSplitOptions.RemoveEmptyEntries);
                        Dictionary<string, string> gameEntry = new Dictionary<string, string>();
                        for (int subIndex = 0; subIndex < entry.Count(); subIndex++)
                        {
                            int refIndex = entry[subIndex].IndexOf("<ref>", 0);
                            if (refIndex > 0)
                            {
                                entry[subIndex] = entry[subIndex].Remove(refIndex);
                            }
                            if (subIndex < headers.Count())
                                gameEntry[headers[subIndex]] = entry[subIndex];
                        }
                        gameList.Add(gameEntry);
                    }

                    DateTime cutoff = DateTime.Now.AddMonths(-12);
                    List<Dictionary<string, string>> filteredList = gameList.Where(u => ConvertJsonDate(u["North America"], gameSystem) >= cutoff).ToList();

                    return filteredList;
                }
                else
                    return null;
            }
            else
                return null;
        }

        public DateTime ConvertJsonDate(string date, string gameSystem)
        {
            if (date.ToLower().Contains("unreleased"))
                return DateTime.MinValue;
            else
            {
                string[] parts = date.Split('|');
                if (parts.Count() >= 4)
                {
                    try
                    {
                        if (gameSystem == "Wii U")
                        {
                            int number;
                            if (!Int32.TryParse(parts[3], out number))
                            {
                                number = ConvertMonthTextToNumber(parts[3]);
                            }
                            return new DateTime(Convert.ToInt32(parts[2]), number, Convert.ToInt32(parts[4]));
                        }
                        else
                        {
                            int number;
                            if (!Int32.TryParse(parts[2], out number))
                            {
                                number = ConvertMonthTextToNumber(parts[2]);
                            }
                            return new DateTime(Convert.ToInt32(parts[1]), number, Convert.ToInt32(parts[3]));
                        }
                    }
                    catch
                    {
                        return DateTime.MinValue;
                    }
                }
                else
                    return DateTime.MinValue;
            }
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
                int startIndex = toTrim.Length;
                return value.Substring(0, value.Length - startIndex);
            }
            return value;
        }

        private string[] GetUSReleaseDate(int index, string text)
        {
            string usReleaseDate = String.Empty;
            int dateIndex = text.IndexOf("{{", index);

            if (dateIndex > 0)
            {
                int endDateIndex = text.IndexOf("}}", dateIndex);
                usReleaseDate = text.Substring(dateIndex + 2, endDateIndex - (dateIndex + 2));
            }

            string[] splitDate = usReleaseDate.Split('|');

            if (splitDate.Length < 3 || splitDate[2].ToLower() == "unreleased")
                splitDate = null;
            else
            {
                foreach (string part in splitDate)
                {
                    if (part.ToLower().Contains("dts"))
                        continue;
                    int number;
                    bool result = Int32.TryParse(part, out number);

                    if (result == false)
                        splitDate = null;
                }
            }

            return splitDate;
        }

        private string GetGameTitleFromWikipedia(int index, string text)
        {
            string title = String.Empty;

            int titleIndex = text.IndexOf("[[", index);

            if (titleIndex > 0)
            {
                int endTitleIndex = text.IndexOf("]]", titleIndex);
                title = text.Substring(titleIndex + 2, endTitleIndex - (titleIndex + 2));
            }

            return title;
        }

        private string[] ParseGameInfomatics(int index, string text)
        {
            int endIndex = text.IndexOf("{{", index);

            if (endIndex > 0)
                return text.Substring(index, endIndex - index).Replace("\'\'", String.Empty).Replace("[[", String.Empty).Replace("]]", String.Empty).Split('|');
            else
                return null;
        }

        private string ParsePublisherFromWikipedia(int index, string text)
        {
            return String.Empty;
        }


        private IEnumerable<GameInfo> GetGameInfoBySystem(string GameSystem)
        {
            int gameSystemId = GetGameSystemId(GameSystem);
            return db.GameInfoes.Where(u => u.GameSystemId == gameSystemId);
        }

        //private GameInfo GetGameInfo(int Id)
        //{
        //    return db.GameInfoes.SingleOrDefault(u => u.Id == Id);
        //}

        private bool IsDuplicateGameInfo(GameInfo gameInfo)
        {
            var temp = from tempDb in db.GameInfoes
                       where tempDb.GamesId == gameInfo.GamesId && tempDb.GameSystemId == gameInfo.GameSystemId
                       select tempDb;

            return temp != null && temp.Count() > 0;
        }

        public bool IgnoreThisGame(Games game)
        {
            var temp = from tempDb in db.GameIgnores
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
            return db.Games;
        }

        public bool IsDuplicateGame(Games game)
        {
            var isDuplicate = from tempGameItem in GetGamesQuery()
                              where game.GameTitle == tempGameItem.GameTitle
                              select tempGameItem;

            return isDuplicate != null && isDuplicate.Count() > 0;
        }

        public void DeleteGame(int Id)
        {
            Games game = GetGame(Id);

            if (game != null)
            {
                GameIgnore gameIgnore = new GameIgnore();
                gameIgnore.GameTitle = game.GameTitle;

                if (!IsDuplicateIgnoredGame(game))
                    db.GameIgnores.AddObject(gameIgnore);

                // Delete from GameInfo first
                DeleteGameInfo(game.Id);

                db.Games.DeleteObject(game);
                db.SaveChanges();
            }
        }

        private bool IsDuplicateIgnoredGame(Games game)
        {
            var isDuplate = from tempDb in db.GameIgnores
                            where game.GameTitle == tempDb.GameTitle
                            select tempDb;
            return isDuplate != null && isDuplate.Count() > 0;
        }

        private void DeleteGameInfo(int Id)
        {
            IQueryable<GameInfo> gameInfos = db.GameInfoes.Where(u => u.GamesId == Id);
            foreach (GameInfo gameInfo in gameInfos)
            {
                db.GameInfoes.DeleteObject(gameInfo);
            }
            db.SaveChanges();
        }

        private IEnumerable<GameInfo> GetGameInfoByGameId(int Id)
        {
            return db.GameInfoes.Where(u => u.GamesId == Id);
        }

        private GameInfo GetGameInfo(int gameId, string gameSystem)
        {
            int gameSystemId = GetGameSystemId(gameSystem);
            return db.GameInfoes.SingleOrDefault(u => u.GamesId == gameId && u.GameSystemId == gameSystemId);
        }

        internal string GetImage(int Id, string GameSystem)
        {
            try
            {
                int gameSystemId = GetGameSystemId(GameSystem);
                return db.GameInfoes.SingleOrDefault(u => u.GamesId == Id && u.GameSystemId == gameSystemId).GameImage;
            }
            catch
            {
                return String.Empty;
            }
        }

        internal string GetPublisher(int Id, string GameSystem)
        {
            try
            {
                int gameSystemId = GetGameSystemId(GameSystem);
                return db.GameInfoes.SingleOrDefault(u => u.GamesId == Id && u.GameSystemId == gameSystemId).Publisher;
            }
            catch
            {
                return String.Empty;
            }
        }

        internal string GetDeveloper(int Id, string GameSystem)
        {
            try
            {
                int gameSystemId = GetGameSystemId(GameSystem);
                return db.GameInfoes.SingleOrDefault(u => u.GamesId == Id && u.GameSystemId == gameSystemId).Developer;
            }
            catch
            {
                return String.Empty;
            }
        }

        internal string GetOverview(int Id, string GameSystem)
        {
            try
            {
                int gameSystemId = GetGameSystemId(GameSystem);
                return db.GameInfoes.SingleOrDefault(u => u.GamesId == Id && u.GameSystemId == gameSystemId).Overview;
            }
            catch
            {
                return String.Empty;
            }
        }

        internal int GetGamesDBNetId(int Id, string GameSystem)
        {
            try
            {
                int gameSystemId = GetGameSystemId(GameSystem);
                return db.GameInfoes.SingleOrDefault(u => u.GamesId == Id && u.GameSystemId == gameSystemId).GamesDbNetId;
            }
            catch
            {
                return -1;
            }
        }

        internal DateTime GetReleaseDate(int Id, string GameSystem)
        {
            try
            {
                int gameSystemId = GetGameSystemId(GameSystem);
                return db.GameInfoes.SingleOrDefault(u => u.GamesId == Id && u.GameSystemId == gameSystemId).USReleaseDate;
            }
            catch
            {
                return DateTime.MinValue;
            }
        }

        internal List<string> GetGameSystemsForThisGame(Games game)
        {
            List<string> gameSystem = new List<string>();
            IEnumerable<GameSystem> gameSystemList = db.GameSystems;
            foreach (GameSystem system in gameSystemList)
            {
                var temp = from tempDb in db.GameInfoes
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


    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "Data")]
    public partial class DataByPlatform
    {

        [XmlElement("DataGame")]
        private DataGameByPlatform[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Game", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataGameByPlatform[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameByPlatform
    {

        private string idField;

        private string gameTitleField;

        private string releaseDateField;

        private string thumbField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string GameTitle
        {
            get
            {
                return this.gameTitleField;
            }
            set
            {
                this.gameTitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ReleaseDate
        {
            get
            {
                return this.releaseDateField;
            }
            set
            {
                this.releaseDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string thumb
        {
            get
            {
                return this.thumbField;
            }
            set
            {
                this.thumbField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = true)]
    public partial class original
    {

        private string widthField;

        private string heightField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false, ElementName = "Data")]
    public partial class DataByGameId
    {

        private string baseImgUrlField;

        [XmlElement("DataGame")]
        private DataGameByGameId[] gameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string baseImgUrl
        {
            get
            {
                return this.baseImgUrlField;
            }
            set
            {
                this.baseImgUrlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Game", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataGameByGameId[] Game
        {
            get
            {
                return this.gameField;
            }
            set
            {
                this.gameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameByGameId
    {

        private string idField;

        private string gameTitleField;

        private string platformIdField;

        private string platformField;

        private string releaseDateField;

        private string overviewField;

        private string eSRBField;

        private string playersField;

        private string coopField;

        private string youtubeField;

        private string publisherField;

        private string developerField;

        private string ratingField;

        private DataGameGenres[] genresField;

        private DataGameImages[] imagesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string GameTitle
        {
            get
            {
                return this.gameTitleField;
            }
            set
            {
                this.gameTitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PlatformId
        {
            get
            {
                return this.platformIdField;
            }
            set
            {
                this.platformIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Platform
        {
            get
            {
                return this.platformField;
            }
            set
            {
                this.platformField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ReleaseDate
        {
            get
            {
                return this.releaseDateField;
            }
            set
            {
                this.releaseDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Overview
        {
            get
            {
                return this.overviewField;
            }
            set
            {
                this.overviewField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ESRB
        {
            get
            {
                return this.eSRBField;
            }
            set
            {
                this.eSRBField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Players
        {
            get
            {
                return this.playersField;
            }
            set
            {
                this.playersField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Co-op", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Coop
        {
            get
            {
                return this.coopField;
            }
            set
            {
                this.coopField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Youtube
        {
            get
            {
                return this.youtubeField;
            }
            set
            {
                this.youtubeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Publisher
        {
            get
            {
                return this.publisherField;
            }
            set
            {
                this.publisherField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Developer
        {
            get
            {
                return this.developerField;
            }
            set
            {
                this.developerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Rating
        {
            get
            {
                return this.ratingField;
            }
            set
            {
                this.ratingField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Genres", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataGameGenres[] Genres
        {
            get
            {
                return this.genresField;
            }
            set
            {
                this.genresField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Images", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataGameImages[] Images
        {
            get
            {
                return this.imagesField;
            }
            set
            {
                this.imagesField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameGenres
    {

        private string genreField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string genre
        {
            get
            {
                return this.genreField;
            }
            set
            {
                this.genreField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameImages
    {

        private DataGameImagesFanart[] fanartField;

        private DataGameImagesBoxart[] boxartField;

        private DataGameImagesBanner[] bannerField;

        private DataGameImagesScreenshot[] screenshotField;

        private DataGameImagesClearlogo[] clearlogoField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("fanart", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataGameImagesFanart[] fanart
        {
            get
            {
                return this.fanartField;
            }
            set
            {
                this.fanartField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("boxart", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public DataGameImagesBoxart[] boxart
        {
            get
            {
                return this.boxartField;
            }
            set
            {
                this.boxartField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("banner", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public DataGameImagesBanner[] banner
        {
            get
            {
                return this.bannerField;
            }
            set
            {
                this.bannerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("screenshot", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataGameImagesScreenshot[] screenshot
        {
            get
            {
                return this.screenshotField;
            }
            set
            {
                this.screenshotField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("clearlogo", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = true)]
        public DataGameImagesClearlogo[] clearlogo
        {
            get
            {
                return this.clearlogoField;
            }
            set
            {
                this.clearlogoField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameImagesFanart
    {

        private string thumbField;

        private original[] originalField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string thumb
        {
            get
            {
                return this.thumbField;
            }
            set
            {
                this.thumbField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("original", IsNullable = true)]
        public original[] original
        {
            get
            {
                return this.originalField;
            }
            set
            {
                this.originalField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameImagesBoxart
    {

        private string sideField;

        private string widthField;

        private string heightField;

        private string thumbField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string side
        {
            get
            {
                return this.sideField;
            }
            set
            {
                this.sideField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string thumb
        {
            get
            {
                return this.thumbField;
            }
            set
            {
                this.thumbField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameImagesBanner
    {

        private string widthField;

        private string heightField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameImagesScreenshot
    {

        private string thumbField;

        private original[] originalField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string thumb
        {
            get
            {
                return this.thumbField;
            }
            set
            {
                this.thumbField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("original", IsNullable = true)]
        public original[] original
        {
            get
            {
                return this.originalField;
            }
            set
            {
                this.originalField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameImagesClearlogo
    {

        private string widthField;

        private string heightField;

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string width
        {
            get
            {
                return this.widthField;
            }
            set
            {
                this.widthField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string height
        {
            get
            {
                return this.heightField;
            }
            set
            {
                this.heightField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class NewDataSet
    {

        private object[] itemsField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Data", typeof(DataByGameId))]
        [System.Xml.Serialization.XmlElementAttribute("original", typeof(original), IsNullable = true)]
        public object[] Items
        {
            get
            {
                return this.itemsField;
            }
            set
            {
                this.itemsField = value;
            }
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



    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace = "", IsNullable = false)]
    public partial class Data
    {

        private string baseImgUrlField;

        private DataGame[] gameField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string baseImgUrl
        {
            get
            {
                return this.baseImgUrlField;
            }
            set
            {
                this.baseImgUrlField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Game", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataGame[] Game
        {
            get
            {
                return this.gameField;
            }
            set
            {
                this.gameField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGame
    {

        private string idField;

        private string gameTitleField;

        private string platformIdField;

        private string platformField;

        private string releaseDateField;

        private string overviewField;

        private string eSRBField;

        private string playersField;

        private string coopField;

        private string youtubeField;

        private string publisherField;

        private string developerField;

        private string ratingField;

        private DataGameAlternateTitlesTitle[] alternateTitlesField;

        private DataGameGenresGenre[] genresField;

        private DataGameImages[] imagesField;

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string id
        {
            get
            {
                return this.idField;
            }
            set
            {
                this.idField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string GameTitle
        {
            get
            {
                return this.gameTitleField;
            }
            set
            {
                this.gameTitleField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string PlatformId
        {
            get
            {
                return this.platformIdField;
            }
            set
            {
                this.platformIdField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Platform
        {
            get
            {
                return this.platformField;
            }
            set
            {
                this.platformField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ReleaseDate
        {
            get
            {
                return this.releaseDateField;
            }
            set
            {
                this.releaseDateField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Overview
        {
            get
            {
                return this.overviewField;
            }
            set
            {
                this.overviewField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string ESRB
        {
            get
            {
                return this.eSRBField;
            }
            set
            {
                this.eSRBField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Players
        {
            get
            {
                return this.playersField;
            }
            set
            {
                this.playersField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Co-op", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Coop
        {
            get
            {
                return this.coopField;
            }
            set
            {
                this.coopField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Youtube
        {
            get
            {
                return this.youtubeField;
            }
            set
            {
                this.youtubeField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Publisher
        {
            get
            {
                return this.publisherField;
            }
            set
            {
                this.publisherField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Developer
        {
            get
            {
                return this.developerField;
            }
            set
            {
                this.developerField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public string Rating
        {
            get
            {
                return this.ratingField;
            }
            set
            {
                this.ratingField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("title", typeof(DataGameAlternateTitlesTitle), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataGameAlternateTitlesTitle[] AlternateTitles
        {
            get
            {
                return this.alternateTitlesField;
            }
            set
            {
                this.alternateTitlesField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlArrayAttribute(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [System.Xml.Serialization.XmlArrayItemAttribute("genre", typeof(DataGameGenresGenre), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataGameGenresGenre[] Genres
        {
            get
            {
                return this.genresField;
            }
            set
            {
                this.genresField = value;
            }
        }

        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Images", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public DataGameImages[] Images
        {
            get
            {
                return this.imagesField;
            }
            set
            {
                this.imagesField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameAlternateTitlesTitle
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }

    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.3038")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType = true)]
    public partial class DataGameGenresGenre
    {

        private string valueField;

        /// <remarks/>
        [System.Xml.Serialization.XmlTextAttribute()]
        public string Value
        {
            get
            {
                return this.valueField;
            }
            set
            {
                this.valueField = value;
            }
        }
    }


}