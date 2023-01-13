using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphRendererScript : MonoBehaviour
{
    [SerializeField] private GameObject NODE_TEMPLATE;
    [SerializeField] private GameObject EDGE_TEMPLATE;
    [SerializeField] private float minX=0, maxX=-50, minY=0, maxY=-50;
    [SerializeField] private List<NodeScript> nodeList;

    private void Awake()
    {
        nodeList = new List<NodeScript>();
    }

    public Vector2 randomBoundedVector2()
    {
        return new Vector2(Random.Range(minX,maxX), Random.Range(minY, maxY));
    }

    public void addAgent(Agent a)
    {
        /* Instantiate node template and set values */
        NodeScript ns = Instantiate(NODE_TEMPLATE).GetComponent<NodeScript>();
        ns.setPosition(randomBoundedVector2());
        ns.agent = a;
        nodeList.Add(ns);
    }

    public void removeAgent(Agent a)
    {
        NodeScript ns_to_remove = null;
        foreach (NodeScript ns in nodeList)
        {
            if (ns.agent == a)
            {
                ns_to_remove = ns;
                Destroy(ns.gameObject);
                updateGraph(); /* Might not be best to update here? (It's only to re-render edges anyway) */
                break;
            }
        }
        if (ns_to_remove != null)
        {
            nodeList.Remove(ns_to_remove);
        }
    }

    public void updateGraph() /* Simply needs to update edges, nodes will always re-render themselves when moved */
    {
        foreach (NodeScript ns in nodeList)
        {
            ns.destroyAllEdges();
            Agent[] neighbours = ns.getNeighbours();
            if (neighbours.Length > 0) /* ie only if there are neighbours */
            {
                Vector2 pos = ns.getPosition();
                for (int i=0; i<neighbours.Length; ++i)
                {
                    Vector2 neighbour_pos = getPosition(neighbours[i]);
                    if (!neighbour_pos.Equals(Vector2.positiveInfinity)) /* ie neighbour is valid */
                    {
                        LineRenderer lr = Instantiate(EDGE_TEMPLATE).GetComponent<LineRenderer>();
                        lr.SetPosition(0, pos);
                        lr.SetPosition(1, neighbour_pos);
                        ns.addLineRenderer(lr);
                    }
                    
                }
            }

        }
    }

    public Vector2 getPosition(Agent a)
    {
        foreach (NodeScript ns in nodeList)
        {
            if (ns.agent == a)
            {
                return ns.getPosition();
            }
        }

        return Vector2.positiveInfinity; /* ie ERROR */
    }
}
