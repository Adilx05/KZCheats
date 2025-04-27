using System;
using System.IO;
using System.Text.Json;
using KZCheats.Core.Models;

namespace KZCheats.Core.Services
{
    public static class KzcService
    {
        public static KzcFile LoadKzcFile(string filePath)
        {
            if (!File.Exists(filePath))
                throw new FileNotFoundException("KZC file not found.", filePath);

            string jsonContent = File.ReadAllText(filePath);
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var kzcFile = JsonSerializer.Deserialize<KzcFile>(jsonContent, options);

            if (kzcFile == null)
                throw new Exception("Failed to parse KZC file.");

            ValidateKzcFile(kzcFile);

            return kzcFile;
        }

        private static void ValidateKzcFile(KzcFile kzc)
        {
            if (string.IsNullOrWhiteSpace(kzc.GameName))
                throw new Exception("KZC file is missing Game Name.");

            if (string.IsNullOrWhiteSpace(kzc.GameVersion))
                throw new Exception("KZC file is missing Game Version.");

            if (string.IsNullOrWhiteSpace(kzc.ProcessName))
                throw new Exception("KZC file is missing Process Name.");

            if (kzc.Cheats == null || kzc.Cheats.Count == 0)
                throw new Exception("KZC file has no cheats.");

            foreach (var cheat in kzc.Cheats)
            {
                if (string.IsNullOrWhiteSpace(cheat.Name))
                    throw new Exception("A cheat is missing its Name.");

                if (string.IsNullOrWhiteSpace(cheat.Address) && (cheat.PointerChain == null || cheat.PointerChain.Count == 0))
                    throw new Exception($"Cheat '{cheat.Name}' must have either Address or PointerChain.");

                if (cheat.Value == null)
                    throw new Exception($"Cheat '{cheat.Name}' is missing its Value.");

                if (string.IsNullOrWhiteSpace(cheat.Type))
                    throw new Exception($"Cheat '{cheat.Name}' is missing its Type.");
            }
        }
    }
}
