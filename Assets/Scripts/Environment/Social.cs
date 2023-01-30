using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Social : Building
{
    [SerializeField] private bool for_children;
    public Social(int x, int y, bool for_children) : base(x,y)
    {
        this.tile_type = TileType.Social;
        this.for_children = for_children;
    }

    public bool forAdults()
    {
        return !for_children;
    }

    public bool forChildren()
    {
        return for_children;
    }

    public void setForChildren(bool for_children)
    {
        this.for_children = for_children;
    }
}
