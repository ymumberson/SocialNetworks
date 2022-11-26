using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Tile
{
    public int x, y;
    public bool can_walk_on = false;
    [SerializeField] public TileType tile_type;
    public enum TileType { 
        Road,
        Offroad,
        Social,
        House,
        Workplace,
        School,
    }

    public Tile(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
