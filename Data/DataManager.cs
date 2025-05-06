using System.Text.Json;
using WordsGame.Models;

namespace WordsGame.Data
{
    public static class DataManager
    {
        private static readonly string FilePath = Path.Combine(
            Directory.GetParent(Directory.GetCurrentDirectory())!
                .Parent!
                .Parent!
                .FullName,
            "game_results.json"
        );

        private static readonly JsonSerializerOptions s_options = new()
        {
            WriteIndented = true,
            AllowTrailingCommas = true
        };

        public static List<GameResult> LoadResults()
        {
            if (!File.Exists(FilePath))
                return [];

            var json = File.ReadAllText(FilePath);
            return JsonSerializer.Deserialize<List<GameResult>>(json, s_options) ?? [];
        }

        public static void SaveResults(List<GameResult> results)
        {
            var json = JsonSerializer.Serialize(results, s_options);
            File.WriteAllText(FilePath, json);
        }
    }

}
