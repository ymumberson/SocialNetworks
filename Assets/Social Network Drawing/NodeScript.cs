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
    private List<NodeScript> reachableInN;

    /* Just keeping these here for debugging reasons */
    [SerializeField] private int community_id;
    private Color community_color;

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
        this.agent.setNode(this);
        this.gameObject.name = "Node: " + a.getAgentID();
    }

    public void setReachableInN(List<NodeScript> ls)
    {
        this.reachableInN = ls;
    }

    public List<NodeScript> getReachableInN()
    {
        return this.reachableInN;
    }

    public void setCommunity(int id, Color c)
    {
        this.community_id = id;
        this.community_color = c;
        this.agent.setCommunity(id, c);
    }

    public int getCommunityID()
    {
        return this.agent.getCommunityID();
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
        float previous_clustering_coefficient = this.clusteringCoefficient;
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
            foreach(NodeScript nb_node2 in neighbourNodes)
            {
                if (nb_node == nb_node2) continue;
                if (neighbour_neighbourNodes.Contains(nb_node2))
                {
                    ++numEdgesBetweenNeighbours;
                }
                else
                {
                    //Debug.Log(nb_node.agent.getAgentID() + " and " + nb_node2.agent.getAgentID() + " are not friends.");
                    //if (nb_node.agent.comparePersonality(nb_node2.agent) > nb_node.agent.getWorstFriendValue()
                    //    && nb_node.agent.hasEncountered(nb_node2.agent))
                    //{
                    //    Debug.Log("Agent " + nb_node.agent.getAgentID() + " and agent " + nb_node2.agent.getAgentID()
                    //        + " aren't friends but they should be!");
                    //}

                    //float score = nb_node.agent.comparePersonality(nb_node2.agent);
                    //float worst_score = nb_node.agent.getWorstFriendValue();
                    //if (score > worst_score)
                    //{
                    //    if (nb_node.agent.hasEncountered(nb_node2.agent))
                    //    {
                    //        Debug.Log("Agent " + nb_node.agent.getAgentID() + " and agent " + nb_node2.agent.getAgentID()
                    //        + " aren't friends but they should be! (" + score + " > " + worst_score + ")");
                    //    }
                    //    else
                    //    {
                    //        Debug.Log("Agent " + nb_node.agent.getAgentID() + " and agent " + nb_node2.agent.getAgentID()
                    //        + " would be great friends, but they haven't met! (" + score + " > " + worst_score + ")");
                    //    }
                    //}
                    //else
                    //{
                    //    //Debug.Log("Agent " + nb_node.agent.getAgentID() + " and agent " + nb_node2.agent.getAgentID()
                    //    //    + " wouldn't make good friends! (" + score + " <= " + worst_score + ")");
                    //    Debug.Log("Wouldn't make good friends.");
                    //}
                }
            }
            //foreach (NodeScript ns in neighbour_neighbourNodes)
            //{
            //    if (neighbourNodes.Contains(ns))
            //    {
            //        ++numEdgesBetweenNeighbours;
            //    }
            //    else
            //    {
            //        Debug.Log(nb_node.agent.getAgentID() + "|" + ns.agent.getAgentID() + "|" + " My friends aren't friends... but should they be? " + nb_node.agent.comparePersonality(ns.agent)
            //            + ". Has encountered? " + nb_node.agent.hasEncountered(ns.agent)
            //            + ". My min value: " + nb_node.agent.getWorstFriendValue());
            //        Debug.Log(nb_node.agent.getAgentID() + "|" + ns.agent.getAgentID() + "|" +
            //            "nb_node's friends: " + nb_node.agent.getFriendIDs() + "| ns's friends: " + ns.agent.getFriendIDs());
            //        Debug.Log(nb_node.agent.getAgentID() + "|" + ns.agent.getAgentID() + "|" +
            //            "nb_node's scores: " + nb_node.agent.getCloseFriendsString() + "| ns's scores: " + ns.agent.getCloseFriendsString());
            //    }
            //}
        }

        //float numNeighbours = getNeighbours().Length;
        float numNeighbours = neighbourNodes.Count;
        float maxPossibleEdges = numNeighbours * (numNeighbours - 1);

        this.clusteringCoefficient = (numEdgesBetweenNeighbours) / (maxPossibleEdges);
        //Debug.Log(this.agent.getAgentID() + "|I have " + numNeighbours + " friends, and " + numEdgesBetweenNeighbours + 
        //    " consider each other friends (One-way). This gives a coefficient of " + this.clusteringCoefficient);

        //if (this.clusteringCoefficient < previous_clustering_coefficient)
        //{
        //    Debug.Log("AgentID: " + this.agent.getAgentID() + ", Clustering Coefficient has decreased from " + previous_clustering_coefficient
        //        + " to " + this.clusteringCoefficient);
        //    if (edges.Count < neighbourNodes.Count)
        //    {
        //        Debug.Log("AgentID: " + this.agent.getAgentID() + ", but maybe it's because I went from " + edges.Count
        //            + " friends to " + neighbourNodes.Count + " friends?");
        //    }
        //    else
        //    {
        //        Debug.Log("AgentID: " + this.agent.getAgentID() + ", maybe it's the heap? " + this.agent.getCloseFriendsString());
        //    }
        //}
    }

    public void calculateClusteringCoefficient2()
    {
        float previous_clustering_coefficient = this.clusteringCoefficient;
        /* Loop through each neighbour, and for each neighbour count 
         * the edges with other neighbours of THIS node */
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
            foreach (NodeScript nb_node2 in neighbourNodes)
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

    public void highlight()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Parameters.Instance.HIGHLIGHTED_AGENT_COLOUR;
        sr.sortingOrder = 2;
    }

    public void highlightAsFriend()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Parameters.Instance.HIGHLIGHTED_FRIEND_COLOUR;
        sr.sortingOrder = 1;
    }

    public void unHighlight()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Parameters.Instance.UNHIGHLIGHTED_AGENT_COLOR;
        sr.sortingOrder = 1;
    }

    public void highlightCommunity()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = this.community_color;
        sr.sortingOrder = 0;
    }
}
