using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInfo : MonoBehaviour
{
    public Tile tile;
    public int g, h; /* astar */
    public TileInfo parent = null; /* astar */

    public int f()
    {
        return g + h;
    }
}
