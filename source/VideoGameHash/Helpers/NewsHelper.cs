using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;
using VideoGameHash.Models;
using HtmlAgilityPack;

namespace VideoGameHash.Helpers
{
    public class NewsHelper
    {
        public static IList<string> SectionList
        {
            get
            {
                InfoRepository repository = new InfoRepository();

                List<string> SectionNames = new List<string>();
                foreach (InfoType type in repository.GetInfoTypes())
                {
                    SectionNames.Add(type.InfoTypeName);
                }

                return SectionNames;
            }
        }

        public static IList<string> SourceList
        {
            get
            {
                InfoRepository repository = new InfoRepository();

                List<string> SourceNames = new List<string>();
                foreach (InfoSource source in repository.GetSources())
                {
                    SourceNames.Add(source.InfoSourceName);
                }

                return SourceNames;
            }
        }

        public static IList<string> GameSystemList(int section)
        {
            GameSystemsRepository repository = new GameSystemsRepository();
            InfoRepository ir = new InfoRepository();

            List<string> GameSystemNames = new List<string>();
            foreach (GameSystem gameSystem in repository.GetGameSystems())
            {
                if (gameSystem.GameSystemName == "All" || ir.HasArticles(section, gameSystem.Id))
                {
                    GameSystemNames.Add(gameSystem.GameSystemName);
                }
            }

            return GameSystemNames;
        }

        public static int GameSystemId(string GameSystem)
        {
            try
            {
                GameSystemsRepository repository = new GameSystemsRepository();
                return repository.GetGameSystemByGameSystemName(GameSystem).Id;
            }
            catch
            {
                return -1;
            }
        }

        public static bool BadImageCompany(string source)
        {
            return (source == "CVG" || source == "GameSpot" || source == "VGleaks");
        }

        public static int SectionId(string section)
        {
            InfoRepository ir = new InfoRepository();
            return ir.GetInfoTypeId(section);
        }

        public static string SectionTitle(int sectionId)
        {
            InfoRepository ir = new InfoRepository();
            return ir.GetInfoType(sectionId).InfoTypeName;
        }

        public static int SourceId(string source)
        {
            InfoRepository ir = new InfoRepository();
            return ir.GetInfoSourceId(source);
        }

        public static bool UseGameSystem(string section)
        {
            InfoRepository ir = new InfoRepository();
            return ir.UseGameSystem(section);
        }

        public static List<char> Alphabet()
        {
            List<char> alpha = new List<char>();

            for (int i = 0; i < 26; i++)
            {
                alpha.Add(Convert.ToChar(i + 65));
            }

            return alpha;
        }

        //public static string GetMetacriticScore(string gameSystem, string gameTitle)
        //{
        //    gameSystem = gameSystem.Replace(' ', '-').ToLower();
        //    StringBuilder sb = new StringBuilder();
        //    for (int i = 0; i < gameTitle.Length; i++)
        //    {
        //        if ((gameTitle[i] >= '0' && gameTitle[i] <= '9') || (gameTitle[i] >= 'a' && gameTitle[i] <= 'z') ||(gameTitle[i] >= 'A' && gameTitle[i] <= 'Z') || gameTitle[i] == ' ')
        //            sb.Append(gameTitle[i]);
        //    }

        //    gameTitle = sb.ToString().Replace(' ', '-').ToLower();

        //    string url = string.Format("http://www.metacritic.com/game/{0}/{1}", gameSystem, gameTitle);

        //    try
        //    {
        //        HtmlDocument doc = new HtmlDocument();

        //        using (var webClient = new System.Net.WebClient())
        //        {
        //            using (var stream = webClient.OpenRead(url))
        //            {
        //                doc.Load(stream);
        //                string test1 = "";
        //            }
        //        }

        //        return "";
        //    }
        //    catch
        //    {
        //        return string.Empty;
        //    }
        //}
    }
}