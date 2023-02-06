using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphUIScript : MonoBehaviour
{
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
        Camera.main.orthographicSize = (camera_height / 2f) * 1.1f;
        graphRenderer.enableVisuals(true);
        CameraScript.Instance.setMaxSize(((camera_height / 2f) * 1.1f));
    }

    public void hideGraphUI()
    {
        graphUI.SetActive(false);
        graphRenderer.enableVisuals(false);
    }

    public void switchToGraphUI()
    {
        this.hideGraphUI();
        inGameUI.showUI();
    }
}
