using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphRendererScript : MonoBehaviour
{
    /*
     * Characteristics we need to calculate:
     * 1) Size: DONE
     *      -> Number of nodes or edges.
     * 2) Path length: TODO
     *      -> Distance between a pair of nodes (Probably store the average)
     * 3) Whole network density: DONE
     *      -> Ratio of actual edges to the maximum number of edges.
     */
    [SerializeField] private GameObject NODE_TEMPLATE;
    [SerializeField] private GameObject EDGE_TEMPLATE;
    [SerializeField] private float minX=-5, maxX=-105, minY=-5, maxY=-105;
    [SerializeField] private List<NodeScript> nodeList;
    [SerializeField] private int numNodes;
    [SerializeField] private int numEdges;
    [SerializeField] private float density;
    [SerializeField] private float avgConnectivity;
    [SerializeField] private float avgClusteringCoefficient;
    [SerializeField] private float avgPathLength;
    private Vector2 lower, upper;
    private Vector2 centre;

    /* For moving nodes */
    private float l;

    private void Awake()
    {
        nodeList = new List<NodeScript>();
        numEdges = 0;
        numNodes = 0;
        density = 0;
        lower = new Vector2(minX, minY);
        upper = new Vector2(maxX, maxY);
        centre = new Vector2((minX + maxX) / 2f, (minY + maxY) / 2f);
        //l = Mathf.Abs(maxX - minX)/5f;
        l = Mathf.Abs(maxX - minX)/20f;
    }

    public Vector2 randomBoundedVector2()
    {
        return new Vector2(Random.Range(minX,maxX), Random.Range(minY, maxY));
    }

    public void addAgent(Agent a)
    {
        /* Instantiate node template and set values */
        NodeScript ns = Instantiate(NODE_TEMPLATE).GetComponent<NodeScript>();
        ns.setBounds(lower,upper);
        ns.setPosition(randomBoundedVector2());
        ns.setAgent(a);
        nodeList.Add(ns);
        ++numNodes;
    }

    public void removeAgent(Agent a)
    {
        NodeScript ns_to_remove = null;
        foreach (NodeScript ns in nodeList)
        {
            if (ns.getAgent() == a)
            {
                ns_to_remove = ns;
                ns.destroyAllEdges();
                ns.removeFromAgent();
                
                /* Could either destroy the object or set it to inactive */
                Destroy(ns.gameObject);
                //ns.gameObject.SetActive(false);

                break;
            }
        }
        if (ns_to_remove != null)
        {
            nodeList.Remove(ns_to_remove);
        }
        --numNodes;
    }

    public void updateGraph() /* Simply needs to update edges, nodes will always re-render themselves when moved */
    {
        repositionNodes();
        redrawEdges();
        //recalculateNetworkDensity();
        //recalculateConnectivity();
        //recalculateClusteringCoefficient();
        //calculateAveragePathLength();
    }

    public void repositionNodes()
    {
        foreach (NodeScript node in nodeList)
        {
            Vector3 node_pos = node.getPosition();
            if (!node.hasNeighbours()) continue;
            foreach (NodeScript other_node in nodeList)
            {
                if (node == other_node) continue;
                Vector2 other_pos = other_node.getPosition();
                if (node.hasNeighbour(other_node.getAgent()))
                {
                    node.moveTowards(other_pos, attractiveForce(node, other_node));
                }
                else
                {
                    node.moveTowards(other_pos, repulsiveForce(node, other_node));
                }
                node.moveBy(towardsCentre(node.getPosition())); /* Just centring nodes */
            }
        }
    }

    private void redrawEdges()
    {
        numEdges = 0;
        foreach (NodeScript ns in nodeList)
        {
            ns.destroyAllEdges();
            Agent[] neighbours = ns.getNeighbours();
            if (neighbours.Length > 0) /* ie only if there are neighbours */
            {
                Vector2 pos = ns.getPosition();
                for (int i = 0; i < neighbours.Length; ++i)
                {
                    //Vector2 neighbour_pos = getPosition(neighbours[i]);
                    Vector2 neighbour_pos = neighbours[i].getNodePosition();
                    if (neighbour_pos.Equals(Vector2.positiveInfinity)) continue;
                    if (!neighbour_pos.Equals(Vector2.positiveInfinity)) /* ie neighbour is valid */
                    {
                        /* Creates a new edge */
                        LineRenderer lr = Instantiate(EDGE_TEMPLATE).GetComponent<LineRenderer>();
                        lr.SetPosition(0, pos);
                        lr.SetPosition(1, neighbour_pos);
                        ns.addLineRenderer(lr);
                        ++numEdges;
                    }

                }
            }

        }
    }

    //public Vector2 getPosition(Agent a)
    //{
    //    foreach (NodeScript ns in nodeList)
    //    {
    //        if (ns.getAgent() == a)
    //        {
    //            return ns.getPosition();
    //        }
    //    }

    //    return Vector2.positiveInfinity; /* ie ERROR */
    //}

    private void recalculateNetworkDensity()
    {
        /* Directed network so density = E / (N(N-1)) */
        density = numEdges / (float)(numNodes * (numNodes - 1));
    }

    public Vector2 attractiveForce(NodeScript u, NodeScript v)
    {
        if (u.getPosition().Equals(v.getPosition())) return Vector2.zero;
        Vector3 pU = u.getPosition();
        Vector3 pV = v.getPosition();
        float distance = Vector3.Distance(pU, pV);

        return ((distance * distance) / l) * (v.getPosition() - u.getPosition()).normalized;
    }

    public Vector2 repulsiveForce(NodeScript u, NodeScript v)
    {
        if (u.getPosition().Equals(v.getPosition())) return Vector2.zero;
        Vector3 pU = u.getPosition();
        Vector3 pV = v.getPosition();
        float distance = Vector3.Distance(pU, pV);

        return ((l*l)/distance) * (v.getPosition() - u.getPosition()).normalized;
    }

    public void recalculateConnectivity()
    {
        foreach (NodeScript ns in nodeList)
        {
            ns.resetDegreeOfConnectivity();
        }
        foreach (NodeScript ns in nodeList)
        {
            Agent[] neighbours = ns.getNeighbours();
            if (neighbours.Length > 0)
            {
                foreach (NodeScript other in nodeList)
                {
                    if (ns.hasNeighbour(other.getAgent()))
                    {
                        ns.incrementDegreeOfConnectivity();
                        other.incrementDegreeOfConnectivity();
                    }
                }
            }
        }

        this.avgConnectivity = 0;
        foreach (NodeScript ns in nodeList)
        {
            this.avgConnectivity += ns.getDegreeOfConnectivity();
        }
        this.avgConnectivity /= nodeList.Count;
    }

    public void recalculateClusteringCoefficient()
    {
        this.avgClusteringCoefficient = 0;
        foreach (NodeScript ns in nodeList)
        {
            ns.calculateClusteringCoefficient();
            avgClusteringCoefficient += ns.getClusteringCoefficient();
        }
        avgClusteringCoefficient /= numNodes;
    }

    public Vector2 towardsCentre(Vector2 pos)
    {
        return (centre - pos).normalized / l; /* opposite way around bc graph is in negative coordinate space */
    }

    //public List<NodeScript> getNodes(Agent[] agents)
    //{
    //    List<Agent> agent_ls = new List<Agent>(agents);
    //    List<NodeScript> ns_ls = new List<NodeScript>();
    //    foreach (NodeScript ns in nodeList)
    //    {
    //        if (agent_ls.Contains(ns.getAgent()))
    //        {
    //            ns_ls.Add(ns);
    //        }
    //    }
    //    return ns_ls;
    //}

    public void calculateAveragePathLength()
    {
        float total = 0;
        List<int> ls = calculateAllShortestPaths();
        foreach (int i in ls)
        {
            total += i;
        }
        this.avgPathLength = total / ls.Count;
    }

    public List<int> calculateAllShortestPaths()
    {
        List<int> path_lengths = new List<int>();

        foreach (NodeScript root in nodeList)
        {
            /* Want to reset each time */
            Queue<NodeScript> nodes_to_visit = new Queue<NodeScript>();
            List<NodeScript> visited_nodes = new List<NodeScript>();

            visited_nodes.Add(root);
            nodes_to_visit.Enqueue(root);
            int depth = 1;
            while (nodes_to_visit.Count > 0)
            {
                NodeScript ns = nodes_to_visit.Dequeue();
                foreach (NodeScript neighbour in ns.getNeighbourNodes())
                {
                    if (!visited_nodes.Contains(neighbour)) /* if not visited */
                    {
                        path_lengths.Add(depth);
                        visited_nodes.Add(neighbour);
                        nodes_to_visit.Enqueue(neighbour);
                    }
                }
                ++depth;
            }
        }

        return path_lengths;
    }
}
