using System.Globalization;
using System.Text;
using WordsGame.Models;
using WordsGame.Resources;
using WordsGame.Services;

namespace WordsGame
{
    class Program
    {
        const int TIMER_DURATION = 8000;
        const int MAX_LENGTH = 30;
        const int MIN_LENGTH = 8;
        const int MIN_NEW_WORD_LENGTH = 3;

        static string baseWord = string.Empty;
        static string newWord = string.Empty;
        static string firstPlayer = string.Empty;
        static string secondPlaayer = string.Empty;
        static string activePlayer = string.Empty;
        static bool isTimeOut = false;
        static bool gameFinished = false;
        static Timer turnTimer;

        static ScoreService scoreService;
        static CommandService commandService;

        static void Main(string[] args)
        {
            InitializeGame(out Queue<string> players, out List<string> usedWords);

            var (wins1, wins2, draws) = scoreService.GetCurrentScore(players.Peek(), players.Last());
            Console.WriteLine(Resource.CurrentScore, players.Peek(), wins1, wins2, players.Last(), draws);

            AppDomain.CurrentDomain.ProcessExit += (s, e) =>
            {
                if (!gameFinished && !string.IsNullOrEmpty(activePlayer))
                {
                    var loser = activePlayer;
                    var winner = loser == firstPlayer ? players.Last() : firstPlayer;
                    scoreService.AddResult(new GameResult { Player1 = players.First(), Player2 = players.Last(), Winner = winner });
                }
            };

            bool prevFailed = false;
            while (players.Count > 0)
            {
                activePlayer = players.Dequeue();
                bool moveSuccess = TryExecTurn(activePlayer, usedWords, out bool validMove);

                if (!moveSuccess)
                {
                    switch ((prevFailed, activePlayer == firstPlayer))
                    {
                        case (true, _):
                            Console.WriteLine($"{Resource.Draw}!");
                            scoreService.AddResult(new GameResult { Player1 = players.First(), Player2 = players.Last(), Winner = GameResult.DRAW });
                            gameFinished = true;
                            return;
                        case (false, true):
                            prevFailed = true;
                            Console.WriteLine($"{Resource.LastMove} {players.Peek()}");
                            continue;
                        case (false, false):
                            Console.WriteLine($"{firstPlayer} {Resource.Win}!");
                            scoreService.AddResult(new GameResult { Player1 = players.First(), Player2 = players.Last(), Winner = firstPlayer });
                            gameFinished = true;
                            return;
                    }
                }
                else
                {
                    prevFailed = false;
                }

                if (players.Count == 0)
                {
                    string resultMessage = validMove ? $"{activePlayer} {Resource.Win}!" : $"{Resource.Draw}!";
                    Console.WriteLine(resultMessage);

                    string winner = validMove ? activePlayer : GameResult.DRAW;
                    scoreService.AddResult(new GameResult
                    {
                        Player1 = firstPlayer,
                        Player2 = secondPlaayer,
                        Winner = winner
                    });
                    gameFinished = true;
                    break;
                }

                players.Enqueue(activePlayer);
            }
        }

        static void InitializeGame(out Queue<string> players, out List<string> usedWords)
        {
            SelectLanguage();
            Console.WriteLine(Resource.Meething);

            string name1 = ReadName(string.Format(Resource.EnterPlayerName, 1), Resource.Player + " 1");
            string name2 = ReadName(string.Format(Resource.EnterPlayerName, 2), Resource.Player + " 2");


            players = new Queue<string>([name1, name2]);
            usedWords = new List<string>();
            firstPlayer = players.Peek();
            secondPlaayer = players.Last();
            scoreService = new ScoreService();
            commandService = new CommandService(scoreService, players, usedWords);

            baseWord = GetValidBaseWord();
            Console.WriteLine(Resource.StartGame + baseWord);

            turnTimer = new Timer(TimerCallback, null, Timeout.Infinite, Timeout.Infinite);
        }

        static string ReadName(string prompt, string defaultName)
        {
            Console.Write(prompt);
            string? input = Console.ReadLine()?.Trim();
            return string.IsNullOrWhiteSpace(input) ? defaultName : input;
        }

        static string GetValidBaseWord()
        {
            string word;
            do
            {
                Console.Write(Resource.EnterBaseWord);
                word = (Console.ReadLine() ?? string.Empty).Trim().ToLower();
            }
            while (word.Length < MIN_LENGTH || word.Length > MAX_LENGTH);
            return word;
        }

        static bool TryExecTurn(string activePlayer, List<string> usedWords, out bool validMove)
        {
            isTimeOut = false;

            while (true)
            {
                Console.Write(activePlayer + " " + Resource.Turn + ": ");
                turnTimer.Change(TIMER_DURATION, Timeout.Infinite);

                newWord = GetNewWord();

                turnTimer.Change(Timeout.Infinite, Timeout.Infinite);

                if (newWord.StartsWith("/"))
                {
                    commandService.TryHandle(newWord);
                    Console.WriteLine(Resource.Continue);
                    Console.ReadLine();
                    continue;
                }

                validMove = !usedWords.Contains(newWord) && IsValidWord();

                if (!isTimeOut && validMove)
                {
                    Console.WriteLine(" > " + newWord);
                    usedWords.Add(newWord);
                    return true;
                }

                return false;
            }
        }


        static void SelectLanguage()
        {
            Console.WriteLine("Выберите язык/Select language:\n1 - Русский, 2 - English, Default - English");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Console.ReadLine() == "1" ? "ru" : "en");
        }

        static string GetNewWord()
        {
            var input = new StringBuilder();
            while (!isTimeOut)
            {
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(true);
                    if (key.Key == ConsoleKey.Backspace)
                    {
                        if (input.Length > 0)
                        {
                            input.Length--;
                            Console.Write("\b \b");
                        }
                    }
                    else if (key.Key == ConsoleKey.Enter)
                    {
                        if (input.Length >= MIN_NEW_WORD_LENGTH)
                            break;
                        else
                            Console.Write($"\n{Resource.MoreChars}: ");
                    }
                    else
                    {
                        input.Append(key.KeyChar);
                        Console.Write(key.KeyChar);
                    }
                }
                Thread.Sleep(50);
            }
            Console.WriteLine();
            return input.ToString().Trim().ToLower();
        }

        static bool IsValidWord()
        {
            return newWord.All(c => baseWord.Count(x => x == c) >= newWord.Count(y => y == c));
        }

        static void TimerCallback(object? state)
        {
            Console.WriteLine(Resource.TimeIsUp);
            isTimeOut = true;
        }
    }
}