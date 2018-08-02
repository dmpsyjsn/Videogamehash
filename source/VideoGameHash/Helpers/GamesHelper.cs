namespace VideoGameHash.Helpers
{
    public static class GamesHelper
    {
        public static string GetGamesDbPlatformId(string gameSystem)
        {
            switch (gameSystem)
            {
                case "Xbox 360":
                    return "15";
                case "Xbox One":
                    return "4920";
                case "Wii U":
                    return "38";
                case "PS3":
                    return "12";
                case "PS4":
                    return "4919";
                case "Switch":
                    return"4971";
                case "PC":
                    return "1";
                default:
                    return string.Empty;
            }
        }
    }
}