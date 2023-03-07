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
    [SerializeField] public bool SHOW_COMMUNITIES;
    [SerializeField] private GameObject NODE_TEMPLATE;
    [SerializeField] private GameObject EDGE_TEMPLATE;
    [SerializeField] private GameObject BACKGROUND_TEMPLATE;
    [SerializeField] private float minX=-5, maxX=-105, minY=-5, maxY=-105;
    [SerializeField] private List<NodeScript> nodeList;
    [SerializeField] private int COMMUNITY_PATH_LENGTH_CUTOFF = 2;
    [SerializeField] private int MIN_COMMUNITY_SIZE = 3;
    [SerializeField] private int num_communities;
    [SerializeField] private float avg_community_size;
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
    private bool previously_drew_ideal;

    /* For moving nodes */
    private float l;

    private void Awake()
    {
        SHOW_IDEAL_GRAPH = false;
        previously_drew_ideal = false;
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
            if (SHOW_IDEAL_GRAPH)
            {
                repositionNodesIdeal();
                redrawEdgesIdeal();
            }
            else
            {
                repositionNodes();
                redrawEdges();
            }
        }
    }

    public void destroyAll()
    {
        foreach (NodeScript ns in nodeList)
        {
            ns.destroyAllEdges();
            Destroy(ns.gameObject);
        }
        foreach (IdealNode id in idealNodeList)
        {
            id.destroyAllEdges();
            Destroy(id.gameObject);
        }
    }

    public void enableVisuals(bool b)
    {
        this.ENABLE_VISUALS = b;
        calculateIdealNeighbours();
    }

    public void toggleShowCommunities()
    {
        if (SHOW_COMMUNITIES)
        {
            this.SHOW_COMMUNITIES = false;
            Landscape.Instance.unhighlightAllAgents();
        }
        else
        {
            this.SHOW_COMMUNITIES = true;
            this.recalculateGraphProperties();
        }
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

        //System.Diagnostics.Stopwatch s = new System.Diagnostics.Stopwatch();
        //s.Start();
        //System.Diagnostics.Stopwatch s2 = new System.Diagnostics.Stopwatch();
        //s2.Start();
        recalculateNetworkDensity();
        //s2.Stop();
        //Debug.Log("Calulating network density took " + s2.ElapsedMilliseconds + "ms." + Random.value);
        //s2.Reset();
        //s2.Start();
        recalculateConnectivity();
        //s2.Stop();
        //Debug.Log("Calulating connectivity took " + s2.ElapsedMilliseconds + "ms." + Random.value);
        //s2.Reset();
        //s2.Start();
        recalculateClusteringCoefficient();
        //s2.Stop();
        //Debug.Log("Calulating clustering coefficient took " + s2.ElapsedMilliseconds + "ms." + Random.value);
        //s2.Reset();
        //s2.Start();
        calculateAveragePathLength();
        //s2.Stop();
        //Debug.Log("Calulating average path length took " + s2.ElapsedMilliseconds + "ms." + Random.value);
        //s.Stop();
        //Debug.Log("Network calculations took " + s.ElapsedMilliseconds + "ms." + Random.value);

        calculateCommunities();
    }

    public bool repositionNodes()
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
                    //s.Restart();
                    //s.Start();
                    /* Calculate attractive force */
                    float distance = Vector2.Distance(node_pos, other_pos);
                    float attractive_force =
                        (distance * distance) / K;
                    if (float.IsInfinity(attractive_force)) continue;
                    Vector2 direction = other_pos - node_pos;
                    direction.Normalize();
                    sum_attractive += direction * attractive_force;
                    //s.Stop();
                    //print("Time to calculate attractive force: " + s.Elapsed.TotalMilliseconds + "ms.");
                }
                else
                {
                    //s.Reset();
                    //s.Start();
                    /* Calculate repulsive force */
                    float distance = Vector2.Distance(node_pos, other_pos);
                    float repulsive_force =
                        (-C*K*K) / distance;
                    if (float.IsInfinity(repulsive_force)) continue;
                    Vector2 direction = other_pos - node_pos;
                    direction.Normalize();
                    sum_repulsive += direction * repulsive_force;
                    //s.Stop();
                    //print("Time to calculate repulsive force: " + s.Elapsed.TotalMilliseconds + "ms.");
                }
            }
            //s.Reset();
            //s.Start();
            Vector2 sum_forces = sum_repulsive + sum_attractive + towardsCentre(node_pos);
            //s.Stop();
            //print("Time to calculate total force: " + s.Elapsed.TotalMilliseconds + "ms.");

            //Debug.Log("Sum Attractive: " + sum_attractive);
            //Debug.Log("Sum Repulsive: " + sum_repulsive);
            //Debug.Log("Sum foces: " + sum_forces);

            //node.setPosition(node_pos + sum_forces); /* Can be inside each other, but continues while paused */

            //s.Reset();
            //s.Start();
            node.moveRigidBodyPosition(node_pos + sum_forces); /* Can't be inside each other, but stops while paused */
            //s.Stop();
            //print("Time to move rigidbody: " + s.Elapsed.TotalMilliseconds + "ms.");
        }
        return true;
    }

    private void redrawEdges()
    {
        if (previously_drew_ideal)
        {
            foreach (IdealNode id in idealNodeList)
            {
                id.destroyAllEdges();
            }
        }
        previously_drew_ideal = false;
        
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

    private void recalculateNetworkDensity()
    {
        /* Directed network so density = E / (N(N-1)) */
        this.redrawEdges();
        density = numEdges / (float)(numNodes * (numNodes - 1));
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

            /* List of nodes reachable within N steps */
            List<NodeScript> reachableWithinN = new List<NodeScript>();
            //reachableWithinN.Add(root);

            visited_nodes.Add(root);
            nodes_to_visit.Enqueue(root);
            depth_queue.Enqueue(0);
            int maxDepth = 0;
            float avgDepth = 0;
            while (nodes_to_visit.Count > 0)
            {
                NodeScript ns = nodes_to_visit.Dequeue();
                int depth = depth_queue.Dequeue();
                maxDepth = depth;
                /* Add to reachable within N if depth<=n */
                if (depth <= COMMUNITY_PATH_LENGTH_CUTOFF)
                {
                    reachableWithinN.Add(ns);
                }
                foreach (NodeScript neighbour in ns.getNeighbourNodes())
                {
                    if (!visited_nodes.Contains(neighbour)) /* if not visited */
                    {
                        path_lengths.Add(depth);
                        visited_nodes.Add(neighbour);
                        nodes_to_visit.Enqueue(neighbour);
                        depth_queue.Enqueue(depth + 1);
                        avgDepth += depth;

                        ///* Add to reachable within N if depth<=n */
                        //if (depth <= COMMUNITY_PATH_LENGTH_CUTOFF)
                        //{
                        //    reachableWithinN.Add(neighbour);
                        //}
                    }
                }
            }
            max_depth_ls.Add(maxDepth);
            root.setMaxDepth(maxDepth);
            root.setAvgPathLength(avgDepth / visited_nodes.Count);
            root.setCanReachAllNodes(visited_nodes.Count == numNodes);
            root.setReachableInN(reachableWithinN);
            //Debug.Log(root.name + " can reach " + reachableWithinN.Count + " nodes in " + COMMUNITY_PATH_LENGTH_CUTOFF + " steps.");
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

    public void calculateCommunities()
    {
        List<List<NodeScript>> all_communities = new List<List<NodeScript>>();

        /* Reset each node's community_id */
        foreach (NodeScript node in nodeList)
        {
            node.setCommunity(-1,Color.white);
        }

        /*
         * Calculate all groups of nodes who can reach each other
         * within N steps -> N = COMMUNITY_PATH_LENGTH_CUTOFF
         */
        foreach (NodeScript ns in nodeList)
        {
            NodeScript[] neighbours = ns.getReachableInN().ToArray();
            List<NodeScript> common_nodes = new List<NodeScript>();
            //common_nodes.Add(ns);
            foreach (NodeScript n in neighbours)
            {
                common_nodes.Add(n);
            }
            foreach (NodeScript other in neighbours)
            {
                List<NodeScript> reachable = other.getReachableInN();
                foreach (NodeScript common in common_nodes.ToArray())
                {
                    if (!reachable.Contains(common))
                    {
                        common_nodes.Remove(common);
                        //Debug.Log(other.name + " cannot reach " + common + " in " + COMMUNITY_PATH_LENGTH_CUTOFF + " steps.");
                    }
                }
            }
            if (common_nodes.Count >= MIN_COMMUNITY_SIZE) /* min size of community */
            {
                //string s = "";
                //foreach (NodeScript n in common_nodes)
                //{
                //    s += n.name + ", ";
                //}
                //Debug.Log("Found a community! -> " + s);

                /*
                 * Found a community, check that it isn't a subset of another community.
                 * If a community is a subset of this community, then replace the subset with this.
                 */
                bool subset_or_replaced = false;
                //foreach (List<NodeScript> community in all_communities)
                for (int i=0; i<all_communities.Count; ++i)
                {
                    List<NodeScript> community = all_communities[i];
                    int num_matches = 0;
                    foreach (NodeScript n in community)
                    {
                        if (common_nodes.Contains(n)) ++num_matches;
                    }


                    if (num_matches == community.Count && common_nodes.Count > community.Count)
                    {
                        /* Community is subset */
                        all_communities[i] = common_nodes;
                        subset_or_replaced = true;
                        break;
                    }
                    /* Nodes can be in two sets */
                    //if (num_matches == common_nodes.Count)
                    //{
                    //    /* common_nodes is subset so remove (skip) */
                    //    subset_or_replaced = true;
                    //    break;
                    //}

                    /* Nodes cannot be in two sets */
                    if (num_matches > 0)
                    {
                        subset_or_replaced = true;
                        break;
                    }
                }
                if (!subset_or_replaced)
                {
                    all_communities.Add(common_nodes);
                }
            }
            //else
            //{
            //    Debug.Log("No community: size = " + common_nodes.Count);
            //}
        }

        this.num_communities = all_communities.Count;
        //Debug.Log("Number of communities -> " + all_communities.Count);
        //foreach (List<NodeScript> community in all_communities)
        float tally = 0;
        for (int i=0; i<all_communities.Count; ++i)
        {
            List<NodeScript> community = all_communities[i];
            Color community_color = new Color(Random.value,Random.value,Random.value);
            tally += community.Count;
            //string s = "";
            foreach (NodeScript n in community)
            {
                //s += n.name + ",";
                n.setCommunity(i, community_color);
            }
            //print("Community: " + s);
        }
        this.avg_community_size = (tally / all_communities.Count);
    }

    public string toTxt()
    {
        string TAB = "    ";
        string json = "{\n";

        json += TAB + "\"Density\":" + this.density + ",\n";
        json += TAB + "\"Connectivity\":" + this.avgConnectivity + ",\n";
        json += TAB + "\"Clustering\":" + this.avgClusteringCoefficient + ",\n";
        json += TAB + "\"Average_path_length\":" + this.avgPathLength + ",\n";
        json += TAB + "\"Max_depth\":" + this.maxDepth + ",\n";
        json += TAB + "\"Average_depth\":" + this.getAverageDepth() + ",\n";
        json += TAB + "\"Num_communities\":" + this.num_communities + ",\n";
        json += TAB + "\"Average_community_size\":" + this.avg_community_size + ",\n";
        json += TAB + "\"Can_reach_all\":" + this.getPercentCanReachAll() + "\n}";

        return json;
    }

    public void toggleIdeal()
    {
        this.SHOW_IDEAL_GRAPH = !this.SHOW_IDEAL_GRAPH;
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

    private void redrawEdgesIdeal()
    {
        if (!previously_drew_ideal)
        {
            foreach (NodeScript ns in nodeList)
            {
                ns.destroyAllEdges();
            }
        }
        previously_drew_ideal = true;

        numEdges = 0;
        foreach (IdealNode ns in idealNodeList)
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

    public void calculateIdealNeighbours()
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
    }

    public void repositionNodesIdeal()
    {
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
}
