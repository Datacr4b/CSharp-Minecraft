using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Minecraft.Properties
{
    public abstract class Tile
    {
        public char Icon { get; protected set; }
        public bool IsMineable { get; protected set; }
        public bool IsPlaceable { get; protected set; }
        public bool IsWalkable { get; protected set; }
        public string InvName { get; protected set; }

        public abstract (string resource, int amount) Mine();

        public override bool Equals(object obj)
        {
            if (obj is Tile other) return InvName == other.InvName;
            return false;
        }

        public override int GetHashCode() => InvName.GetHashCode();
    }

    public class Stone : Tile
    {
        public Stone()
        {
            Icon = '▓';
            IsMineable = true;
            IsWalkable = false;
            IsPlaceable = true;
            InvName = "Stone";
        }

        public override (string resource, int amount) Mine()
        {
            return ("Stone", 1);
        }
    }

    public class PlacedBlock : Tile
    {

        public PlacedBlock()
        {
            Icon = '#';
            IsMineable = true;
            IsWalkable = false;
            IsPlaceable = true;
            InvName = "Blocks";
        }

        public override (string resource, int amount) Mine()
        {
            return ("Blocks", 1);
        }
    }

    public class Tree : Tile
    {

        public Tree()
        {
            Icon = '\x05'; // Tree icon
            IsMineable = true;
            IsWalkable = false;
            IsPlaceable = true;
            InvName = "Wood";
        }

        public override (string resource, int amount) Mine()
        {
            return ("Wood", 1);
        }
    }

    public class Empty : Tile
    {

        public Empty()
        {
            Icon = ' '; 
            IsMineable = false;
            IsWalkable = true;
            IsPlaceable = false;
            InvName = "";
        }

        public override (string resource, int amount) Mine()
        {
            return ("Blocks", 0);
        }
    }
}
