using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IdealNode : MonoBehaviour
{
    /*
     * Characteristics we need to calculate:
     * 1) Degree of connectivity: DONE
     *      -> The number of links to or from the node.
     * 2) Clustering coefficient: DONE
     *      -> The extent to which nodes connected to a given node are
     *          in turn linked to each other.
     */
    private Transform transform;
    [SerializeField] private Agent agent;
    private List<LineRenderer> edges;
    [SerializeField] private int degreeOfConnectivity;
    private Rigidbody2D rb;
    private Vector2 lowerBound;
    private Vector2 upperBound;
    private MinHeap neighbours;

    /// <summary>
    /// This stores the value of clustering coefficient from when it was last calculated.
    /// </summary>
    [SerializeField] private float clusteringCoefficient;
    [SerializeField] private bool canReachAllNodes;
    [SerializeField] private float avgPathLength;
    [SerializeField] private int maxDepth;

    private void Awake()
    {
        this.transform = GetComponent<Transform>();
        this.edges = new List<LineRenderer>();
        this.degreeOfConnectivity = 0;
        rb = GetComponent<Rigidbody2D>();
        lowerBound = Vector2.zero;
        upperBound = Vector2.zero;
    }
    private void Update()
    {
        keepInBounds();
    }

    public void setBounds(Vector2 lower, Vector2 upper)
    {
        lowerBound = lower;
        upperBound = upper;
    }

    public void setAgent(Agent a)
    {
        this.agent = a;
        a.ideal_node = this;
        this.gameObject.name = "IdealNode: " + a.getAgentID();
    }

    public Agent getAgent()
    {
        return this.agent;
    }

    private void keepInBounds()
    {
        Vector2 pos = transform.position;
        transform.position = new Vector3(
            Mathf.Clamp(pos.x, upperBound.x, lowerBound.x),
            Mathf.Clamp(pos.y, upperBound.y, lowerBound.y),
            0f
            );
    }

    public void setPosition(Vector2 pos)
    {
        transform.position = pos;
    }

    public Vector2 getPosition()
    {
        return transform.position;
    }

    public void moveRigidBodyPosition(Vector2 newPosition)
    {
        rb.MovePosition(newPosition);
    }

    public void moveBy(Vector3 pos)
    {
        rb.AddForce(pos);
    }

    public void moveTowards(Vector3 pos, Vector3 speed)
    {
        if (speed.magnitude >= Vector2.Distance(pos, transform.position))
        {
            transform.position = pos + new Vector3(Random.Range(-0.01f, 0.01f), Random.Range(-0.01f, 0.01f), 0);
        }
        else
        {
            rb.MovePosition(transform.position + speed);
        }
    }

    public void setMinHeap(MinHeap h)
    {
        this.neighbours = h;
    }

    public MinHeap getMinHeap()
    {
        return this.neighbours;
    }

    public Agent[] getNeighbours()
    {
        return neighbours.getAgents();
    }
    public List<IdealNode> getNeighbourNodes()
    {
        //return Landscape.Instance.getGraphRenderer().getNodes(getNeighbours());
        List<IdealNode> ls = new List<IdealNode>();
        foreach (Agent a in getNeighbours())
        {
            if (a != null)
            {
                ls.Add(a.ideal_node);
            }

        }
        return ls;
    }

    public bool hasNeighbours()
    {
        return !this.neighbours.isEmpty();
    }

    public bool hasNeighbour(Agent a)
    {
        foreach (Agent neighbour in getNeighbours())
        {
            if (a == neighbour)
            {
                return true;
            }
        }
        return false;
    }

    public void addLineRenderer(LineRenderer lr)
    {
        edges.Add(lr);
    }

    public void destroyAllEdges()
    {
        foreach (LineRenderer lr in edges)
        {
            Destroy(lr.gameObject);
        }
        edges.Clear();
    }

    public void resetDegreeOfConnectivity()
    {
        degreeOfConnectivity = 0;
    }

    public void incrementDegreeOfConnectivity()
    {
        ++degreeOfConnectivity;
    }

    public int getDegreeOfConnectivity()
    {
        return degreeOfConnectivity;
    }

    public void calculateClusteringCoefficient()
    {
        float previous_clustering_coefficient = this.clusteringCoefficient;
        /* Loop through each neighbour, and for each neighbour count the edges with other neighbours of THIS node */
        float numEdgesBetweenNeighbours = 0;
        List<IdealNode> neighbourNodes = getNeighbourNodes();

        /* If node has none or one neighbours then return zero to avoid NaN */
        if (neighbourNodes.Count <= 1)
        {
            this.clusteringCoefficient = 0;
            return;
        }

        foreach (IdealNode nb_node in neighbourNodes)
        {
            List<IdealNode> neighbour_neighbourNodes = nb_node.getNeighbourNodes();
            foreach (IdealNode nb_node2 in neighbourNodes)
            {
                if (nb_node == nb_node2) continue;
                if (neighbour_neighbourNodes.Contains(nb_node2))
                {
                    ++numEdgesBetweenNeighbours;
                }
            }
        }
        float numNeighbours = neighbourNodes.Count;
        float maxPossibleEdges = numNeighbours * (numNeighbours - 1);

        this.clusteringCoefficient = (numEdgesBetweenNeighbours) / (maxPossibleEdges);
    }

    public void calculateClusteringCoefficient2()
    {
        float previous_clustering_coefficient = this.clusteringCoefficient;
        /* Loop through each neighbour, and for each neighbour count 
         * the edges with other neighbours of THIS node */
        float numEdgesBetweenNeighbours = 0;
        List<IdealNode> neighbourNodes = getNeighbourNodes();

        /* If node has none or one neighbours then return zero to avoid NaN */
        if (neighbourNodes.Count <= 1)
        {
            this.clusteringCoefficient = 0;
            return;
        }

        foreach (IdealNode nb_node in neighbourNodes)
        {
            List<IdealNode> neighbour_neighbourNodes = nb_node.getNeighbourNodes();
            foreach (IdealNode nb_node2 in neighbourNodes)
            {
                if (nb_node == nb_node2) continue;
                if (neighbour_neighbourNodes.Contains(nb_node2))
                {
                    ++numEdgesBetweenNeighbours;
                }
            }
        }
        float numNeighbours = neighbourNodes.Count;
        float maxPossibleEdges = numNeighbours * (numNeighbours - 1);

        this.clusteringCoefficient = (numEdgesBetweenNeighbours) / (maxPossibleEdges);
    }

    /// <summary>
    /// Returns the clustering coefficient from the last time it was calculated.
    /// </summary>
    /// <returns></returns> Clustering Coefficient
    public float getClusteringCoefficient()
    {
        return this.clusteringCoefficient;
    }

    public void setCanReachAllNodes(bool b)
    {
        this.canReachAllNodes = b;
    }

    public bool getCanReachAlNodes()
    {
        return this.canReachAllNodes;
    }

    public void setAvgPathLength(float n)
    {
        this.avgPathLength = n;
    }

    public void setMaxDepth(int n)
    {
        this.maxDepth = n;
    }

    public float getAvgPathLength()
    {
        return this.avgPathLength;
    }

    public int getMaxDepth()
    {
        return this.maxDepth;
    }

    public void highlightRed()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;
        sr.sortingOrder = 2;
    }

    public void highlightGreen()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.green;
        sr.sortingOrder = 1;
    }

    public void highlightMagenta()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.magenta;
        sr.sortingOrder = 1;
    }

    public void unHighlight()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.white;
        sr.sortingOrder = 0;
    }

    public void highlightFriendsMagenta()
    {
        foreach (Agent a in this.neighbours.getAgents())
        {
            a.ideal_node.highlightMagenta();
        }
    }
}
