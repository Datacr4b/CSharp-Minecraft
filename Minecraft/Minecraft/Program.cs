using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Channels;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Minecraft.Properties;

namespace Minecraft
{
    public enum GameMode { Move, Mine, Place }
    class Program
    {
        static void Main(string[] args)
        {
            GameMenu main = new GameMenu(); // Create Instance of GameMenu
            main.MainMenu(); // Start Main Menu
        }
        
        
    }

    class GameMenu
    {
        private GameInitialization gameinit = new GameInitialization(); // Composed of GameInit

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
                    gameinit.GameInit();
                else if (input == "2")
                    break;
                else
                {
                    Console.WriteLine("Invalid Input");
                }
            }
        }


    }

    class GameInitialization
    {
        private GameEngine engine = new GameEngine(); // composed of GameEngine, Player and Map.
        private Player player;
        private GameMap map;
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
            engine.MainGame(player, map);
        }
    }

    class GameEngine
    {
        public GameEngine() { }

        public void MainGame(Player player, GameMap map) // Dependency on player and map from GameInit
        {
            ConsoleKeyInfo moveInput;
            Tile selectedBlock = new PlacedBlock();

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
                    player.SelectedBlock = player.InventoryUI.ViewInventory(selectedBlock);
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

    static class Utility // Utility static
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
        private GameMode mode;
        private Tile selectedBlock;
        private InventoryData playerInvData;
        private InventoryUI inventoryUI;

        public string Icon { get { return icon; } set { icon = value; } }
        public (int dx,int dy) PlayerPos { get { return playerPos; } set { playerPos = value; } }
        public GameMode Mode { get { return mode; } set { mode = value; } }
        public Tile SelectedBlock { get { return selectedBlock; } set { selectedBlock = value; }  }
        public InventoryData PlayerInvData { get { return playerInvData; } set { playerInvData = value; } }
        public InventoryUI InventoryUI { get { return inventoryUI; } set { inventoryUI = value; } }
        public Player((int,int) playerPos) : base() // Position is defined on instance
        {
            Icon = "\x01";
            PlayerPos = playerPos;
            PlayerInvData = new InventoryData();
            inventoryUI = new InventoryUI(PlayerInvData);
            SelectedBlock = new PlacedBlock();
            Mode = GameMode.Move;
        }

        public bool GetAdjacentTile(int dy, int dx, List<List<Tile>> map, out Tile tile)
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
                tile = null;
                return false;
            }
        }

        public void HandleDirection(string entry, List<List<Tile>> map, Action<int, int, Tile> action)
        {
            Tile tile;
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

        public void Move(string entry, List<List<Tile>> map) // Move Method
        {

            HandleDirection(entry, map, (dy, dx, tile) =>
            {
                if (tile.IsWalkable)
                    playerPos = (playerPos.dx + dx,  playerPos.dy + dy);
            }
            );

        }

        public void Mine(string entry, List<List<Tile>> map) // Mine Method
        {
            HandleDirection(entry, map, (dy, dx, tile) =>
            {
                if (tile.IsMineable)
                {
                    map[playerPos.dy + dy][playerPos.dx + dx] = new Empty();
                    PlayerInvData.inventory[tile]++;
                }
            }
            );
        }

        public void Place(string entry, List<List<Tile>> map, Tile selectedBlock) // Place Method
        {
            if (selectedBlock.IsPlaceable && PlayerInvData.inventory[selectedBlock] > 0)
            {
                HandleDirection(entry, map, (dy, dx, tile) =>
                {
                    if (tile.IsWalkable)
                    {
                        map[playerPos.dy + dy][playerPos.dx + dx] = selectedBlock;
                        PlayerInvData.inventory[selectedBlock]--;
                    }
                }
                );
            }
        }

        public void SwitchMode(ConsoleKeyInfo moveInput) // Switch mode
        {
            if (moveInput.Key == ConsoleKey.RightArrow)
                mode = (GameMode)(((int)mode + 1) % 3);
            else if (moveInput.Key == ConsoleKey.LeftArrow)
                mode = (GameMode)(((int)mode - 1 + 3) % 3);
        }
    }

    public class InventoryData
    {
        public Dictionary<Tile, int> inventory = new Dictionary<Tile, int>();

        public InventoryData()
        {
            inventory = new Dictionary<Tile, int>()
            {
                {new PlacedBlock(),0 },
                {new Tree(),0 },
                {new Stone(),0 }
            };
        }

    }

    public class InventoryUI
    {
        private InventoryData invData;
        private int selectedIndex = 0;
        public InventoryUI(InventoryData invData)
        {
            this.invData = invData;
        }

        public Tile ViewInventory(Tile selectedBlock)
        {
            ConsoleKeyInfo selectInput = new ConsoleKeyInfo();
            int i =0;
            int width = 0;
            int widthTemp;

            foreach (KeyValuePair<Tile, int> kvp in invData.inventory) // Determine the longest width
            {
                widthTemp = kvp.Key.InvName.Length;
                if (widthTemp > width)
                {
                    width = widthTemp;  
                }
            }
            

            while (true)
            {

                Console.Clear();
                Console.WriteLine("╔" + new string('═', width+8) + "╗");
                foreach (KeyValuePair<Tile,int> kvp in invData.inventory)
                {
                    Console.WriteLine($"║{(selectedIndex == i ? '\x10' : ' ')}'{kvp.Key.Icon}' {kvp.Key.InvName}: {new string(' ', width - kvp.Key.InvName.Length)}{kvp.Value}║");
                    i++;
                }
                i = 0;
                Console.WriteLine("╚" + new string('═', width+8) + "╝");
                selectInput = Console.ReadKey();
                if (selectInput.Key == ConsoleKey.DownArrow)
                {
                    selectedIndex = (selectedIndex + 1) % 3;
                }
                else if (selectInput.Key == ConsoleKey.UpArrow)
                {
                    selectedIndex = (selectedIndex - 1 + 3) % 3;
                }
                else if (selectInput.Key == ConsoleKey.Q)
                {
                    break;
                }
                else if (selectInput.Key == ConsoleKey.Enter)
                {
                    return selectedBlock = invData.inventory.ElementAt(selectedIndex).Key;
                }
            }
            return selectedBlock;

        }
    }

    class GameMap // Game Map class, has the Map and GameModes
    {
        public List<List<Tile>> map;
        private static Random rnd = new Random(); // Initialize Random Class

        public GameMap() // Constructor 
        {
            map = new List<List<Tile>>();
        }
        
        public void CreateMap(int mapSize, Player player) // Generates Map
        {
            for (int y = 0; y < mapSize; y++)
            {
                map.Add(new List<Tile>());
                for (int x = 0; x < mapSize; x++)
                {
                    if (x != player.PlayerPos.dx || y != player.PlayerPos.dy)
                    {
                        if (rnd.Next(1, 11) == 1) // 10% chance a block spawns
                            map[y].Add(new Stone());
                        else if (rnd.Next(1, 11) == 1)
                            map[y].Add(new Tree());
                        else if (rnd.Next(1, 11) == 1)
                            map[y].Add(new PlacedBlock());
                        else // Otherwise empty space
                            map[y].Add(new Empty());
                    }
                    else
                    {
                        map[y].Add(new Empty());
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
                            row.Append(map[y][x].Icon);
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
