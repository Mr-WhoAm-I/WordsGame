using System.Globalization;
using System.Text;
using WordsGame.Resources;

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
        static Timer turnTimer = new(TimerCallback, null, -1, -1);
        static bool isTimeOut = false;

        static void Main(string[] args)
        {
            InitializeGame(out Queue<string> players, out List<string> usedWords);
            bool prevFailed = false;

            while (players.Count > 0)
            {
                string activePlayer = players.Dequeue();

                bool moveSuccess = TryExecTurn(activePlayer, usedWords, out bool validMove);

                if (!moveSuccess)
                {
                    switch ((prevFailed, activePlayer == firstPlayer))
                    {
                        case (true, _):
                            Console.WriteLine($"{Resource.Draw}!");
                            return;
                        case (false, true):
                            prevFailed = true;
                            Console.WriteLine($"{Resource.LastMove} {players.Peek()}");
                            continue;
                        case (false, false):
                            Console.WriteLine($"{firstPlayer} {Resource.Win}!");
                            return;
                    }
                }
                else
                {
                    prevFailed = false;
                }

                if (players.Count == 0)
                {
                    Console.WriteLine(validMove ? $"{activePlayer} {Resource.Win}!" : $"{Resource.Draw}!");
                    break;
                }

                players.Enqueue(activePlayer);
            }
        }

        static void InitializeGame(out Queue<string> players, out List<string> usedWords)
        {
            SelectLanguage();

            string player = Resource.Player;
            players = new Queue<string>([$"{player} 1", $"{player} 2"]);
            usedWords = [];
            firstPlayer = players.Peek();

            Console.WriteLine(Resource.Meething);
            baseWord = GetValidBaseWord();
            Console.WriteLine(Resource.StartGame + baseWord);
        }

        static bool TryExecTurn(string activePlayer, List<string> usedWords, out bool validMove)
        {
            isTimeOut = false;
            Console.Write($"{activePlayer} {Resource.Turn}: ");
            turnTimer.Change(TIMER_DURATION, -1);
            newWord = GetNewWord();

            validMove = !usedWords.Contains(newWord) && IsValidWord();
            if (!isTimeOut && validMove)
            {
                Console.WriteLine($" > {newWord}");
                usedWords.Add(newWord);
                return true;
            }
            return false;
        }

        static void SelectLanguage()
        {
            Console.WriteLine("Выберите язык/Select language:\n1 - Русский, 2 - English, Default - English");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Console.ReadLine() == "1" ? "ru" : "en");
        }

        static string GetValidBaseWord()
        {
            string word;
            do
            {
                Console.Write(Resource.EnterBaseWord);
                word = (Console.ReadLine() ?? "").Trim().ToLower();
            }
            while (word.Length < MIN_LENGTH || word.Length > MAX_LENGTH);
            return word;
        }

        static string GetNewWord()
        {
            StringBuilder input = new StringBuilder();
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
                        continue;
                    }

                    if (key.Key == ConsoleKey.Enter)
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
            Console.Write($" {Resource.TimeIsUp}");
            isTimeOut = true;
        }
    }
}
