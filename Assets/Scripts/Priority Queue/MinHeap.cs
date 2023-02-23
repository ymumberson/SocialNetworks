using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Synthezised from: https://www.geeksforgeeks.org/max-heap-in-java/#:~:text=A%20max-heap%20is%20a,child%20at%20index%202k%20%2B%202.
 *
 * While we could store scores, this wouldn't work because personalities can change overtime
 * and so we should always re-evaluate these each time.
 * -> More expensive but allows for dynamic behaviour.
 * 
 * Also assumes size is at least 3
 */
[System.Serializable]
public class MinHeap
{
    private Agent owner;
    [SerializeField] private Agent[] agents;
    [SerializeField] private int size;
    [SerializeField] private int max_size;
    private float thresh;
    private HashSet<int> agents_attempted_inserted;

    public MinHeap(Agent owner, int max_size)
    {
        this.owner = owner;
        this.size = 0;
        this.max_size = max_size;
        agents = new Agent[max_size];
        thresh = Parameters.Instance.PERSONALITY_THRESHOLD; /* Fetched once at object creation */
        agents_attempted_inserted = new HashSet<int>();
    }

    private int parent(int i)
    {
        return (i - 1) / 2;
    }

    private int left(int i)
    {
        return (2 * i) + 1;
    }

    private int right(int i)
    {
        return (2 * i) + 2;
    }

    private bool isLeaf(int i)
    {
        return (i > (size / 2)-1 && i < size);
    }

    private void swap(int index_a, int index_b)
    {
        Agent temp = agents[index_a];
        agents[index_a] = agents[index_b];
        agents[index_b] = temp;
    }

    private float cost(int index)
    {
        //Debug.Log("Getting cost of index " + index + " for heap " + size + "/" + max_size);
        return agents[index].comparePersonality(this.owner.getPersonality());
    }

    private bool inBounds(int index)
    {
        return index < size && index > 0;
    } 

    private void minHeapify(int i)
    {
        //Debug.Log("MinHeapify index " + i + ", maxsize: " + max_size + ", current size: " + size);
        if (isLeaf(i)) return;
        if (max_size == 1) return; /* Special case */
        //Debug.Log(i + " is not a leaf for " + size + "/" + max_size);

        /* Should be index bounds safe? */
        float cost_i = cost(i);
        float cost_l = cost(left(i));

        if (!inBounds(right(i)))
        {
            if ((cost_i > cost_l))
            {
                swap(i, left(i));
            }
            return;
        }
        //Debug.Log(right(i) + " is in bounds?");

        float cost_r = cost(right(i));

        if (cost_i > cost_l || cost_i > cost_r)
        {

            if (cost_l < cost_r)
            {
                swap(i, left(i));
                minHeapify(left(i));
            }
            else
            {
                swap(i, right(i));
                minHeapify(right(i));
            }
        }
    }

    public bool isEmpty()
    {
        return size == 0;
    }

    public float getMaxValue()
    {
        if (isEmpty())
        {
            return 0;
        } else
        {
            return cost(size);
        }
    }

    public float getMinValue()
    {
        if (isEmpty())
        {
            return 0;
        }
        else
        {
            return cost(0);
        }
    }

    public bool insert(Agent a)
    {
        this.agents_attempted_inserted.Add(a.getAgentID());
        if (a == this.owner) return false; /* Can't insert self */
        float a_cost = a.comparePersonality(this.owner.getPersonality());
        if (a_cost < thresh)
        {
            //Debug.Log("Rejecting because below thresh");
            return false; /* Reject if similarity is less than threshold */
        }

        if (this.contains(a))
        {
            //Debug.Log("Rejecting because I already contain.");
            if (Landscape.Instance.ENABLE_PERSONALITY_TRANSMISSION) a.personalityTransmission(this.owner);
            return false; /* Don't insert if already contained */
        }

        if (size < max_size)
        {
            agents[size] = a;

            int i = size;
            while (cost(i) < cost(parent(i)))
            {
                swap(i, parent(i));
                i = parent(i);
            }

            ++size;
            return true;
        }

        /* If queue doesn't have room, or new agent is a worse friend */
        //if (a_cost <= cost(size - 1))
        //{
        //    Debug.Log("Rejecting insert: (" + a_cost + " <= " + cost(size - 1) + ", wait is that the wrong value? " + cost(0));
        //    return false;
        //}

        //if (a_cost <= cost(0))
        if (a_cost < cost(0))
        {
            //Debug.Log("Rejecting insert: (" + a_cost + " / " + cost(0));
            return false;
        }

        //Debug.Log("Found a better friend (" + a.comparePersonality(this.owner.getPersonality()) + " > " + cost(size - 1));
        //agents[size - 1] = a;
        agents[0] = a;
        minHeapify(0);

        //int i = size-1;
        //while (cost(i) < cost(parent(i)))
        //{
        //    swap(i, parent(i));
        //    i = parent(i);
        //}

        //Debug.Log("Finished reheapifying: " + this.toString());

        return true;
    }

    public Agent[] getAgents()
    {
        //return agents;
        return getAllActiveAgents();
    }

    public Agent[] getAllActiveAgents()
    {
        List<Agent> ls = new List<Agent>();
        for (int i=0; i<agents.Length; ++i)
        {
            if (agents[i] != null && agents[i].isActiveAndEnabled)
            {
                ls.Add(agents[i]);
            }
        }
        return ls.ToArray();
    }

    public bool contains(Agent a)
    {
        if (this.isEmpty()) return false;
        for (int i=0; i<size; ++i)
        {
            if (agents[i] == a) return true;
        }
        return false;
    }

    public void printHeap()
    {
        string s = "Max size: " + max_size;
        for (int i = 0; i < size / 2; ++i)
        {
            s += "| Parent node: " + cost(i);
            if (left(i) < size)
            {
                s += "| left node: " + cost(left(i));
            }
            if (right(i) < size)
            {
                s += "| right node: " + cost(right(i));
            }
            s += "/";
        }
        Debug.Log(s);
    }

    public string toString()
    {
        string s = "Max size: " + max_size;
        for (int i=0; i<size; ++i)
        {
            s += "| " + cost(i);
        }
        return s;
    }

    public string toJSON(int index)
    {
        if (index < 0 || index >= size) return "{}";
        return "{\"agentID\":" + agents[index].getAgentID() + ",\"cost\":" + cost(index) + "}";
    }

    public string toJSON()
    {
        string json = "{\"maxsize:\"" + max_size + ",\"agents\":{";

        json += toJSON(0);
        for (int i = 1; i < max_size; ++i)
        {
            json += "," + toJSON(i);
        }

        return json + "}}";
    }

    public string getAgentIDs()
    {
        string s = "";
        for (int i=0; i<size; ++i)
        {
            s += agents[i].getAgentID() + ", ";
        }
        return s;
    }

    public bool hasTriedToInsert(Agent a)
    {
        return this.hasTriedToInsert(a.getAgentID());
    }

    public bool hasTriedToInsert(int agentID)
    {
        return this.agents_attempted_inserted.Contains(agentID);
    }
}
