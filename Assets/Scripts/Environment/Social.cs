using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Social : Building
{
    public Social(int x, int y) : base(x,y)
    {
        this.tile_type = TileType.Social;
    }
}
