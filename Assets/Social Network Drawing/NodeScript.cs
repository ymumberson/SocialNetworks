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
    public Agent agent;
    private List<LineRenderer> edges;
    [SerializeField] private int degreeOfConnectivity;
    private Rigidbody2D rb;
    private Vector2 lowerBound;
    private Vector2 upperBound;

    /// <summary>
    /// This stores the value of clustering coefficient from when it was last calculated.
    /// </summary>
    [SerializeField] private float clusteringCoefficient;

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
        return Landscape.Instance.getGraphRenderer().getNodes(getNeighbours());
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
        foreach (NodeScript nb_node in neighbourNodes)
        {
            List<NodeScript> neighbour_neighbourNodes = nb_node.getNeighbourNodes();
            foreach (NodeScript ns in neighbour_neighbourNodes)
            {
                if (neighbourNodes.Contains(ns))
                {
                    ++numEdgesBetweenNeighbours;
                }
            }
        }

        float numNeighbours = getNeighbours().Length;
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
}
