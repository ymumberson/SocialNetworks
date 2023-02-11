using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickManager : MonoBehaviour
{
    private bool dragging_node = false;
    private NodeScript clicked_node;

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
                    Landscape.Instance.setHighlightedAgent(a);
                    return;
                }

                /* Check for node script */
                NodeScript ns = hit.collider.GetComponent<NodeScript>();
                if (ns != null)
                {
                    Landscape.Instance.setHighlightedAgent(ns.getAgent());
                    clicked_node = ns;
                    dragging_node = true;
                    return;
                }
            }
        }
        else if (Input.GetMouseButtonUp(0))
        {
            dragging_node = false;
        }
        else if (Input.GetMouseButton(0) && dragging_node)
        {
            clicked_node.moveRigidBodyPosition(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        }
    }
}
