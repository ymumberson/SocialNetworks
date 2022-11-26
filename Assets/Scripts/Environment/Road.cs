using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Road : Tile
{   
    public Road(int x, int y) : base(x,y)
    {
        this.tile_type = TileType.Road;
        can_walk_on = true;
    }
}
