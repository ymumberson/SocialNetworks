using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScript : MonoBehaviour
{
    public static CameraScript Instance;
    [SerializeField] private float zoomFactor = 5f;
    private Camera cam;
    private float max_size = 31f;
    private Vector3 drag_position;
    private bool dragging_camera = false;
    private bool dragging_node = false;
    private Vector2 min_bounds = new Vector2(1,1);
    private Vector2 max_bounds = new Vector2(0,0);

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
        } 
        else
        {
            Instance = this;
        }
    }

    private void Start()
    {
        //this.cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float size = Mathf.Clamp(Camera.main.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * zoomFactor,0.1f,max_size);
            Camera.main.orthographicSize = size;
            putCameraWithinBounds();
        }

        else if (Input.GetMouseButtonDown(1))
        {
            dragging_camera = true;
            drag_position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        } else if (Input.GetMouseButtonUp(1))
        {
            dragging_camera = false;
            putCameraWithinBounds();
        } else if (Input.GetMouseButton(1) && dragging_camera)
        {
            Vector3 drag_diff = drag_position - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            drag_diff.z = 0f;
            Camera.main.transform.position += drag_diff;
            putCameraWithinBounds();
        }
    }

    public void putCameraWithinBounds()
    {
        //float camX = Camera.main.transform.position.x + Camera.main.orthographicSize;
        Vector3 pos = Camera.main.transform.position;
        float size = Camera.main.orthographicSize;

        if (pos.x + size > max_bounds.x)
        {
            pos.x -= (pos.x + size - max_bounds.x);
        }
        if (pos.y + size > max_bounds.y)
        {
            pos.y -= (pos.y + size - max_bounds.y);
        }
        if (pos.y - size < min_bounds.y)
        {
            pos.y += (min_bounds.y - pos.y + size);
        }
        if (pos.x - size < min_bounds.x)
        {
            pos.x += (min_bounds.x - pos.x + size);
        }
        Camera.main.transform.position = pos;
    }

    public void setMaxSize(float n)
    {
        this.max_size = n;
        Vector2 pos = Camera.main.transform.position;
        this.max_bounds = new Vector2(pos.x + n, pos.y + n);
        this.min_bounds = new Vector2(pos.x - n, pos.y - n);
    }

    public float getMaxSize()
    {
        return this.max_size;
    }

    public void setZoomFactor(float zoom)
    {
        this.zoomFactor = zoom;
    }
}
