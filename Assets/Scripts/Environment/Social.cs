using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Social : Building
{
    [SerializeField] private bool for_children;
    private List<List<Agent>> social_groups = new List<List<Agent>>();

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

    public void clearSocialGroupsList()
    {
        this.social_groups.Clear();
    }

    /// <summary>
    /// Creates a new social group
    /// </summary>
    /// <returns>The index of the new social group within the list</returns>
    public int createNewSocialGroup()
    {
        List<Agent> ls = new List<Agent>();
        social_groups.Add(ls);
        return social_groups.Count - 1;
    }

    public void addAgentToSocialGroup(int group_index, Agent a)
    {
        social_groups[group_index].Add(a);
    }

    public List<Agent> getSocialGroup(int group_index)
    {
        if (group_index < social_groups.Count && group_index >= 0)
        {
            return social_groups[group_index];
        } 
        else
        {
            Debug.Log("Out of bounds!");
            return null;
        }
    }
}
