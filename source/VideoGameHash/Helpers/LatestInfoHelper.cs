using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VideoGameHash.Helpers
{
    public class LatestInfoHelper
    {
        public static Dictionary<string, string> LatestNewsUrls 
        {
            get
            {
                var urls = new Dictionary<string, string>();
                urls.Add("IGN.com", "http://feeds.ign.com/ign/games-articles");
                urls.Add("GameSpot", "http://www.gamespot.com/rss/game_updates.php?type=3");
                urls.Add("N4G", "http://n4g.com/rss/news?channel=&sort=latest");
                urls.Add("Eurogamer", "http://www.eurogamer.net/?format=rss&type=news");

                return urls;
            }
        }

        public static Dictionary<string, string> LatestReviewsUrls
        {
            get
            {
                var urls = new Dictionary<string, string>();
                urls.Add("IGN.com", "http://feeds.ign.com/ign/game-reviews");
                urls.Add("GameSpot", "http://www.gamespot.com/rss/game_updates.php?type=5");
                urls.Add("Destructoid", "http://www.destructoid.com/elephant/index.phtml?t=reviews&mode=atom");
                urls.Add("CVG", "http://feeds.feedburner.com/cvg/reviews");

                return urls;
            }
        }

        public static Dictionary<string, string> LatestMediaUrls
        {
            get
            {
                var urls = new Dictionary<string, string>();
                urls.Add("IGN.com", "http://feeds.ign.com/ign/games-videos");
                urls.Add("GameSpot", "http://www.gamespot.com/rss/game_updates.php?type=14");
                urls.Add("Eurogamer", "http://www.eurogamer.net/?format=rss&type=video");
                urls.Add("Giant Bomb", "http://www.giantbomb.com/feeds/video");

                return urls;
            }
        }
    }
}