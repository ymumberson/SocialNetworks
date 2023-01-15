using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphRendererScript : MonoBehaviour
{
    [SerializeField] private GameObject NODE_TEMPLATE;
    [SerializeField] private GameObject EDGE_TEMPLATE;
    [SerializeField] private float minX=0, maxX=-50, minY=0, maxY=-50;
    [SerializeField] private List<NodeScript> nodeList;
    [SerializeField] private int numNodes;
    [SerializeField] private int numEdges;
    [SerializeField] private float density;
    [SerializeField] private float avgConnectivity;

    /* For moving nodes */
    private float l = 1f;

    private void Awake()
    {
        nodeList = new List<NodeScript>();
        numEdges = 0;
        numNodes = 0;
        density = 0;
    }

    public Vector2 randomBoundedVector2()
    {
        return new Vector2(Random.Range(minX,maxX), Random.Range(minY, maxY));
    }

    public void addAgent(Agent a)
    {
        /* Instantiate node template and set values */
        NodeScript ns = Instantiate(NODE_TEMPLATE).GetComponent<NodeScript>();
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
                Vector3 other_pos = other_node.getPosition();
                if (node.hasNeighbour(other_node.agent))
                {
                    //Debug.Log(attractiveForce(node,other_node));
                    //node.moveBy(Vector3.MoveTowards(node_pos, other_pos, 1f).normalized);
                    //Debug.Log(Vector3.MoveTowards(node_pos, other_pos, 1f).normalized);
                    //Debug.Log(node_pos + " | " + (other_pos - node_pos).normalized);
                    node.moveBy((other_pos - node_pos).normalized * attractiveForce(node, other_node));

                }
                else
                {
                    //Debug.Log(repulsiveForce(node, other_node));
                    //node.moveBy(Vector3.MoveTowards(other_pos, node_pos, 1f).normalized);
                    //Debug.Log("Not neighbour");
                    node.moveBy((node_pos - other_pos).normalized * repulsiveForce(node, other_node));
                }
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

    public float attractiveForce(NodeScript u, NodeScript v)
    {
        Vector3 pU = u.getPosition();
        Vector3 pV = v.getPosition();
        float length = Vector3.Distance(pU, pV);

        return (l * l) / (length);
        //return (length * length) / l;
    }

    public float repulsiveForce(NodeScript u, NodeScript v)
    {
        Vector3 pU = u.getPosition();
        Vector3 pV = v.getPosition();
        float length = Vector3.Distance(pU, pV);

        //return (length*length) / l;
       return ((l*l) / (length*numNodes));
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
}
