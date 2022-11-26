using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Workplace : Building
{
    public Workplace(int x, int y) : base(x,y)
    {
        this.tile_type = TileType.Workplace;
    }
}
