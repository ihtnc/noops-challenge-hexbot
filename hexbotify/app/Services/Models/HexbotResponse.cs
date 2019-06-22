using System.Collections.Generic;

namespace Hexbotify.Services.Models
{
    public class HexbotResponse
    {
        public IEnumerable<HexbotResponseColor> Colors { get; set; }
    }

    public class HexbotResponseColor
    {
        public string Value { get; set; }
        public HexbotResponseCoordinates Coordinates { get; set; }
    }

    public class HexbotResponseCoordinates
    {
        public int X { get; set; }
        public int Y { get; set; }
    }
}