using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeScript : MonoBehaviour
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
    private void FixedUpdate()
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
        this.agent.setNode(this);
        this.gameObject.name = "Node: " + a.getAgentID();
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

        //Vector2 pos = transform.position;
        //Vector3 pp = new Vector3(
        //    Mathf.Clamp(pos.x, upperBound.x, lowerBound.x),
        //    Mathf.Clamp(pos.y, upperBound.y, lowerBound.y),
        //    0f
        //    );
        //Debug.Log("Clamped pos: " + pp);
    }

    public void setPosition(Vector2 pos)
    {
        transform.position = pos;
    }

    public Vector2 getPosition()
    {
        if (transform == null) Debug.Log("So the script isn't null but the transform is?");
        return transform.position;
    }

    public void moveBy(Vector3 pos)
    {
        //transform.position += pos;
        //Debug.Log("Moving by: " + pos);
        //rb.AddForce(pos*100);
        rb.AddForce(pos);
        //Debug.Log("Adding force " + (pos*100));
        //rb.MovePosition(transform.position + pos);
    }

    public void moveTowards(Vector3 pos, Vector3 speed)
    {
        if (speed.magnitude >= Vector2.Distance(pos,transform.position))
        {
            transform.position = pos + new Vector3(Random.Range(-0.01f,0.01f), Random.Range(-0.01f, 0.01f),0);
        }
        else
        {
            //rb.AddForce(speed);
            rb.MovePosition(transform.position + speed);
        }
    }

    public Agent[] getNeighbours()
    {
        return agent.getFriends();
    }

    public List<NodeScript> getNeighbourNodes()
    {
        //return Landscape.Instance.getGraphRenderer().getNodes(getNeighbours());
        List<NodeScript> ls = new List<NodeScript>();
        foreach (Agent a in getNeighbours())
        {
            if (a != null)
            {
                ls.Add(a.getNode());
            }
            
        }
        return ls;
    }

    public bool hasNeighbours()
    {
        return agent.hasFriends();
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
        /* Loop through each neighbour, and for each neighbour count the edges with other neighbours of THIS node */
        float numEdgesBetweenNeighbours = 0;
        List<NodeScript> neighbourNodes = getNeighbourNodes();

        /* If node has none or one neighbours then return zero to avoid NaN */
        if (neighbourNodes.Count <= 1)
        {
            this.clusteringCoefficient = 0;
            return;
        }

        foreach (NodeScript nb_node in neighbourNodes)
        {
            List<NodeScript> neighbour_neighbourNodes = nb_node.getNeighbourNodes();
            foreach (NodeScript ns in neighbour_neighbourNodes)
            {
                if (neighbourNodes.Contains(ns))
                {
                    ++numEdgesBetweenNeighbours;
                }
                //else
                //{
                //    Debug.Log("My friends aren't friends... but should they be? " + nb_node.agent.comparePersonality(ns.agent));
                //}
            }
        }

        //float numNeighbours = getNeighbours().Length;
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

    public void removeFromAgent()
    {
        //if (this.agent != null)
        //{
        //    this.agent.setNode(null);
        //}
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

    public void unHighlight()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.white;
        sr.sortingOrder = 0;
    }
}
