using System.Collections.Generic;

namespace KZCheats.Core.Models
{
    public class KzcFile
    {
        public string GameName { get; set; }
        public string GameVersion { get; set; }
        public string ProcessName { get; set; }
        public List<CheatEntry> Cheats { get; set; }
    }

    public class CheatEntry
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public List<string> PointerChain { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
        public bool Freeze { get; set; } = false;
    }
}
