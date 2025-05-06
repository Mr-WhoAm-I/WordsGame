using WordsGame.Resources;

namespace WordsGame.Services
{
    public class CommandService(ScoreService scoreService, Queue<string> players, List<string> usedWords)
    {
        private readonly ScoreService _scoreService = scoreService;
        private readonly Queue<string> _players = players;
        private readonly List<string> _usedWords = usedWords;

        public bool TryHandle(string input)
        {
            switch (input.Trim().ToLower())
            {
                case "/show-words":
                    Console.WriteLine(Resource.ShowWordsTitle);
                    foreach (var w in _usedWords)
                        Console.WriteLine($"  {w}");
                    return true;

                case "/score":
                    var p1 = _players.First();
                    var p2 = _players.Last();
                    var (wins1, wins2, draws) = _scoreService.GetCurrentScore(p1, p2);
                    Console.WriteLine(Resource.CurrentScore, p1, wins1, wins2, p2, draws);
                    return true;

                case "/total-score":
                    var totals = _scoreService.GetTotalScores();
                    Console.WriteLine(Resource.TotalScore);
                    foreach (var player in totals)
                    {
                        Console.WriteLine(
                            $"  {player.Name}: {player.Wins}W/{player.Losses}L/{player.Draws}D"
                        );
                    }
                    return true;

                default:
                    return false;
            }
        }
    }
}
