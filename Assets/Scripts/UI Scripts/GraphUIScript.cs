using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphUIScript : MonoBehaviour
{
    [SerializeField] private float zoomFactor = 1f;
    [SerializeField] private GameObject graphUI;
    [SerializeField] private GraphRendererScript graphRenderer;
    [SerializeField] private InGameScript inGameUI;

    private float camera_height;
    private Vector3 camera_position;

    private void Start()
    {
        Vector2 temp = graphRenderer.getCentre();
        this.camera_position = new Vector3(temp.x, temp.y, -10);
        this.camera_height = Mathf.Abs(graphRenderer.getLower().y - graphRenderer.getUpper().y);
    }

    public void showGraphUI()
    {
        graphUI.SetActive(true);
        Camera.main.transform.position = camera_position;
        Camera.main.orthographicSize = (camera_height / 2f) * 1.2f;
        graphRenderer.enableVisuals(true);
        Landscape.Instance.disableDayLoop();
        CameraScript.Instance.setMaxSize(((camera_height / 2f) * 1.2f));
        CameraScript.Instance.setZoomFactor(this.zoomFactor);
    }

    public void hideGraphUI()
    {
        graphUI.SetActive(false);
        graphRenderer.enableVisuals(false);
        Landscape.Instance.enableDayLoop();
    }

    public void switchToGraphUI()
    {
        this.hideGraphUI();
        inGameUI.showUI();
    }
}
