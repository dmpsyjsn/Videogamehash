using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Routing;
using Newtonsoft.Json;
using VideoGameHash.Models.TheGamesDB;

namespace VideoGameHash.Helpers
{
    public static class TheGamesDBHelper
    {
        public static async Task<List<Game>> GetGamesBySystem(string gameSystem)
        {
            var platformId = GamesHelper.GetGamesDbPlatformId(gameSystem);

            var games = new List<Game>();
            var index = 1;
            while (true)
            {
                if (index > 5) break;

                var url = $"{ConfigurationManager.AppSettings["TheGamesDBApiUrl"]}Games/ByPlatformID?id={platformId}&fields=publishers&apikey={ConfigurationManager.AppSettings["TheGamesDBApiKey"]}&page={index++}";

                var request = WebRequest.Create(url);
                using (var response = (HttpWebResponse) await request.GetResponseAsync())
                {
                    var responseStream = response.GetResponseStream();
                    if (responseStream == null) throw new InvalidOperationException("Bad response!");

                    string responseString;

                    using (var sr = new StreamReader(responseStream))
                    {
                        responseString = sr.ReadToEnd();
                    }

                    if (string.IsNullOrEmpty(responseString)) throw new InvalidOperationException("Bad response!");

                    var gameResponse = JsonConvert.DeserializeObject<RootObject>(responseString);

                    var cutoff = DateTime.Now.AddMonths(-12);
                    games.AddRange(gameResponse.data.games.Where(u => !string.IsNullOrEmpty(u.release_date) && u.release_date.IndexOf('-') > 0 && Convert.ToDateTime(u.release_date) >= cutoff).ToList());
                }
            }

            return games;
        }

        public static async Task<List<IdNameMapping>> GetDevelopers()
        {
            var url = $"{ConfigurationManager.AppSettings["TheGamesDBApiUrl"]}/Developers?apikey={ConfigurationManager.AppSettings["TheGamesDBApiKey"]}";
            var request = WebRequest.Create(url);
            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream == null) throw new InvalidOperationException("Bad response!");

                string responseString;

                using (var sr = new StreamReader(responseStream))
                {
                    responseString = sr.ReadToEnd();
                }

                if (string.IsNullOrEmpty(responseString)) throw new InvalidOperationException("Bad response!");

                var gameResponse = JsonConvert.DeserializeObject<dynamic>(responseString);

                var developers = new List<IdNameMapping>();
                
                if (gameResponse.status != "Success") return developers;
                
                var result = new RouteValueDictionary(gameResponse.data.developers);
                var developerJson = JsonConvert.SerializeObject(result.Values);
                developers.AddRange(JsonConvert.DeserializeObject<List<IdNameMapping>>(developerJson));

                return developers;
            }
        }

        public static async Task<List<IdNameMapping>> GetPublishers()
        {
            var url = $"{ConfigurationManager.AppSettings["TheGamesDBApiUrl"]}/Publishers?apikey={ConfigurationManager.AppSettings["TheGamesDBApiKey"]}";
            var request = WebRequest.Create(url);
            using (var response = (HttpWebResponse) await request.GetResponseAsync())
            {
                var responseStream = response.GetResponseStream();
                if (responseStream == null) throw new InvalidOperationException("Bad response!");

                string responseString;

                using (var sr = new StreamReader(responseStream))
                {
                    responseString = sr.ReadToEnd();
                }

                if (string.IsNullOrEmpty(responseString)) throw new InvalidOperationException("Bad response!");

                var gameResponse = JsonConvert.DeserializeObject<dynamic>(responseString);

                var publishers = new List<IdNameMapping>();
                
                if (gameResponse.status != "Success") return publishers;
                
                var result = new RouteValueDictionary(gameResponse.data.publishers);
                var developerJson = JsonConvert.SerializeObject(result.Values);
                publishers.AddRange(JsonConvert.DeserializeObject<List<IdNameMapping>>(developerJson));

                return publishers;
            }
        }
    }
}