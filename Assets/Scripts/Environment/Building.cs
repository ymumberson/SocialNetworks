using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Building : Tile
{
    public Building(int x, int y) : base(x, y) {
        can_walk_on = true;
    }
}