using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeScript : MonoBehaviour
{
    private Transform transform;
    public Agent agent;
    private List<LineRenderer> edges;
    [SerializeField] private int degreeOfConnectivity;
    private Rigidbody2D rb;

    private void Awake()
    {
        this.transform = GetComponent<Transform>();
        this.edges = new List<LineRenderer>();
        this.degreeOfConnectivity = 0;
        rb = GetComponent<Rigidbody2D>();
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
        rb.AddForce(pos*100);
        //Debug.Log("Adding force " + (pos*100));
    }

    public Agent[] getNeighbours()
    {
        return agent.getFriends();
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
}
