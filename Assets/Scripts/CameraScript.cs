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
    private bool dragging = false;

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
        this.cam = Camera.main;
    }

    private void Update()
    {
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            float size = Mathf.Clamp(cam.orthographicSize - Input.GetAxis("Mouse ScrollWheel") * zoomFactor,0.1f,max_size);
            cam.orthographicSize = size;
        }

        if (Input.GetMouseButtonDown(0))
        {
            dragging = true;
            drag_position = cam.ScreenToWorldPoint(Input.mousePosition);
        } else if (Input.GetMouseButtonUp(0))
        {
            dragging = false;
        } else if (Input.GetMouseButton(0))
        {
            Vector3 drag_diff = drag_position - cam.ScreenToWorldPoint(Input.mousePosition);
            drag_diff.z = 0f;
            cam.transform.position += drag_diff;
        }

    }

    public void putCameraWithinBounds()
    {

    }

    public void setMaxSize(float n)
    {
        this.max_size = n;
    }

    public float getMaxSize()
    {
        return this.max_size;
    }
}
