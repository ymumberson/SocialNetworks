using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class House : Building
{
    List<Agent> occupants;
    public House(int x, int y) : base(x,y)
    {
        this.tile_type = TileType.House;
        occupants = new List<Agent>();
    }

    public void addOccupant(Agent a)
    {
        occupants.Add(a);
    }

    public void removeOccupant(Agent a)
    {
        occupants.Remove(a);
    }

    public bool isEmpty()
    {
        return occupants.Count == 0;
    }
}
