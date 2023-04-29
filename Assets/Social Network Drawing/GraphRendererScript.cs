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
    private List<IdealNode> idealNodeList;

    public ComputeShader computeShader;
    private NodeStruct[] node_structs;
    public float C = 0.000001f;
    public float K = 500;
    public bool CALCULATE_GPU = true;


    /* Network properties */
    [SerializeField] private int numNodes;
    [SerializeField] private int numEdges;
    [SerializeField] private float density;
    [SerializeField] private float avgConnectivity;
    [SerializeField] private float avgClusteringCoefficient;
    [SerializeField] private float avgClusteringCoefficient_ifHasFriends;
    [SerializeField] private float avgPathLength;
    [SerializeField] private float medianPathLength;
    [SerializeField] private float avgPercentCanReach;
    [SerializeField] private float percent_nodes_that_can_reach_all_nodes;
    [SerializeField] private float maxDepth;
    [SerializeField] private float avgDepth;
    [SerializeField] private int num_communities;
    [SerializeField] private float avg_community_size;
    [SerializeField] private float max_community_size;
    [SerializeField] private float min_community_size;
    [SerializeField] private float skewness;

    /* Ideal network properties */
    [SerializeField] private int idealNumEdges;
    [SerializeField] private float idealDensity;
    [SerializeField] private float idealAvgConnectivity;
    [SerializeField] private float idealAvgClusteringCoefficient;
    [SerializeField] private float idealAvgClusteringCoefficient_ifHasFriends;
    [SerializeField] private float idealAvgPathLength;
    [SerializeField] private float idealMedianPathLength;
    [SerializeField] private float idealAvgPercentCanReach;
    [SerializeField] private float idealPercent_nodes_that_can_reach_all_nodes;
    [SerializeField] private float idealMaxDepth;
    [SerializeField] private float idealAvgDepth;
    [SerializeField] private int idealNum_communities;
    [SerializeField] private float idealAvg_community_size;
    [SerializeField] private float idealMax_community_size;
    [SerializeField] private float idealMin_community_size;
    [SerializeField] private float idealSkewness;

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
                //System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                //stopwatch.Start();
                //RepositionNodesGPU();
                //stopwatch.Stop();
                //double gpu_time = stopwatch.ElapsedMilliseconds;
                //stopwatch.Reset();
                //stopwatch.Start();
                //repositionNodes();
                //stopwatch.Stop();
                //print("CPU calulations took " + stopwatch.ElapsedMilliseconds + "ms. GPU calulations took " + gpu_time + "ms.");
                //redrawEdges();

                if (CALCULATE_GPU)
                {
                    RepositionNodesGPU();
                } 
                else
                {
                    repositionNodes();
                }
                
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
        calculateIdealGraphProperties();
        ConstructNodeStructs();
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
            //this.recalculateGraphProperties();
            if (SHOW_IDEAL_GRAPH)
            {
                this.calculateIdealGraphProperties();
            }
            else
            {
                this.recalculateGraphProperties();
            }
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
        if (numNodes <= 0) /* Graph is empty ie whole population died */
        {
            setAllPropertiesToZero();
            return;
        }
        
        if (SHOW_IDEAL_GRAPH)
        {
            calculateIdealGraphProperties();
            return;
        }
        recalculateNetworkDensity();
        recalculateConnectivity();
        recalculateClusteringCoefficient();
        calculateAveragePathLength();
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
                        if (sr.color == Parameters.Instance.UNHIGHLIGHTED_AGENT_COLOR)
                        {
                            lr.sortingOrder = -1;
                        }
                        else
                        {
                            lr.sortingOrder = sr.sortingOrder;
                        }
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
        List<int> ls = new List<int>();
        foreach (NodeScript ns in nodeList)
        {
            this.avgConnectivity += ns.getDegreeOfConnectivity();
            ls.Add(ns.getDegreeOfConnectivity());
        }
        this.avgConnectivity /= nodeList.Count;

        ls.Sort();
        float median = ls[ls.Count / 2];

        /* Calculate standard deviation */
        float standard_deviation = 0;
        foreach (NodeScript ns in nodeList)
        {
            standard_deviation += Mathf.Pow((ns.getDegreeOfConnectivity() - this.avgConnectivity), 2);
        }
        standard_deviation /= nodeList.Count - 1;
        standard_deviation = Mathf.Sqrt(standard_deviation);

        float n = nodeList.Count;
        float sum = 0;
        for (int i=0; i<n; ++i)
        {
            sum += Mathf.Pow((nodeList[i].getDegreeOfConnectivity() - this.avgConnectivity) / (standard_deviation),3);
        }
        this.skewness = (n / ((n - 1) * (n - 2))) * sum;
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
        ls.Sort();
        if (ls.Count > 0)
        {
            this.medianPathLength = ls[ls.Count / 2];
        }
        else
        {
            this.medianPathLength = 0;
        }
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
        this.avgPercentCanReach = 0;
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
                        path_lengths.Add(depth+1);
                        visited_nodes.Add(neighbour);
                        nodes_to_visit.Enqueue(neighbour);
                        depth_queue.Enqueue(depth + 1);
                        avgDepth += depth+1;

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
            root.setAvgPathLength(avgDepth / (visited_nodes.Count-1)); // -1 to account for self
            root.setCanReachAllNodes(visited_nodes.Count == numNodes);
            root.setReachableInN(reachableWithinN);
            //Debug.Log(root.name + " can reach " + reachableWithinN.Count + " nodes in " + COMMUNITY_PATH_LENGTH_CUTOFF + " steps.");
            if (root.getCanReachAlNodes())
            {
                ++num_nodes_that_can_reach_all_nodes;
            }

            float numNodesCanReach = visited_nodes.Count-1; // -1 to account for self
            float percentNodesCanReach = numNodesCanReach / numNodes;
            this.avgPercentCanReach += percentNodesCanReach;
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
        this.avgPercentCanReach /= numNodes;
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
        this.max_community_size = float.MinValue;
        this.min_community_size = float.MaxValue;
        //Debug.Log("Number of communities -> " + all_communities.Count);
        //foreach (List<NodeScript> community in all_communities)
        float tally = 0;
        for (int i=0; i<all_communities.Count; ++i)
        {
            List<NodeScript> community = all_communities[i];
            Color community_color = new Color(Random.value,Random.value,Random.value);
            tally += community.Count;

            if (community.Count < min_community_size)
            {
                min_community_size = community.Count;
            }
            if (community.Count > max_community_size)
            {
                max_community_size = community.Count;
            }

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
        json += TAB + "\"Skewness\":" + this.skewness + ",\n";
        json += TAB + "\"Average_path_length\":" + this.avgPathLength + ",\n";
        json += TAB + "\"Median_path_length\":" + this.medianPathLength + ",\n";
        json += TAB + "\"Average_percent_can_reach\":" + this.avgPercentCanReach + ",\n";
        json += TAB + "\"Max_depth\":" + this.maxDepth + ",\n";
        json += TAB + "\"Average_depth\":" + this.getAverageDepth() + ",\n";
        json += TAB + "\"Num_communities\":" + this.num_communities + ",\n";
        json += TAB + "\"Average_community_size\":" + this.avg_community_size + ",\n";
        json += TAB + "\"Maximum_community_size\":" + this.max_community_size + ",\n";
        json += TAB + "\"Minimum_community_size\":" + this.min_community_size + ",\n";
        json += TAB + "\"Can_reach_all\":" + this.getPercentCanReachAll() + "\n}";

        return json;
    }

    public string titlesCSV()
    {
        return
            "density" + "," +
            "avgConnectivity" + "," +
            "avgClusteringCoefficient" + "," +
            "skewness" + "," +
            "avgPathLength" + "," +
            "medianPathLength" + "," +
            "avgPercentCanReach" + "," +
            "maxDepth" + "," +
            "avgDepth" + "," +
            "num_communities" + "," +
            "avg_community_size" + "," +
            "max_community_size" + "," +
            "min_community_size" + "," +
            "percent_nodes_that_can_reach_all_nodes" + ",";
    }

    public string toCSV()
    {
        return
            density + "," +
            avgConnectivity + "," +
            avgClusteringCoefficient + "," +
            skewness + "," +
            avgPathLength + "," +
            medianPathLength + "," +
            avgPercentCanReach + "," +
            maxDepth + "," +
            avgDepth + "," +
            num_communities + "," +
            avg_community_size + "," +
            max_community_size + "," +
            min_community_size + "," +
            percent_nodes_that_can_reach_all_nodes + ",";
    }

    public void toggleIdeal()
    {
        this.SHOW_IDEAL_GRAPH = !this.SHOW_IDEAL_GRAPH;
    }

    public void calculateIdealGraphProperties()
    {
        /* Calculate ideal graph properties */
        recalculateNetworkDensityIdeal();
        recalculateConnectivityIdeal();
        recalculateClusteringCoefficientIdeal();
        calculateAveragePathLengthIdeal();
        calculateCommunitiesIdeal();
    }

    private void recalculateNetworkDensityIdeal()
    {
        /* Directed network so density = E / (N(N-1)) */
        this.redrawEdgesIdeal();
        idealDensity = idealNumEdges / (float)(numNodes * (numNodes - 1)); /* ideal num nodes == num nodes */
    }

    public void recalculateConnectivityIdeal()
    {
        foreach (IdealNode ns in idealNodeList)
        {
            ns.resetDegreeOfConnectivity();
        }
        foreach (IdealNode ns in idealNodeList)
        {
            Agent[] neighbours = ns.getNeighbours();
            if (neighbours.Length > 0)
            {
                foreach (IdealNode other in idealNodeList)
                {
                    if (ns.hasNeighbour(other.getAgent()))
                    {
                        ns.incrementDegreeOfConnectivity();
                        other.incrementDegreeOfConnectivity();
                    }
                }
            }
        }

        this.idealAvgConnectivity = 0;
        foreach (IdealNode ns in idealNodeList)
        {
            this.idealAvgConnectivity += ns.getDegreeOfConnectivity();
        }
        this.idealAvgConnectivity /= idealNodeList.Count;

        /* Calculate standard deviation */
        float standard_deviation = 0;
        foreach (IdealNode ns in idealNodeList)
        {
            standard_deviation += Mathf.Pow((ns.getDegreeOfConnectivity() - this.idealAvgConnectivity), 2);
        }
        standard_deviation /= idealNodeList.Count - 1;
        standard_deviation = Mathf.Sqrt(standard_deviation);

        float n = idealNodeList.Count;
        float sum = 0;
        for (int i = 0; i < n; ++i)
        {
            sum += Mathf.Pow((idealNodeList[i].getDegreeOfConnectivity() - this.avgConnectivity) / (standard_deviation), 3);
        }
        this.idealSkewness = (n / ((n - 1) * (n - 2))) * sum;
    }

    public void recalculateClusteringCoefficientIdeal()
    {
        this.idealAvgClusteringCoefficient = 0;
        this.idealAvgClusteringCoefficient_ifHasFriends = 0;
        int count = 0;
        foreach (IdealNode ns in idealNodeList)
        {
            ns.calculateClusteringCoefficient();
            idealAvgClusteringCoefficient += ns.getClusteringCoefficient();
            //if (ns.getAgent().getNumFriends() > 0)
            if (ns.hasNeighbours())
            {
                idealAvgClusteringCoefficient_ifHasFriends += ns.getClusteringCoefficient();
                ++count;
            }
        }
        idealAvgClusteringCoefficient /= numNodes;
        idealAvgClusteringCoefficient_ifHasFriends /= count;
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

        this.idealNumEdges = 0;
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
                        ++idealNumEdges;
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

    /// <summary>
    /// Calculates the average shortest path lengths of REACHABLE nodes for each node. 
    /// This means that a low average shortest path length might actually indicate isolated clusters, and not 
    /// a low complete reachability.
    /// </summary>
    public void calculateAveragePathLengthIdeal()
    {
        float total = 0;
        List<int> ls = calculateAllShortestPathsIdeal();
        ls.Sort();
        if (ls.Count > 0)
        {
            this.idealMedianPathLength = ls[ls.Count / 2];
        }
        else
        {
            this.idealMedianPathLength = 0;
        }
        foreach (int i in ls)
        {
            total += i;
        }
        this.idealAvgPathLength = total / ls.Count;
    }

    /// <summary>
    /// Returns all shortest paths of REACHABLE nodes for each node.
    /// </summary>
    /// <returns></returns>
    public List<int> calculateAllShortestPathsIdeal()
    {
        List<int> path_lengths = new List<int>();
        List<int> max_depth_ls = new List<int>();
        float num_nodes_that_can_reach_all_nodes = 0;
        this.idealAvgPercentCanReach = 0;
        foreach (IdealNode root in idealNodeList)
        {
            /* Want to reset each time */
            Queue<IdealNode> nodes_to_visit = new Queue<IdealNode>();
            List<IdealNode> visited_nodes = new List<IdealNode>();
            Queue<int> depth_queue = new Queue<int>();

            /* List of nodes reachable within N steps */
            List<IdealNode> reachableWithinN = new List<IdealNode>();
            //reachableWithinN.Add(root);

            visited_nodes.Add(root);
            nodes_to_visit.Enqueue(root);
            depth_queue.Enqueue(0);
            int maxDepth = 0;
            float avgDepth = 0;
            while (nodes_to_visit.Count > 0)
            {
                IdealNode ns = nodes_to_visit.Dequeue();
                int depth = depth_queue.Dequeue();
                maxDepth = depth;
                /* Add to reachable within N if depth<=n */
                if (depth <= COMMUNITY_PATH_LENGTH_CUTOFF)
                {
                    reachableWithinN.Add(ns);
                }
                foreach (IdealNode neighbour in ns.getNeighbourNodes())
                {
                    if (!visited_nodes.Contains(neighbour)) /* if not visited */
                    {
                        path_lengths.Add(depth + 1);
                        visited_nodes.Add(neighbour);
                        nodes_to_visit.Enqueue(neighbour);
                        depth_queue.Enqueue(depth + 1);
                        avgDepth += depth + 1;

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
            root.setAvgPathLength(avgDepth / (visited_nodes.Count - 1)); // -1 to account for self
            root.setCanReachAllNodes(visited_nodes.Count == numNodes);
            root.setReachableInN(reachableWithinN);
            //Debug.Log(root.name + " can reach " + reachableWithinN.Count + " nodes in " + COMMUNITY_PATH_LENGTH_CUTOFF + " steps.");
            if (root.getCanReachAlNodes())
            {
                ++num_nodes_that_can_reach_all_nodes;
            }

            float numNodesCanReach = visited_nodes.Count - 1; // -1 to account for self
            float percentNodesCanReach = numNodesCanReach / numNodes;
            this.idealAvgPercentCanReach += percentNodesCanReach;
        }
        this.idealAvgDepth = 0;
        this.idealMaxDepth = 0;
        foreach (int dep in max_depth_ls)
        {
            idealAvgDepth += dep;
            if (dep > idealMaxDepth)
            {
                idealMaxDepth = dep;
            }
        }
        this.idealAvgDepth /= max_depth_ls.Count;
        this.idealPercent_nodes_that_can_reach_all_nodes = num_nodes_that_can_reach_all_nodes / numNodes;
        this.idealAvgPercentCanReach /= numNodes;
        return path_lengths;
    }

    public void calculateCommunitiesIdeal()
    {
        List<List<IdealNode>> all_communities = new List<List<IdealNode>>();

        /* Reset each node's community_id */
        foreach (IdealNode node in idealNodeList)
        {
            node.setCommunity(-1, Color.white);
        }

        /*
         * Calculate all groups of nodes who can reach each other
         * within N steps -> N = COMMUNITY_PATH_LENGTH_CUTOFF
         */
        foreach (IdealNode ns in idealNodeList)
        {
            IdealNode[] neighbours = ns.getReachableInN().ToArray();
            List<IdealNode> common_nodes = new List<IdealNode>();
            //common_nodes.Add(ns);
            foreach (IdealNode n in neighbours)
            {
                common_nodes.Add(n);
            }
            foreach (IdealNode other in neighbours)
            {
                List<IdealNode> reachable = other.getReachableInN();
                foreach (IdealNode common in common_nodes.ToArray())
                {
                    if (!reachable.Contains(common))
                    {
                        common_nodes.Remove(common);
                    }
                }
            }
            if (common_nodes.Count >= MIN_COMMUNITY_SIZE) /* min size of community */
            {
                /*
                 * Found a community, check that it isn't a subset of another community.
                 * If a community is a subset of this community, then replace the subset with this.
                 */
                bool subset_or_replaced = false;
                //foreach (List<NodeScript> community in all_communities)
                for (int i = 0; i < all_communities.Count; ++i)
                {
                    List<IdealNode> community = all_communities[i];
                    int num_matches = 0;
                    foreach (IdealNode n in community)
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
        }

        this.idealNum_communities = all_communities.Count;
        this.idealMax_community_size = float.MinValue;
        this.idealMin_community_size = float.MaxValue;
        float tally = 0;
        for (int i = 0; i < all_communities.Count; ++i)
        {
            List<IdealNode> community = all_communities[i];
            Color community_color = new Color(Random.value, Random.value, Random.value);
            tally += community.Count;

            if (community.Count < idealMin_community_size)
            {
                idealMin_community_size = community.Count;
            }
            if (community.Count > idealMax_community_size)
            {
                idealMax_community_size = community.Count;
            }

            //string s = "";
            foreach (IdealNode n in community)
            {
                //s += n.name + ",";
                n.setCommunity(i, community_color);
            }
            //print("Community: " + s);
        }
        this.idealAvg_community_size = (tally / all_communities.Count);
    }

    public string toTxtIdeal()
    {
        string TAB = "    ";
        string json = "{\n";

        json += TAB + "\"Density\":" + this.idealDensity + ",\n";
        json += TAB + "\"Connectivity\":" + this.idealAvgConnectivity + ",\n";
        json += TAB + "\"Clustering\":" + this.idealAvgClusteringCoefficient + ",\n";
        json += TAB + "\"Skewness\":" + this.idealSkewness + ",\n";
        json += TAB + "\"Average_path_length\":" + this.idealAvgPathLength + ",\n";
        json += TAB + "\"Median_path_length\":" + this.idealMedianPathLength + ",\n";
        json += TAB + "\"Average_percent_can_reach\":" + this.idealAvgPercentCanReach + ",\n";
        json += TAB + "\"Max_depth\":" + this.idealMaxDepth + ",\n";
        json += TAB + "\"Average_depth\":" + this.idealAvgDepth + ",\n";
        json += TAB + "\"Num_communities\":" + this.idealNum_communities + ",\n";
        json += TAB + "\"Average_community_size\":" + this.idealAvg_community_size + ",\n";
        json += TAB + "\"Maximum_community_size\":" + this.idealMax_community_size + ",\n";
        json += TAB + "\"Minimum_community_size\":" + this.idealMin_community_size + ",\n";
        json += TAB + "\"Can_reach_all\":" + this.idealPercent_nodes_that_can_reach_all_nodes + "\n}";

        return json;
    }

    public string titlesCSVIdeal()
    {
        return
            "idealDensity" + "," +
            "idealAvgConnectivity" + "," +
            "idealAvgClusteringCoefficient" + "," +
            "idealSkewness" + "," +
            "idealAvgPathLength" + "," +
            "idealMedianPathLength" + "," +
            "idealAvgPercentCanReach" + "," +
            "idealMaxDepth" + "," +
            "idealAvgDepth" + "," +
            "idealNum_communities" + "," +
            "idealAvg_community_size" + "," +
            "idealMax_community_size" + "," +
            "idealMin_community_size" + "," +
            "idealPercent_nodes_that_can_reach_all_nodes" + ",";
    }

    public string toCSVIdeal()
    {
        return
            idealDensity + "," +
            idealAvgConnectivity + "," +
            idealAvgClusteringCoefficient + "," +
            idealSkewness + "," +
            idealAvgPathLength + "," +
            idealMedianPathLength + "," +
            idealAvgPercentCanReach + "," +
            idealMaxDepth + "," +
            idealAvgDepth + "," +
            idealNum_communities + "," +
            idealAvg_community_size + "," +
            idealMax_community_size + "," +
            idealMin_community_size + "," +
            idealPercent_nodes_that_can_reach_all_nodes + ",";
    }
    public void setAllPropertiesToZero()
    {
        /* Network properties */
        this.numNodes = 0;
        this.numEdges = 0;
        this.density = 0;
        this.avgConnectivity = 0;
        this.avgClusteringCoefficient = 0;
        this.avgClusteringCoefficient_ifHasFriends = 0;
        this.avgPathLength = 0;
        this.medianPathLength = 0;
        this.percent_nodes_that_can_reach_all_nodes = 0;
        this.maxDepth = 0;
        this.avgDepth = 0;
        this.num_communities = 0;
        this.avg_community_size = 0;
        this.max_community_size = 0;
        this.min_community_size = 0;
        this.skewness = 0;

        /* Ideal Network properties */
        //this.numNodes = 0; //Not a property of the ideal graph
        this.idealNumEdges = 0;
        this.idealDensity = 0;
        this.idealAvgConnectivity = 0;
        this.idealAvgClusteringCoefficient = 0;
        this.idealAvgClusteringCoefficient_ifHasFriends = 0;
        this.idealAvgPathLength = 0;
        this.idealMedianPathLength = 0;
        this.idealPercent_nodes_that_can_reach_all_nodes = 0;
        this.idealMaxDepth = 0;
        this.idealAvgDepth = 0;
        this.idealNum_communities = 0;
        this.idealAvg_community_size = 0;
        this.idealMax_community_size = 0;
        this.idealMin_community_size = 0;
        this.idealSkewness = 0;
    }

    private void ConstructNodeStructs()
    {
        //NodeStruct[] node_structs = new NodeStruct[numNodes];
        node_structs = new NodeStruct[numNodes];
        for (int i = 0; i < numNodes; ++i)
        {
            node_structs[i] = new NodeStruct();
            node_structs[i].id = (uint)nodeList[i].getAgent().getAgentID();
            node_structs[i].position = nodeList[i].getPosition();

            Agent[] agents = nodeList[i].getNeighbours();

            node_structs[i].numFriends = (uint)agents.Length;
            //node_structs[i].neighbours = new uint[20];
            //for (int j = 0; j < agents.Length; j++)
            //{
            //    node_structs[i].neighbours[j] = (uint)agents[j].getAgentID();
            //}

            if (node_structs[i].numFriends > 0) node_structs[i].friend1 = (uint)agents[0].getAgentID();
            if (node_structs[i].numFriends > 1) node_structs[i].friend2 = (uint)agents[1].getAgentID();
            if (node_structs[i].numFriends > 2) node_structs[i].friend3 = (uint)agents[2].getAgentID();
            if (node_structs[i].numFriends > 3) node_structs[i].friend4 = (uint)agents[3].getAgentID();
            if (node_structs[i].numFriends > 4) node_structs[i].friend5 = (uint)agents[4].getAgentID();
            if (node_structs[i].numFriends > 5) node_structs[i].friend6 = (uint)agents[5].getAgentID();
            if (node_structs[i].numFriends > 6) node_structs[i].friend7 = (uint)agents[6].getAgentID();
            if (node_structs[i].numFriends > 7) node_structs[i].friend8 = (uint)agents[7].getAgentID();
            if (node_structs[i].numFriends > 8) node_structs[i].friend9 = (uint)agents[8].getAgentID();
            if (node_structs[i].numFriends > 9) node_structs[i].friend10 = (uint)agents[9].getAgentID();
            if (node_structs[i].numFriends > 10) node_structs[i].friend11 = (uint)agents[10].getAgentID();
            if (node_structs[i].numFriends > 11) node_structs[i].friend12 = (uint)agents[11].getAgentID();
            if (node_structs[i].numFriends > 12) node_structs[i].friend13 = (uint)agents[12].getAgentID();
            if (node_structs[i].numFriends > 13) node_structs[i].friend14 = (uint)agents[13].getAgentID();
            if (node_structs[i].numFriends > 14) node_structs[i].friend15 = (uint)agents[14].getAgentID();
            if (node_structs[i].numFriends > 15) node_structs[i].friend16 = (uint)agents[15].getAgentID();
            if (node_structs[i].numFriends > 16) node_structs[i].friend17 = (uint)agents[16].getAgentID();
            if (node_structs[i].numFriends > 17) node_structs[i].friend18 = (uint)agents[17].getAgentID();
            if (node_structs[i].numFriends > 18) node_structs[i].friend19 = (uint)agents[18].getAgentID();
            if (node_structs[i].numFriends > 19) node_structs[i].friend20 = (uint)agents[19].getAgentID();

            node_structs[i].force = Vector3.zero;
            node_structs[i].didRun = -1;
        }
    }

    public void UpdateNodeStructsPositions()
    {
        for (int i=0; i<numNodes; ++i)
        {
            node_structs[i].force = Vector3.zero;
            node_structs[i].position = nodeList[i].transform.position;
            //node_structs[i].position = new Vector3(nodeList[i].getPosition().x, nodeList[i].getPosition().y, 0);
            node_structs[i].didRun = -1;
        }
    }

    private void RepositionNodesGPU()
    {
        UpdateNodeStructsPositions();

        int totalSize = sizeof(uint) * 2 + sizeof(float) * 3 + sizeof(uint) * 20 + sizeof(float)*3 + sizeof(int);
        ComputeBuffer nodesBuffer = new ComputeBuffer(numNodes, totalSize);
        nodesBuffer.SetData(node_structs);

        computeShader.SetVector("centre", centre);
        computeShader.SetFloat("C", 0.000001f);
        computeShader.SetFloat("K", 500);
        //computeShader.SetFloat("C", this.C);
        //computeShader.SetFloat("K", this.K);
        computeShader.SetBuffer(0, "nodes", nodesBuffer);
        computeShader.Dispatch(0, node_structs.Length / 1, 1, 1);

        nodesBuffer.GetData(node_structs);

        for (int i=0; i<numNodes; i++)
        {
            //print(nodeList[i].gameObject.name + " -> " + node_structs[i].force);
            //nodeList[i].moveRigidBodyPosition(new Vector3(nodeList[i].getPosition().x,nodeList[i].getPosition().y,0) + node_structs[i].force);
            nodeList[i].moveRigidBodyPosition(node_structs[i].position + node_structs[i].force);
            if (node_structs[i].force.z != 0)
            {
                print(node_structs[i].force.z);
            }
        }

        nodesBuffer.Dispose();
    }
}

//[System.Runtime.InteropServices.StructLayout(System.Runtime.InteropServices.LayoutKind.Sequential)]
public struct NodeStruct
{
    public uint id;
    public Vector3 position;

    //[System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.ByValArray, SizeConst = 20)]
    //public uint[] neighbours;

    public uint friend1;
    public uint friend2;
    public uint friend3;
    public uint friend4;
    public uint friend5;
    public uint friend6;
    public uint friend7;
    public uint friend8;
    public uint friend9;
    public uint friend10;
    public uint friend11;
    public uint friend12;
    public uint friend13;
    public uint friend14;
    public uint friend15;
    public uint friend16;
    public uint friend17;
    public uint friend18;
    public uint friend19;
    public uint friend20;

    public uint numFriends;
    public Vector3 force;

    public int didRun;
};
