using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    private float DRAG_THRESHOLD = 0.06f;
    private bool dragging_node = false;
    private NodeScript clicked_node;
    private float total_mouse_down_time;
    private Vector3 original_node_position;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

            RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
            if (hit.collider != null)
            {
                /* Check for agent script */
                Agent a = hit.collider.GetComponent<Agent>();
                if (a != null)
                {
                    highlightAgent(a);
                    return;
                }

                /* Check for node script */
                NodeScript ns = hit.collider.GetComponent<NodeScript>();
                if (ns != null)
                {
                    clicked_node = ns;
                    original_node_position = ns.transform.position;
                    dragging_node = true;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging_node = false;
            if (total_mouse_down_time < DRAG_THRESHOLD && clicked_node != null)
            {
                /* If it's a click then reset the clicked node to where it was before we dragged it oops 
                 (Makes the drag more responsive :P)*/
                highlightNode(clicked_node);
                clicked_node.transform.position = original_node_position;
            }
            clicked_node = null;
            total_mouse_down_time = 0f;
        }
        else if (Input.GetMouseButton(0) && dragging_node)
        {
            total_mouse_down_time += Time.deltaTime;
            clicked_node.moveRigidBodyPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        } 
    }

    private void highlightAgent(Agent a)
    {
        Landscape.Instance.setHighlightedAgent(a);
    }

    private void highlightNode(NodeScript ns)
    {
        Landscape.Instance.setHighlightedAgent(ns.getAgent());
    }
}
