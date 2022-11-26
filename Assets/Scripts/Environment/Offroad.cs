using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Offroad : Tile
{
    public Offroad(int x, int y) : base(x,y)
    {
        this.tile_type = TileType.Offroad;
    }
}
