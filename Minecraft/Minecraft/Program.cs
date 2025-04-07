using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
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

        public void MainMenu() // Main Menu
        {
            string input;

            while (true)
            {
                Console.WriteLine("╔═════════╗");
                Console.WriteLine("║1.Game   ║");
                Console.WriteLine("║2.Quit   ║");
                Console.WriteLine("╚═════════╝");
                input = Utility.GetValidStringInput("Pick an option: ");
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

        public void GameInit() // Map Size and Map Generation Initialized
        {
            string mapSizeString;
            int mapSize;

            // Determine Size of Map
            Console.WriteLine("╔═══════════╗");
            Console.WriteLine("║Size of Map║");
            Console.WriteLine("╚═══════════╝");
            mapSizeString = Utility.GetValidStringInput("Medium (m) / Small (s): ");
            mapSize = mapSizeString.ToLower() == "m" ? 24 : 16;

            map = new GameMap();
            player = new Player((mapSize / 2, mapSize / 2));
            map.CreateMap(mapSize, player);
            MainGame();
        }

        public void MainGame() // The Game itself - Display Map and Handle movement
        {
            ConsoleKeyInfo moveInput;
            char selectedBlock;

            while (true)
            {
                map.DisplayMap(player);
                moveInput = Console.ReadKey();
                if (moveInput.Key == ConsoleKey.Q)
                    break;
                else if (moveInput.Key == ConsoleKey.RightArrow || moveInput.Key == ConsoleKey.LeftArrow)
                {
                    player.SwitchMode(moveInput);
                }
                else if (moveInput.Key == ConsoleKey.V)
                {
                    selectedBlock = player.ViewInventory();
                }
                else
                {
                    if (player.Mode == GameMode.Move)
                        player.Move(moveInput.Key.ToString().ToLower(), map.map);
                    else if (player.Mode == GameMode.Mine)
                        player.Mine(moveInput.Key.ToString().ToLower(), map.map);
                    else if (player.Mode == GameMode.Place)
                        player.Place(moveInput.Key.ToString().ToLower(), map.map, player.SelectedBlock);
                }
            }
        }


    }

    static class TileTypes
    {
        public const char Empty = ' ';
        public const char Block = '▒';
        public const char PlacedBlock = '#';
        public const char Tree = '\x05';
    }

    static class Utility
    {
        public static string GetValidStringInput(string localisation) // For Input Loops 
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

    class Player // Player Class - Icon, Position and the Mode of the Player.
    {
        private string icon;
        private (int dx, int dy) playerPos;
        private GameMode mode = GameMode.Move; // Default - Move mode
        private char selectedBlock = TileTypes.PlacedBlock;

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
        public char SelectedBlock
        {
            get { return selectedBlock; }
            set { selectedBlock = value; }
        }
        public Player((int,int) playerPos) : base() // Position is defined on instance
        {
            Icon = "\x01";
            PlayerPos = playerPos;
        }

        public bool GetAdjacentTile(int dy, int dx, List<List<char>> map, out char tile)
        // Dx - Direction X Dy - Direction Y 
        {
            int newDy = playerPos.dy + dy;
            int newDx = playerPos.dx + dx;

            if (newDy >= 0 && newDy < map.Count && newDx >= 0 && newDx < map[newDy].Count)
            {
                // Verify if tile up/down/right/left is a positive number and below map bounds. Return the tile
                tile = map[newDy][newDx];
                return true;
            }
            else
            {
                tile = ' ';
                return false;
            }
        }

        public void HandleDirection(string entry, List<List<char>> map, Action<int, int, char> action)
        {
            char tile;
            int dx = 0;
            int dy = 0;

            switch(entry)
            {
                case "w": dy -= 1; break;
                case "s": dy += 1; break;
                case "a": dx -= 1; break;
                case "d": dx += 1; break;
            }
            if (GetAdjacentTile(dy,dx,map,out tile))
            {
                action(dy, dx, tile);
            }
        }

        public void Move(string entry, List<List<char>> map) // Move Method
        {

            HandleDirection(entry, map, (dy, dx, tile) =>
            {
                if (tile == TileTypes.Empty)
                    playerPos = (playerPos.dx + dx,  playerPos.dy + dy);
            }
            );

        }

        public void Mine(string entry, List<List<char>> map) // Mine Method
        {
            HandleDirection(entry, map, (dy, dx, tile) =>
            {
                if (tile == TileTypes.Block || tile == TileTypes.PlacedBlock)
                {
                    map[playerPos.dy + dy][playerPos.dx + dx] = ' ';
                    Inventory.blocks++;
                }
                else if (tile == TileTypes.Tree)
                {
                    map[playerPos.dy + dy][playerPos.dx + dx] = ' ';
                    Inventory.wood++;
                }
            }
            );
        }

        public void Place(string entry, List<List<char>> map, char selectedBlock) // Place Method
        {
            if ((selectedBlock == TileTypes.PlacedBlock && Inventory.blocks > 0) || (selectedBlock == TileTypes.Tree && Inventory.wood > 0))
            {
                HandleDirection(entry, map, (dy, dx, tile) =>
                {
                    if (tile == TileTypes.Empty)
                    {
                        map[playerPos.dy + dy][playerPos.dx + dx] = selectedBlock;
                        if (selectedBlock == TileTypes.Tree)
                            Inventory.wood--;
                        else if (selectedBlock == TileTypes.PlacedBlock)
                            Inventory.blocks--;
                    }
                }
                );
            }
        }

        public char ViewInventory()
        {
            ConsoleKeyInfo selectInput = new ConsoleKeyInfo();
            char selectBlocks = '\x10';
            char selectWood = ' ';

            while (true)
            {

                Console.Clear();
                Console.WriteLine("╔═══════════╗");
                Console.WriteLine($"║{selectBlocks} Blocks: {Inventory.blocks}║");
                Console.WriteLine($"║{selectWood} Wood:   {Inventory.wood}║");
                Console.WriteLine("╚═══════════╝");
                selectInput = Console.ReadKey();
                if (selectInput.Key == ConsoleKey.DownArrow || selectInput.Key == ConsoleKey.UpArrow)
                {
                    if (selectBlocks == '\x10')
                    {
                        selectBlocks = ' ';
                        selectWood = '\x10';
                        selectedBlock = TileTypes.Tree;
                    }
                    else if (selectBlocks == ' ')
                    {
                        selectBlocks = '\x10';
                        selectWood = ' ';
                        selectedBlock = TileTypes.PlacedBlock;
                    }
                }
                else if (selectInput.Key == ConsoleKey.Q)
                {
                    break;
                }
                else if (selectInput.Key == ConsoleKey.Enter)
                {
                    return selectedBlock;
                }
            }
            return selectedBlock;

        }

        public void SwitchMode(ConsoleKeyInfo moveInput) // Switch mode
        {
            if (moveInput.Key == ConsoleKey.RightArrow)
                mode = (GameMode)(((int)mode + 1) % 3);
            else if (moveInput.Key == ConsoleKey.LeftArrow)
                mode = (GameMode)(((int)mode - 1 + 3) % 3);
        }
    }

    static class Inventory
    {
        public static int wood = 0;
        public static int blocks = 0;
    }

    class GameMap // Game Map class, has the Map and GameModes
    {
        public List<List<char>> map;
        private static Random rnd = new Random(); // Initialize Random Class

        public GameMap() // Constructor 
        {
            map = new List<List<char>>();
        }
        
        public void CreateMap(int mapSize, Player player) // Generates Map
        {
            for (int y = 0; y < mapSize; y++)
            {
                map.Add(new List<char>());
                for (int x = 0; x < mapSize; x++)
                {
                    if (x != player.PlayerPos.dx || y != player.PlayerPos.dy)
                    {
                        if (rnd.Next(1, 10) == 1) // 25% chance a block spawns
                            map[y].Add(TileTypes.Block);
                        else if (rnd.Next(1, 7) == 1)
                            map[y].Add(TileTypes.Tree);
                        else // Otherwise empty space
                            map[y].Add(TileTypes.Empty);
                    }
                    else
                    {
                        map[y].Add(TileTypes.Empty);
                    }
                }
            }
        }

        public void DisplayMap(Player player) // Display the Map
        {
            if (map == null || map.Count == 0)
                return;
            else
            {
                StringBuilder row = new StringBuilder();

                // Dynamic Map Display based on Player position
                int startX = Math.Max(0, player.PlayerPos.dx - 3); // -3 Relative to Player on X
                int endX = Math.Min(map.Count, player.PlayerPos.dx + 4); // +4  Relative to Player on X
                int startY = Math.Max(0, player.PlayerPos.dy - 2); // -2 Relative to Player on Y
                int endY = Math.Min(map.Count, player.PlayerPos.dy + 3); // +2 Relative to Player on Y

                int width = endX - startX; // Width of the frame

                Console.Clear();
                Console.WriteLine("╔" + new string('═', width) + "╗");
                for (int y = startY; y < endY; y++)
                {
                    row.Append("║");
                    for (int x = startX; x < endX; x++)
                    {
                        if (x == player.PlayerPos.dx && y == player.PlayerPos.dy)
                            row.Append(player.Icon);
                        else
                            row.Append(map[y][x]);
                    }
                    row.Append("║");
                    Console.WriteLine(row.ToString());
                    row.Clear();
                }
                Console.WriteLine("╚" + new string('═', width) + "╝");
                Console.WriteLine("Mode: \x11 " + player.Mode + " \x10");
            }
        }
    }
}
