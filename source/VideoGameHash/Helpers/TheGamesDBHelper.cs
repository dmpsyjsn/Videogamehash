using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
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

            var hitMax = Convert.ToInt32(ConfigurationManager.AppSettings["TheGamesDBImageFileName"]);

            var index = 1;
            while (true)
            {
                if (index > hitMax) break;

                var url = $"{ConfigurationManager.AppSettings["TheGamesDBApiUrl"]}Games/ByPlatformID?id={platformId}&fields=publishers&apikey={ConfigurationManager.AppSettings["TheGamesDBApiKey"]}&page={index++}";

                var gameResponse = await HttpClientHelper.GetData<RootObject>(url);

                var cutoff = DateTime.Now.AddMonths(-3);
                games.AddRange(gameResponse.data.games.Where(u => !string.IsNullOrEmpty(u.release_date) && u.release_date.IndexOf('-') > 0 && Convert.ToDateTime(u.release_date) >= cutoff).ToList());
            }

            return games;
        }

        public static async Task<List<IdNameMapping>> GetDataByField(string field)
        {
            var url = $"{ConfigurationManager.AppSettings["TheGamesDBApiUrl"]}/{field}?apikey={ConfigurationManager.AppSettings["TheGamesDBApiKey"]}";
            
            var gameResponse = await HttpClientHelper.GetData<dynamic>(url);

            var data = new List<IdNameMapping>();
            
            if (gameResponse.status != "Success") return data;
            
            var result = new RouteValueDictionary(field.Equals("Developers") ? gameResponse.data.developers : gameResponse.data.publishers); // ToDo: Should revisit this in the future.
            var developerJson = JsonConvert.SerializeObject(result.Values);
            data.AddRange(JsonConvert.DeserializeObject<List<IdNameMapping>>(developerJson));

            return data;
        }
    }
}