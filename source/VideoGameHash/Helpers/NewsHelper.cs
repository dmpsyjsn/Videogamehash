//using System;
//using System.Collections.Generic;
//using VideoGameHash.Models;

//namespace VideoGameHash.Helpers
//{
//    public static class NewsHelper
//    {
//        private readonly InfoRepository InfoRepository;
//        private atic readonly GameSystemsRepository SystemRepository();


//        public static IList<string> SectionList
//        {
//            get
//            {
//                var sectionNames = new List<string>();
//                foreach (var type in InfoRepository.GetInfoTypes())
//                {
//                    sectionNames.Add(type.InfoTypeName);
//                }

//                return sectionNames;
//            }
//        }

//        public static IList<string> SourceList
//        {
//            get
//            {
//                var sourceNames = new List<string>();
//                foreach (var source in InfoRepository.GetSources())
//                {
//                    sourceNames.Add(source.InfoSourceName);
//                }

//                return sourceNames;
//            }
//        }

//        public static IList<string> GameSystemList(int section)
//        {
//            var gameSystemNames = new List<string>();
//            foreach (var gameSystem in SystemRepository.GetGameSystems())
//            {
//                if (gameSystem.GameSystemName == "All" || InfoRepository.HasArticles(section, gameSystem.Id))
//                {
//                    gameSystemNames.Add(gameSystem.GameSystemName);
//                }
//            }

//            return gameSystemNames;
//        }

//        public static int GameSystemId(string gameSystem)
//        {
//            try
//            {
//                return SystemRepository.GetGameSystemByGameSystemName(gameSystem).Id;
//            }
//            catch
//            {
//                return -1;
//            }
//        }

//        public static bool BadImageCompany(string source)
//        {
//            return source == "CVG" || source == "GameSpot" || source == "VGleaks";
//        }

//        public static int SectionId(string section)
//        {
//            return InfoRepository.GetInfoTypeId(section);
//        }

//        public static string SectionTitle(int sectionId)
//        {
//            return InfoRepository.GetInfoType(sectionId).InfoTypeName;
//        }

//        public static int SourceId(string source)
//        {
//            return InfoRepository.GetInfoSourceId(source);
//        }

//        public static bool UseGameSystem(string section)
//        {
//            return InfoRepository.UseGameSystem(section);
//        }

//        public static List<char> Alphabet()
//        {
//            var alpha = new List<char>();

//            for (var i = 0; i < 26; i++)
//            {
//                alpha.Add(Convert.ToChar(i + 65));
//            }

//            return alpha;
//        }
//    }
//}