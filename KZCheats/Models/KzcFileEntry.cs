using KZCheats.Core.Models;

namespace KZCheats.Models
{
    public class KzcFileEntry
    {
        public string GameName { get; set; }
        public string GameVersion { get; set; }
        public string FilePath { get; set; }
        public KzcFile KzcContent { get; set; }
    }
}
