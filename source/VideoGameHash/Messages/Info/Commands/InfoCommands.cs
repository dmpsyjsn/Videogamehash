

namespace VideoGameHash.Messages.Info.Commands
{
    public class AddInfoType
    {
        public AddInfoType(string type)
        {
            Type = type;
        }

        public string Type { get; }
    }
    public class AddInfoSource
    {
        public AddInfoSource(string name)
        {
            Name = name;
        }

        public string Name { get; }
    }

    public class AddUrl
    {
        public AddUrl(string section, string source, string gameSystem, string url)
        {
            Section = section;
            Source = source;
            GameSystem = gameSystem;
            Url = url;
        }

        public string Section { get; }
        public string Source { get; }
        public string GameSystem { get; }
        public string Url { get; }
    }
}