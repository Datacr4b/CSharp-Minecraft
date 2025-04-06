using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft
{
    public enum GameMode { Move, Mine, Place }
    class Program
    {
        static void Main(string[] args)
        {
            Game main = new Game();
            main.MainMenu();
        }
        
        
    }

    class Game
    {
        private Player player;
        private GameMap map;
        public Game(int mapSize)
        {
            map = new GameMap(); 
            player = new Player((mapSize / 2, mapSize / 2));
        }
        public Game()
        {
        }

        public void MainMenu()
        {
            // Main Menu
            string input;

            while (true)
            {
                Console.WriteLine("╔═════════╗");
                Console.WriteLine("║1.Game   ║");
                Console.WriteLine("║2.Quit   ║");
                Console.WriteLine("╚═════════╝");
                input = GetValidStringInput("Pick an option: ");
                input = input.Trim().ToLower();
                if (input == "1")
                    GameInit();
                else if (input == "2")
                    break;
                else
                {
                    Console.WriteLine("Invalid Input");
                }
            }
        }

        public void GameInit()
        {
            string mapSizeString;
            int mapSize;

            // Determine Size of Map
            Console.WriteLine("╔═══════════╗");
            Console.WriteLine("║Size of Map║");
            Console.WriteLine("╚═══════════╝");
            mapSizeString = GetValidStringInput("Medium (m) / Small (s): ");
            mapSize = mapSizeString.ToLower() == "m" ? 24 : 16;

            map = new GameMap();
            player = new Player((mapSize / 2, mapSize / 2));
            map.CreateMap(mapSize, player);
            MainGame();
        }

        public void MainGame()
        {
            ConsoleKeyInfo moveInput;

            while (true)
            {
                map.DisplayMap(player);
                moveInput = Console.ReadKey();
                if (moveInput.Key.ToString().ToLower() == "q")
                    break;
                else if (moveInput.Key.ToString().ToLower() == "m")
                {
                    player.SwitchMode();
                }
                if (player.Mode == GameMode.Move)
                    player.Move(moveInput.Key.ToString().ToLower(), map.map);
                else if (player.Mode == GameMode.Mine)
                    player.Mine(moveInput.Key.ToString().ToLower(), map.map);
                else if (player.Mode == GameMode.Place)
                    player.Place(moveInput.Key.ToString().ToLower(), map.map);
            }
        }

        public string GetValidStringInput(string localisation) // For input loops 
        {
            string validInput;

            while (true)
            {
                Console.WriteLine(localisation);
                validInput = Console.ReadLine();
                if (!string.IsNullOrWhiteSpace(validInput))
                    return validInput;
                else
                {
                    Console.WriteLine("Invalid Input");
                }
            }
        }


    }

    class Player
    {
        private string icon;
        private (int dx, int dy) playerPos;
        private GameMode mode = GameMode.Move;

        public string Icon 
        {
            get { return icon; }
            set { icon = value; }
        }
        public (int dx,int dy) PlayerPos
        {
            get { return playerPos; }
            set { playerPos = value; }
        }
        public GameMode Mode
        {
            get { return mode; }
            set { mode = value; }
        }


        public Player((int,int) playerPos) : base()
        {
            Icon = "\x01";
            PlayerPos = playerPos;
        }

        public bool GetAdjacentTile(int dx, int dy, List<List<string>> map, out string tile)
        {
            int newDy = playerPos.dy + dy;
            int newDx = playerPos.dx + dx;

            if (newDy >= 0 && newDy < map.Count && newDx >= 0 && newDx < map[playerPos.dy].Count)
            {
                tile = map[newDy][newDx];
                return true;
            }
            else
            {
                tile = null;
                return false;
            }
        }

        public void Move(string entry, List<List<string>> map)
        {
            string tile;
            switch(entry)
            {
                case "w":
                    if (GetAdjacentTile(0, -1, map, out tile) && tile == " ")
                        playerPos.dy -= 1;
                    break;
                case "s":
                    if (GetAdjacentTile(0, +1, map, out tile) && tile == " ")
                        playerPos.dy += 1;
                    break;
                case "a":
                    if (GetAdjacentTile(-1, 0, map, out tile) && tile == " ")
                        playerPos.dx -= 1;
                    break;
                case "d":
                    if (GetAdjacentTile(+1, 0, map, out tile) && tile == " ")
                        playerPos.dx += 1;
                    break;

            }
        }

        public void Mine(string entry, List<List<string>> map)
        {
            string tile;
            switch (entry)
            {
                case "w":
                    if (GetAdjacentTile(0, -1, map, out tile) && (tile == "▒" || tile == "#"))
                        map[playerPos.dy - 1][playerPos.dx] = " ";
                    break;
                case "s":
                    if (GetAdjacentTile(0, +1, map, out tile) && (tile == "▒" || tile == "#"))
                        map[playerPos.dy + 1][playerPos.dx] = " ";
                    break;
                case "a":
                    if (GetAdjacentTile(-1, 0, map, out tile) && (tile == "▒" || tile == "#"))
                        map[playerPos.dy][playerPos.dx - 1] = " ";
                    break;
                case "d":
                    if (GetAdjacentTile(+1, 0, map, out tile) && (tile == "▒" || tile == "#"))
                        map[playerPos.dy][playerPos.dx + 1] = " ";
                    break;
            }
        }

        public void Place(string entry, List<List<string>> map)
        {
            string tile;
            switch (entry)
            {
                case "w":
                    if (GetAdjacentTile(0, -1, map, out tile) && tile == " ")
                        map[playerPos.dy - 1][playerPos.dx] = "#";
                    break;
                case "s":
                    if (GetAdjacentTile(0, +1, map, out tile) && tile == " ")
                        map[playerPos.dy + 1][playerPos.dx] = "#";
                    break;
                case "a":
                    if (GetAdjacentTile(-1, 0, map, out tile) && tile == " ")
                        map[playerPos.dy][playerPos.dx - 1] = "#";
                    break;
                case "d":
                    if (GetAdjacentTile(+1, 0, map, out tile) && tile == " ")
                        map[playerPos.dy][playerPos.dx + 1] = "#";
                    break;
            }
        }

        public void SwitchMode()
        {
            if (mode == GameMode.Move)
            {
                mode = GameMode.Mine;
            }
            else if (mode == GameMode.Mine)
            {
                mode = GameMode.Place;
            }
            else if (mode == GameMode.Place)
            {
                mode = GameMode.Move;
            }
        }
    }

    class GameMap
    {
        public List<List<string>> map;
        public enum GameMode {Move, Mine, Place}

        public GameMap() // Constructor 
        {
            map = new List<List<string>>();
        }
        
        public void CreateMap(int mapSize, Player player) // Generates Map
        {
            Random rnd = new Random(); // Initialize Random Class
            for (int y = 0; y < mapSize; y++)
            {
                map.Add(new List<string>());
                for (int x = 0; x < mapSize; x++)
                {
                    if (x != player.PlayerPos.dx || y != player.PlayerPos.dy)
                    {
                        if (rnd.Next(0, 3) == 1)
                            map[y].Add("▒");
                        else
                            map[y].Add(" ");
                    }
                    else
                    {
                        map[y].Add(" ");
                    }
                }
            }
        }

        public void DisplayMap(Player player)
        {
            if (map == null || map.Count == 0)
                return;
            else
            {
                // Dynamic Map Display based on Player position
                int startX = Math.Max(0, player.PlayerPos.dx - 2);
                int endX = Math.Min(map.Count, player.PlayerPos.dx + 4);
                int startY = Math.Max(0, player.PlayerPos.dy - 2);
                int endY = Math.Min(map.Count, player.PlayerPos.dy + 3);

                int width = endX - startX;

                Console.Clear();
                Console.WriteLine("╔" + new string('═', width) + "╗");
                for (int y = startY; y < endY; y++)
                {
                    Console.Write("║");
                    for (int x = startX; x < endX; x++)
                    {
                        if (x == player.PlayerPos.dx && y == player.PlayerPos.dy)
                            Console.Write(player.Icon);
                        else
                            Console.Write(map[y][x]);
                    }
                    Console.Write("║");
                    Console.WriteLine();
                }
                Console.WriteLine("╚" + new string('═', width) + "╝");
                Console.WriteLine("Mode: \x11 " + player.Mode + " \x10");
            }
        }
    }
}
