using System;
using System.Collections.Generic;

namespace lab5v10
{
    public class Game
    {
        public string Title { get; set; }

        public Game(string title)
        {
            Title = title;
        }

        public virtual void Play()
        {
            Console.WriteLine($"Гра: {Title}");
        }
    }

    public class BoardGame : Game
    {
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }

        public BoardGame(string title, int minPlayers, int maxPlayers) : base(title)
        {
            MinPlayers = minPlayers;
            MaxPlayers = maxPlayers;
        }

        public override void Play()
        {
            Console.WriteLine($"Настільна гра: {Title}, гравців: {MinPlayers}-{MaxPlayers}");
        }
    }

    public class VideoGame : Game
    {
        public string Platform { get; set; }

        public VideoGame(string title, string platform) : base(title)
        {
            Platform = platform;
        }

        public override void Play()
        {
            Console.WriteLine($"Відеогра: {Title}, платформа: {Platform}");
        }
    }

    public class CardGame : Game
    {
        public int NumDecks { get; set; }

        public CardGame(string title, int numDecks) : base(title)
        {
            NumDecks = numDecks;
        }

        public override void Play()
        {
            Console.WriteLine($"Картярська гра: {Title}, колод: {NumDecks}");
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            List<Game> games = new List<Game>
            {
                new BoardGame("Каркасон", 2, 5),
                new VideoGame("Cyberpunk 2077", "PC"),
                new CardGame("Покер", 1)
            };

            foreach (var game in games)
            {
                game.Play();
            }

            Console.WriteLine($"Всього зіграних ігор: {games.Count}");
        }
    }
}