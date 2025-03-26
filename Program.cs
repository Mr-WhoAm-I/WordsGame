using System.Globalization;
using System.Text;
using WordsGame.Resources;
namespace WordsGame
{
    class Program
    {
        static string baseWord = string.Empty;
        static string newWord = string.Empty;
        static string firstPlayer = string.Empty;
        static Timer turnTimer = new Timer(TimerCallback, null, -1, -1);
        static bool isTimeOut = false;
        static void Main(string[] args)
        {
            Console.WriteLine("Выберите язык/Select language:\n 1 - Русский, 2 - English, Default - English");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo(Console.ReadLine() == "1" ? "ru" : "en");

            string player = Resource.Player;
            List<string> usedWords = new List<string>();
            Queue<string> players = new([$"{player} 1", $"{player} 2"]);

            firstPlayer = players.Peek(); 

            Console.WriteLine(Resource.Meething);

            baseWord = GetValidBaseWord();
            Console.WriteLine(Resource.StartGame + baseWord);

            bool prevFailed = false;
            while (players.Count >= 1)
            {
                isTimeOut = false;
                string activePlayer = players.Dequeue();

                Console.Write($"{activePlayer} {Resource.Turn} (>2): ");
                turnTimer.Change(8000, -1);
                newWord = GetNewWord();
                bool isValid = !usedWords.Contains(newWord) && IsValidWord();
                if (isTimeOut || !isValid)
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
                            Console.WriteLine($"{firstPlayer}  {Resource.Win}!");
                            return;
                    }
                }
                else
                {
                    prevFailed = false;
                }

                if (players.Count == 0)
                {
                    Console.WriteLine(!isValid ? Resource.Draw : activePlayer + ' ' + Resource.Win + '!');
                    break;
                }

                Console.WriteLine($" > {newWord}");
                usedWords.Add(newWord);
                players.Enqueue(activePlayer);
            }



        }

        static string GetValidBaseWord()
        {
            string word;
            do
            {
                Console.Write(Resource.EnterBaseWord);
                word = (Console.ReadLine() ?? "").Trim().ToLower();
            }
            while (word.Length < 8 || word.Length > 30);
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
                        if (input.Length >= 3)
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