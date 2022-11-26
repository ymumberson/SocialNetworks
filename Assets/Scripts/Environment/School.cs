using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class School : Building
{
    public School(int x, int y) : base(x,y)
    {
        this.tile_type = TileType.School;
    }
}
