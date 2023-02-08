using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphRendererScript : MonoBehaviour
{
    /*
     * Characteristics we need to calculate:
     * 1) Size: DONE
     *      -> Number of nodes or edges.
     * 2) Path length: DONE
     *      -> Distance between a pair of nodes (Probably store the average)
     * 3) Whole network density: DONE
     *      -> Ratio of actual edges to the maximum number of edges.
     */
    [SerializeField] private bool ENABLE_VISUALS;
    [SerializeField] private GameObject NODE_TEMPLATE;
    [SerializeField] private GameObject EDGE_TEMPLATE;
    [SerializeField] private float minX=-5, maxX=-105, minY=-5, maxY=-105;
    [SerializeField] private List<NodeScript> nodeList;
    [SerializeField] private int numNodes;
    [SerializeField] private int numEdges;
    [SerializeField] private float density;
    [SerializeField] private float avgConnectivity;
    [SerializeField] private float avgClusteringCoefficient;
    [SerializeField] private float avgClusteringCoefficient_ifHasFriends;
    [SerializeField] private float avgPathLength;
    [SerializeField] private float percent_nodes_that_can_reach_all_nodes;
    [SerializeField] private float maxDepth;
    [SerializeField] private float avgDepth;
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
        //l = Mathf.Abs(maxX - minX) / 5f;
        l = Mathf.Abs(maxX - minX) / 20f;
        //ENABLE_VISUALS = true;
        //l = 1000f;
    }

    public void enableVisuals(bool b)
    {
        this.ENABLE_VISUALS = b;
    }

    public Vector2 getCentre()
    {
        return this.centre;
    }

    public Vector2 getUpper()
    {
        return this.upper;
    }

    public Vector2 getLower()
    {
        return this.lower;
    }

    public float getDensity()
    {
        return this.density;
    }

    public float getAverageConnectivity()
    {
        return this.avgConnectivity;
    }

    public float getAverageClusteringCoefficient()
    {
        return this.avgClusteringCoefficient;
    }

    public float getAveragePathLength()
    {
        return this.avgPathLength;
    }

    public float getMaxDepth()
    {
        return this.maxDepth;
    }

    public float getAverageDepth()
    {
        return this.avgDepth;
    }

    public float getPercentCanReachAll()
    {
        return this.percent_nodes_that_can_reach_all_nodes;
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
        if (ENABLE_VISUALS)
        {
            repositionNodes();
            redrawEdges();
        }
        
        recalculateNetworkDensity();
        recalculateConnectivity();
        recalculateClusteringCoefficient();
        calculateAveragePathLength();
    }

    public void repositionNodes()
    {
        foreach (NodeScript node in nodeList)
        {
            Vector3 node_pos = node.getPosition();
            if (!node.hasNeighbours()) continue;

            /* Tried setting nodes to the average of their neighbours */
            //Vector3 sum_pos = Vector3.zero;
            //List<NodeScript> neighbours = node.getNeighbourNodes();
            //foreach (NodeScript ns in neighbours)
            //{
            //    sum_pos += ns.transform.position;
            //}
            //sum_pos /= neighbours.Count;
            ////node.setPosition(sum_pos);
            //node.transform.position = sum_pos;

            foreach (NodeScript other_node in nodeList)
            {
                if (node == other_node) continue;
                Vector2 other_pos = other_node.getPosition();
                int count = 0;
                Vector2 sum_position = Vector2.zero;
                Vector2 sum_force = Vector2.zero;
                if (node.hasNeighbour(other_node.getAgent()))
                {
                    node.moveTowards(other_pos, attractiveForce(node, other_node));

                    //sum_position += other_pos;
                    //sum_force += attractiveForce(node, other_node);
                    //++count;

                    //Vector2 moveTowards = (other_node.getPosition() - node.getPosition()).normalized;
                    //moveTowards += attractiveForce(node, other_node);
                    //sum_force += moveTowards;

                    //sum_force += attractiveForce(node, other_node);
                }
                else
                {
                    node.moveTowards(other_pos, repulsiveForce(node, other_node));

                    //Vector2 moveTowards = (node.getPosition() - other_node.getPosition()).normalized;
                    //moveTowards *= repulsiveForce(node, other_node);
                    //sum_force += moveTowards;

                    //sum_force += repulsiveForce(node, other_node);
                }
                //if (count > 0)
                //{
                //    sum_position /= count;
                //    sum_force /= count;
                //    node.moveTowards(sum_position, sum_force);
                //}
                node.moveBy(towardsCentre(node.getPosition())); /* Just centring nodes */

                //sum_force += towardsCentre(node.getPosition()) * (Vector3.Distance(centre,node.getPosition()) / Mathf.Abs(upper.x - lower.x));

                //Debug.Log("Sum force: " + sum_force);
                //node.GetComponent<Rigidbody2D>().AddForce(sum_force);

                //sum_force.Normalize();
                //node.transform.position += new Vector3(sum_force.x, sum_force.y, 0);
                //node.setPosition(node.getPosition() + sum_force);
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

        //return ((distance * distance) / l) * (v.getPosition() - u.getPosition()).normalized;
        Vector2 vec = ((distance * distance) / l) * (v.getPosition() - u.getPosition()).normalized;
        if (float.IsInfinity(vec.x) || float.IsInfinity(vec.y))
        {
            return Vector2.zero;
        }
        else
        {
            return vec;
        }
    }

    public Vector2 repulsiveForce(NodeScript u, NodeScript v)
    {
        if (u.getPosition().Equals(v.getPosition())) return Vector2.zero;
        Vector3 pU = u.getPosition();
        Vector3 pV = v.getPosition();
        float distance = Vector3.Distance(pU, pV);

        return ((l * l) / distance) * (v.getPosition() - u.getPosition()).normalized;
        //Vector2 vec = ((l*l)/distance) * (u.getPosition() - v.getPosition()).normalized;
        //if (float.IsInfinity(vec.x) || float.IsInfinity(vec.y))
        //{
        //    return Vector2.zero;
        //} else
        //{
        //    return vec;
        //}

        ////return 2f * ((u.getPosition() - v.getPosition()).normalized) / numNodes;
        ////return ((u.getPosition() - v.getPosition()).normalized) / (numNodes);

        //Vector3 pU = u.getPosition();
        //Vector3 pV = v.getPosition();
        //float distance = Vector3.Distance(pU, pV);

        //return ((-0.0001f * l*l)/distance) * (u.getPosition() - v.getPosition()).normalized;
        ////Vector2 vec = ((l*l)/distance) * (u.getPosition() - v.getPosition()).normalized;
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
        this.avgClusteringCoefficient_ifHasFriends = 0;
        int count = 0;
        foreach (NodeScript ns in nodeList)
        {
            ns.calculateClusteringCoefficient();
            avgClusteringCoefficient += ns.getClusteringCoefficient();
            if (ns.getAgent().getNumFriends() > 0)
            {
                avgClusteringCoefficient_ifHasFriends += ns.getClusteringCoefficient();
                ++count;
            }
        }
        avgClusteringCoefficient /= numNodes;
        avgClusteringCoefficient_ifHasFriends /= count;
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

    /// <summary>
    /// Calculates the average shortest path lengths of REACHABLE nodes for each node. 
    /// This means that a low average shortest path length might actually indicate isolated clusters, and not 
    /// a low complete reachability.
    /// </summary>
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

    /// <summary>
    /// Returns all shortest paths of REACHABLE nodes for each node.
    /// </summary>
    /// <returns></returns>
    public List<int> calculateAllShortestPaths()
    {
        List<int> path_lengths = new List<int>();
        List<int> max_depth_ls = new List<int>();
        float num_nodes_that_can_reach_all_nodes = 0;
        foreach (NodeScript root in nodeList)
        {
            /* Want to reset each time */
            Queue<NodeScript> nodes_to_visit = new Queue<NodeScript>();
            List<NodeScript> visited_nodes = new List<NodeScript>();
            Queue<int> depth_queue = new Queue<int>();

            visited_nodes.Add(root);
            nodes_to_visit.Enqueue(root);
            depth_queue.Enqueue(1);
            int maxDepth = 0;
            float avgDepth = 0;
            while (nodes_to_visit.Count > 0)
            {
                NodeScript ns = nodes_to_visit.Dequeue();
                int depth = depth_queue.Dequeue();
                maxDepth = depth;
                foreach (NodeScript neighbour in ns.getNeighbourNodes())
                {
                    if (!visited_nodes.Contains(neighbour)) /* if not visited */
                    {
                        path_lengths.Add(depth);
                        visited_nodes.Add(neighbour);
                        nodes_to_visit.Enqueue(neighbour);
                        depth_queue.Enqueue(depth + 1);
                        avgDepth += depth;
                    }
                }
            }
            max_depth_ls.Add(maxDepth);
            root.setMaxDepth(maxDepth);
            root.setAvgPathLength(avgDepth / visited_nodes.Count);
            root.setCanReachAllNodes(visited_nodes.Count == numNodes);
            if (root.getCanReachAlNodes())
            {
                ++num_nodes_that_can_reach_all_nodes;
            }
        }
        this.avgDepth = 0;
        this.maxDepth = 0;
        foreach (int dep in max_depth_ls)
        {
            avgDepth += dep;
            if (dep > maxDepth)
            {
                maxDepth = dep;
            }
        }
        this.avgDepth /= max_depth_ls.Count;
        this.percent_nodes_that_can_reach_all_nodes = num_nodes_that_can_reach_all_nodes / numNodes;
        return path_lengths;
    }
}
