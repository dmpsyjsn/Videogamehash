using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace VideoGameHash.Helpers
{
    public static class HttpClientHelper
    {
        public static async Task<T> GetData<T>(string url)
        {
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

                return JsonConvert.DeserializeObject<T>(responseString);
            }
        }
    }
}