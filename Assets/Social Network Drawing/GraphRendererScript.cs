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
    [SerializeField] public bool SHOW_IDEAL_GRAPH;
    [SerializeField] private GameObject NODE_TEMPLATE;
    [SerializeField] private GameObject EDGE_TEMPLATE;
    [SerializeField] private GameObject BACKGROUND_TEMPLATE;
    [SerializeField] private float minX=-5, maxX=-105, minY=-5, maxY=-105;
    [SerializeField] private List<NodeScript> nodeList;
    private List<IdealNode> idealNodeList;
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
    private GameObject background;

    /* For moving nodes */
    private float l;

    private void Awake()
    {
        nodeList = new List<NodeScript>();
        idealNodeList = new List<IdealNode>();
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

        /* Setting the size/position of the background */
        float graph_width = Mathf.Abs(maxX - minX);
        float graph_height = Mathf.Abs(maxY - minY);
        background = Instantiate(BACKGROUND_TEMPLATE);
        background.transform.localScale = new Vector3(graph_width, graph_height, 1f);
        background.transform.position = new Vector3(minX + (maxX-minX)/2f, minY + (maxY - minY) / 2f, 0);
    }

    private void Update()
    {
        if (ENABLE_VISUALS)
        {
            //System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
            //s.Start();
            if (SHOW_IDEAL_GRAPH)
            {
                repositionNodes3();
            }
            else
            {
                repositionNodes2();
            }
            //s.Stop();
            //Debug.Log("Repositioning nodes took " + s.ElapsedMilliseconds + "ms.");
            //s.Reset();
            //s.Start();
            redrawEdges();
            //s.Stop();
            //Debug.Log("Redrawing edges took " + s.ElapsedMilliseconds + "ms.");
        }
    }

    private void FixedUpdate()
    {
        //if (ENABLE_VISUALS)
        //{
        //    repositionNodes2();
        //    redrawEdges();
        //}
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

        IdealNode ideal = ns.GetComponent<IdealNode>();
        ideal.setBounds(lower,upper);
        ideal.setPosition(randomBoundedVector2());
        ideal.setAgent(a);
        ideal.setMinHeap(new MinHeap(a, a.getMaxNumFriends()));
        idealNodeList.Add(ideal);
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
        removeIdealNode(a);
    }

    private void removeIdealNode(Agent a)
    {
        IdealNode ns_to_remove = null;
        foreach (IdealNode ns in idealNodeList)
        {
            if (ns.getAgent() == a)
            {
                ns_to_remove = ns;
                ns.destroyAllEdges();

                /* Could either destroy the object or set it to inactive */
                Destroy(ns.gameObject);
                //ns.gameObject.SetActive(false);

                break;
            }
        }
        if (ns_to_remove != null)
        {
            idealNodeList.Remove(ns_to_remove);
        }
    }

    public void recalculateGraphProperties()
    {
        if (SHOW_IDEAL_GRAPH)
        {
            calculateIdealGraphProperties();
            return;
        }
        
        //if (ENABLE_VISUALS)
        //{
        //    repositionNodes2();
        //    redrawEdges();
        //}

        //recalculateNetworkDensity(); // 0ms
        //recalculateConnectivity(); // 133ms
        //recalculateClusteringCoefficient(); // 3ms
        //calculateAveragePathLength(); // 1854ms
        //// total = 1991ms (Due to some prints as well)

        System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
        s.Start();
        System.Diagnostics.Stopwatch s2 = new System.Diagnostics.Stopwatch();
        s2.Start();
        recalculateNetworkDensity();
        s2.Stop();
        Debug.Log("Calulating network density took " + s2.ElapsedMilliseconds + "ms." + Random.value);
        s2.Reset();
        s2.Start();
        recalculateConnectivity();
        s2.Stop();
        Debug.Log("Calulating connectivity took " + s2.ElapsedMilliseconds + "ms." + Random.value);
        s2.Reset();
        s2.Start();
        recalculateClusteringCoefficient();
        s2.Stop();
        Debug.Log("Calulating clustering coefficient took " + s2.ElapsedMilliseconds + "ms." + Random.value);
        s2.Reset();
        s2.Start();
        calculateAveragePathLength();
        s2.Stop();
        Debug.Log("Calulating average path length took " + s2.ElapsedMilliseconds + "ms." + Random.value);
        s.Stop();
        Debug.Log("Network calculations took " + s.ElapsedMilliseconds + "ms." + Random.value);
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

    public void repositionNodes2()
    {
        foreach (NodeScript node in nodeList)
        {
            
            Vector2 node_pos = node.getPosition();
            if (!node.hasNeighbours())
            {
                //node.setPosition(node_pos + towardsCentre(node_pos));
                node.moveRigidBodyPosition(node_pos + towardsCentre(node_pos));
                continue;
            }
            Vector2 sum_repulsive = Vector2.zero;
            Vector2 sum_attractive = Vector2.zero;
            const float C = 0.000001f;
            const float K = 500f;
            /* The force on a vertex is the sum of all attractive forces + the sum of all repulsive foces
             *  (+ a little force towards the centre to stop the graph drifting away) */
            foreach (NodeScript other_node in nodeList)
            {
                if (node == other_node) continue;
                Vector2 other_pos = other_node.getPosition();
                
                if (node.hasNeighbour(other_node.getAgent()))
                {
                    /* Calculate attractive force */
                    float distance = Vector2.Distance(node_pos, other_pos);
                    float attractive_force =
                        (distance * distance) / K;
                    if (float.IsInfinity(attractive_force)) continue;
                    Vector2 direction = other_pos - node_pos;
                    direction.Normalize();
                    sum_attractive += direction * attractive_force;
                }
                else
                {
                    /* Calculate repulsive force */
                    float distance = Vector2.Distance(node_pos, other_pos);
                    float repulsive_force =
                        (-C*K*K) / distance;
                    if (float.IsInfinity(repulsive_force)) continue;
                    Vector2 direction = other_pos - node_pos;
                    direction.Normalize();
                    sum_repulsive += direction * repulsive_force;
                }
            }
            Vector2 sum_forces = sum_repulsive + sum_attractive + towardsCentre(node_pos);
            //Debug.Log("Sum Attractive: " + sum_attractive);
            //Debug.Log("Sum Repulsive: " + sum_repulsive);
            //Debug.Log("Sum foces: " + sum_forces);

            //node.setPosition(node_pos + sum_forces); /* Can be inside each other, but continues while paused */
            node.moveRigidBodyPosition(node_pos + sum_forces); /* Can't be inside each other, but stops while paused */
        }
    }

    public void repositionNodes3()
    {
        foreach (IdealNode id in idealNodeList)
        {
            id.setMinHeap(new MinHeap(id.getAgent(), id.getAgent().getMaxNumFriends()));
            foreach (IdealNode other_id in idealNodeList)
            {
                if (id == other_id) continue;
                id.getMinHeap().insert(other_id.getAgent());
            }
        }


        foreach (IdealNode node in idealNodeList)
        {

            Vector2 node_pos = node.getPosition();
            if (!node.hasNeighbours())
            {
                //node.setPosition(node_pos + towardsCentre(node_pos));
                node.moveRigidBodyPosition(node_pos + towardsCentre(node_pos));
                continue;
            }
            Vector2 sum_repulsive = Vector2.zero;
            Vector2 sum_attractive = Vector2.zero;
            const float C = 0.000001f;
            const float K = 500f;
            /* The force on a vertex is the sum of all attractive forces + the sum of all repulsive foces
             *  (+ a little force towards the centre to stop the graph drifting away) */
            foreach (IdealNode other_node in idealNodeList)
            {
                if (node == other_node) continue;
                Vector2 other_pos = other_node.getPosition();

                if (node.hasNeighbour(other_node.getAgent()))
                {
                    /* Calculate attractive force */
                    float distance = Vector2.Distance(node_pos, other_pos);
                    float attractive_force =
                        (distance * distance) / K;
                    if (float.IsInfinity(attractive_force)) continue;
                    Vector2 direction = other_pos - node_pos;
                    direction.Normalize();
                    sum_attractive += direction * attractive_force;
                }
                else
                {
                    /* Calculate repulsive force */
                    float distance = Vector2.Distance(node_pos, other_pos);
                    float repulsive_force =
                        (-C * K * K) / distance;
                    if (float.IsInfinity(repulsive_force)) continue;
                    Vector2 direction = other_pos - node_pos;
                    direction.Normalize();
                    sum_repulsive += direction * repulsive_force;
                }
            }
            Vector2 sum_forces = sum_repulsive + sum_attractive + towardsCentre(node_pos);
            //Debug.Log("Sum Attractive: " + sum_attractive);
            //Debug.Log("Sum Repulsive: " + sum_repulsive);
            //Debug.Log("Sum foces: " + sum_forces);

            //node.setPosition(node_pos + sum_forces); /* Can be inside each other, but continues while paused */
            node.moveRigidBodyPosition(node_pos + sum_forces); /* Can't be inside each other, but stops while paused */
        }
    }

    private void redrawEdges()
    {
        numEdges = 0;
        foreach (NodeScript ns in nodeList)
        {
            ns.destroyAllEdges();
            Agent[] neighbours = ns.getNeighbours();
            SpriteRenderer sr = ns.GetComponent<SpriteRenderer>();
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
                        lr.startColor = sr.color;
                        lr.endColor = sr.color;
                        lr.sortingOrder = sr.sortingOrder;
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
        return (centre - pos).normalized * Vector2.Distance(centre,pos) * 0.025f; /* opposite way around bc graph is in negative coordinate space */
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


    public void calculateIdealGraphProperties()
    {
        /* TODO: Change these to use the ideal graph nodes instead */
        //recalculateNetworkDensity(); // 0ms
        //recalculateConnectivity(); // 133ms
        //recalculateClusteringCoefficient(); // 3ms
        //calculateAveragePathLength(); // 1854ms
        recalculateClusteringCoefficientIdeal();
    }

    public void recalculateClusteringCoefficientIdeal()
    {
        this.avgClusteringCoefficient = 0;
        this.avgClusteringCoefficient_ifHasFriends = 0;
        int count = 0;
        foreach (IdealNode ns in idealNodeList)
        {
            ns.calculateClusteringCoefficient();
            avgClusteringCoefficient += ns.getClusteringCoefficient();
            //if (ns.getAgent().getNumFriends() > 0)
            if (ns.hasNeighbours())
            {
                avgClusteringCoefficient_ifHasFriends += ns.getClusteringCoefficient();
                ++count;
            }
        }
        avgClusteringCoefficient /= numNodes;
        avgClusteringCoefficient_ifHasFriends /= count;
    }
}
