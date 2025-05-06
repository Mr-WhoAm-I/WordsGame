using WordsGame.Data;
using WordsGame.Models;

namespace WordsGame.Services
{
    public class ScoreService
    {
        private readonly List<GameResult> _results;

        public ScoreService()
        {
            _results = DataManager.LoadResults();
        }

        public void AddResult(GameResult result)
        {
            _results.Add(result);
            DataManager.SaveResults(_results);
        }

        public (int wins1, int wins2, int draws) GetCurrentScore(string p1, string p2)
        {
            var games = _results
                .Where(r => (r.Player1 == p1 && r.Player2 == p2)
                         || (r.Player1 == p2 && r.Player2 == p1))
                .ToList();

            int wins1 = games.Count(r => r.Winner == p1);
            int wins2 = games.Count(r => r.Winner == p2);
            int draws = games.Count(r => r.Winner == GameResult.DRAW);
            return (wins1, wins2, draws);
        }

        public List<Player> GetTotalScores()
        {
            var stats = _results
                .SelectMany(r => new[] {
                    new { Name = r.Player1, Outcome = r.Winner == r.Player1 ? 1
                                                 : r.Winner == GameResult.DRAW ? 0 : -1 },
                    new { Name = r.Player2, Outcome = r.Winner == r.Player2 ? 1
                                                 : r.Winner == GameResult.DRAW ? 0 : -1 }
                })
                .GroupBy(x => x.Name)
                .Select(p => new Player
                {
                    Name = p.Key,
                    Wins = p.Count(x => x.Outcome == 1),
                    Draws = p.Count(x => x.Outcome == 0),
                    Losses = p.Count(x => x.Outcome == -1)
                })
                .ToList();

            return stats;
        }
    }
}
