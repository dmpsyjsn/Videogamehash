using System.Collections.Generic;
using System.Xml.Serialization;

namespace VideoGameHash.Models.TheGamesDB
{

    [XmlRoot(ElementName="Game")]
    public class PlatformGamesGame {
        [XmlElement(ElementName="id")]
        public string Id { get; set; }
        [XmlElement(ElementName="GameTitle")]
        public string GameTitle { get; set; }
        [XmlElement(ElementName="thumb")]
        public string Thumb { get; set; }
        [XmlElement(ElementName="ReleaseDate")]
        public string ReleaseDate { get; set; }
    }

    [XmlRoot(ElementName="Data")]
    public class PlatformGamesData {
        [XmlElement(ElementName="Game")]
        public List<PlatformGamesGame> Games { get; set; }
    }
    
}