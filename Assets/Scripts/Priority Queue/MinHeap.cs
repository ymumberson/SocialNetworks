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

    public MinHeap(Agent owner, int max_size)
    {
        this.owner = owner;
        this.size = 0;
        this.max_size = max_size;
        agents = new Agent[max_size];
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
        return (i > (size / 2) && i <= size);
    }

    private void swap(int index_a, int index_b)
    {
        Agent temp = agents[index_a];
        agents[index_a] = agents[index_b];
        agents[index_b] = temp;
    }

    private float cost(int index)
    {
        return agents[index].comparePersonality(this.owner.getPersonality());
    }

    private void minHeapify(int i)
    {
        if (isLeaf(i)) return;
        if (max_size == 1) return; /* Special case */

        /* Should be index bounds safe? */
        float cost_i = cost(i);
        float cost_l = cost(left(i));
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

    public float getMinValue()
    {
        if (isEmpty())
        {
            return 0;
        } else
        {
            return cost(size);
        }
    }

    public bool insert(Agent a)
    {
        //Debug.Log("maxsize: " + max_size + ", size: " + size);
        //if (size < max_size
        //    || (a.comparePersonality(this.owner.getPersonality()) > cost(size-1)))
        //{
        //    agents[size-1] = a;
        //}
        //else
        //{
        //    return false;
        //}

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
        if (!(a.comparePersonality(this.owner.getPersonality()) > cost(size-1))) return false;
        agents[size-1] = a;
        minHeapify(0);

        //int i = size-1;
        //while (cost(i) < cost(parent(i)))
        //{
        //    swap(i, parent(i));
        //    i = parent(i);
        //}
        return true;
    }

    public Agent[] getAgents()
    {
        return agents;
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
}
