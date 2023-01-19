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
        ns.agent = a;
        nodeList.Add(ns);
        ++numNodes;
    }

    public void removeAgent(Agent a)
    {
        NodeScript ns_to_remove = null;
        foreach (NodeScript ns in nodeList)
        {
            if (ns.agent == a)
            {
                ns_to_remove = ns;
                ns.destroyAllEdges();
                Destroy(ns.gameObject);
                //updateGraph(); /* Might not be best to update here? (It's only to re-render edges anyway) */
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
        recalculateNetworkDensity();
        recalculateConnectivity();
        recalculateClusteringCoefficient();
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
                //Vector3 accumulated_attraction = new Vector3(0, 0, 0);
                //Vector3 accumulated_target = new Vector3(0, 0, 0);
                //float numNeighbours = 0;
                if (node.hasNeighbour(other_node.agent))
                {
                    //Debug.Log(attractiveForce(node,other_node));
                    //node.moveBy(Vector3.MoveTowards(node_pos, other_pos, 1f).normalized);
                    //Debug.Log(Vector3.MoveTowards(node_pos, other_pos, 1f).normalized);
                    //Debug.Log(node_pos + " | " + (other_pos - node_pos).normalized);


                    //node.moveBy((other_pos - node_pos).normalized * attractiveForce(node, other_node));

                    //node.moveTowards(other_node.getPosition(), ((other_pos - node_pos).normalized * attractiveForce(node, other_node)));
                    //accumulated_attraction += ((other_pos - node_pos).normalized * attractiveForce(node, other_node));
                    //accumulated_target += other_pos;
                    //++numNeighbours;


                    //node.moveBy(attractiveForce(node, other_node));
                    node.moveTowards(other_pos, attractiveForce(node, other_node));
                }
                else
                {
                    //Debug.Log(repulsiveForce(node, other_node));
                    //node.moveBy(Vector3.MoveTowards(other_pos, node_pos, 1f).normalized);
                    //Debug.Log("Not neighbour");
                    //node.moveBy((node_pos - other_pos).normalized * repulsiveForce(node, other_node));


                    //node.moveBy(repulsiveForce(node, other_node));
                    node.moveTowards(other_pos, repulsiveForce(node, other_node));
                }
                
                //node.moveBy(accumulated_attraction);
                //accumulated_target /= numNeighbours;
                //node.moveTowards(accumulated_target, accumulated_attraction);
                
                node.moveBy(towardsCentre(node.getPosition())); /* Just centring nodes */
            }
        }

        //foreach (NodeScript ns in nodeList)
        //{
        //    Agent[] neighbours = ns.getNeighbours();
        //    if (neighbours.Length > 0) /* If it has neighbours */
        //    {
        //        foreach (NodeScript ns_other in nodeList)
        //        {
        //            if (ns != ns_other) /* If not self */
        //            {
        //                float force = 0;
        //                bool isNeighbour = false;
        //                foreach (Agent a in neighbours)
        //                {
        //                    if (ns_other.agent == a)
        //                    {
        //                        isNeighbour = true;
        //                    }
        //                }
        //                if (isNeighbour)
        //                {
        //                    /* Apply attractive force */
        //                    force = attractiveForce(ns, ns_other);
        //                } else
        //                {
        //                    /* Apply repulsive force */
        //                    force = -repulsiveForce(ns, ns_other); /* made negative */
        //                }
        //                Vector3 ns_to_other = (ns_other.getPosition() - ns.getPosition());
        //                ns_to_other.Normalize();
        //                Debug.Log("Ns to other: " + ns_to_other);
        //                Debug.Log("Force: " + force);
        //                Debug.Log("Multiplied: " + force*ns_to_other);
        //                ns.moveBy(ns_to_other * force);
        //                ns_other.moveBy(-ns_to_other * force);
        //            }
        //        }
        //    }

        //}
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
                    Vector2 neighbour_pos = getPosition(neighbours[i]);
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
                    if (ns.hasNeighbour(other.agent))
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

    public List<NodeScript> getNodes(Agent[] agents)
    {
        List<Agent> agent_ls = new List<Agent>(agents);
        List<NodeScript> ns_ls = new List<NodeScript>();
        foreach (NodeScript ns in nodeList)
        {
            if (agent_ls.Contains(ns.agent))
            {
                ns_ls.Add(ns);
            }
        }
        return ns_ls;
    }
}
