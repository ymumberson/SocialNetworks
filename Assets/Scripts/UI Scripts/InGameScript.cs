using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InGameScript : MonoBehaviour
{
    /* Zoom */
    [SerializeField] private float zoomFactor = 1f;
    
    /* InGame UIs */
    [SerializeField] GameObject inGameUI;
    [SerializeField] GraphUIScript graphUI;
    
    /* Search UI */
    [SerializeField] private TextMeshProUGUI SEARCH_AGENT_ID_TEXT;

    /* Time Buttons */
    [SerializeField] private Button PAUSE_BUTTON;
    [SerializeField] private Button NORMAL_SPEED_BUTTON;
    [SerializeField] private Button HALF_SPEED_BUTTON;
    [SerializeField] private Button DOUBLE_SPEED_BUTTON;
    [SerializeField] private Button QUADRUPLE_SPEED_BUTTON;

    /* Selected Agent */
    [SerializeField] private TextMeshProUGUI AGENT_ID_TEXT;
    [SerializeField] private TextMeshProUGUI AGE_TEXT;
    [SerializeField] private TextMeshProUGUI GENDER_TEXT;
    [SerializeField] private TextMeshProUGUI HOME_OWNER_TEXT;
    [SerializeField] private TextMeshProUGUI NUM_CHILDREN_TEXT;
    [SerializeField] private TextMeshProUGUI NUM_FRIENDS_TEXT;
    [SerializeField] private TextMeshProUGUI FRIENDS_TEXT;

    /* Agent's Node */
    [SerializeField] private TextMeshProUGUI CONNECTIVITY_TEXT;
    [SerializeField] private TextMeshProUGUI CLUSTERING_TEXT;
    [SerializeField] private TextMeshProUGUI AVG_PATH_LENGTH_TEXT;
    [SerializeField] private TextMeshProUGUI MAX_DEPTH_TEXT;

    /* General Debug UI */
    /* Landscape */
    [SerializeField] private TextMeshProUGUI DAY_TEXT;
    [SerializeField] private TextMeshProUGUI YEAR_TEXT;
    [SerializeField] private TextMeshProUGUI TIME_TEXT;
    [SerializeField] private TextMeshProUGUI NUMBER_ADULTS_TEXT;
    [SerializeField] private TextMeshProUGUI NUMBER_CHILDREN_TEXT;
    /* Graph Renderer */
    [SerializeField] private TextMeshProUGUI GR_DENSITY_TEXT;
    [SerializeField] private TextMeshProUGUI GR_CONNECTIVITY_TEXT;
    [SerializeField] private TextMeshProUGUI GR_CLUSTERING_TEXT;
    [SerializeField] private TextMeshProUGUI GR_PATH_LENGTH_TEXT;
    [SerializeField] private TextMeshProUGUI GR_MAX_DEPTH_TEXT;
    [SerializeField] private TextMeshProUGUI GR_AVG_DEPTH_TEXT;
    [SerializeField] private TextMeshProUGUI GR_CAN_REACH_ALL_TEXT;

    private Button selected_button;
    private float camera_height;
    private Vector3 camera_position;

    private void Awake()
    {
        Time.timeScale = 1f;
        selectButton(NORMAL_SPEED_BUTTON);
    }

    //private void Start()
    //{
    //    /* Centre the camera */
    //    float tileWidth = 1f;
    //    this.camera_height = tileWidth * Landscape.Instance.getHeight();
    //    this.camera_position = new Vector3(tileWidth * Landscape.Instance.getWidth() / 2f, camera_height / 2f, -10f);
    //    Camera.main.transform.position = camera_position;
    //    Camera.main.orthographicSize = (camera_height / 2f) * 1.1f;
    //    CameraScript.Instance.setMaxSize((camera_height / 2f) * 1.1f);
    //}

    public void calculateCameraBounds(float width, float height)
    {
        float tileWidth = 1f;
        this.camera_height = tileWidth * height;
        this.camera_position = new Vector3(tileWidth * width / 2f, camera_height / 2f, -10f);
    }

    public void hideGraphUI()
    {
        graphUI.hideGraphUI();
    }

    public void switchToGraphUI()
    {
        this.hideUI();
        graphUI.showGraphUI();
    }

    public void hideUI()
    {
        this.inGameUI.SetActive(false);
    }

    public void showUI()
    {
        this.inGameUI.SetActive(true);
        Camera.main.transform.position = camera_position;
        Camera.main.orthographicSize = (camera_height / 2f) * 1.2f;
        CameraScript.Instance.setMaxSize((camera_height / 2f) * 1.2f);
        CameraScript.Instance.setZoomFactor(this.zoomFactor);
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        selectButton(PAUSE_BUTTON);
    }

    public void setSpeedHalf()
    {
        Time.timeScale = 0.5f;
        selectButton(HALF_SPEED_BUTTON);
    }

    public void SetSpeedNormal()
    {
        Time.timeScale = 1f;
        selectButton(NORMAL_SPEED_BUTTON);
    }

    public void SetSpeedDouble()
    {
        Time.timeScale = 2f;
        selectButton(DOUBLE_SPEED_BUTTON);
    }

    public void SetSpeedQuadruple()
    {
        Time.timeScale = 4f;
        selectButton(QUADRUPLE_SPEED_BUTTON);
    }

    private void selectButton(Button b)
    {
        if (selected_button)
        {
            selected_button.interactable = true;
        }
        selected_button = b;
        selected_button.interactable = false;
    }

    public void updateSelectedAgentText(Agent a)
    {
        AGENT_ID_TEXT.text = "ID: " + a.getAgentID();
        AGE_TEXT.text = "Age: " + a.getAge() + "/" + a.getMaxAge();
        GENDER_TEXT.text = "Gender: " + a.getGenderString();
        HOME_OWNER_TEXT.text = (a.isHomeOwner()) ? "Home Owner: Yes" : "Home Owner: No";
        NUM_CHILDREN_TEXT.text = "#Children: " + a.numOffspring() + "/" + a.getMaxNumOffspring();
        NUM_FRIENDS_TEXT.text = "#Friends: " + a.getNumFriends() + "/" + a.getMaxNumFriends();
        string friends_string = "Friends: ";
        Agent[] friends = a.getFriends();
        if (friends.Length >= 1)
        {
            friends_string += friends[0].getAgentID() + "(" + a.comparePersonality(friends[0]) + ")";
            for (int i=1; i<friends.Length; ++i)
            {
                friends_string += ", " + friends[i].getAgentID() + "(" + a.comparePersonality(friends[i]) + ")";
            }
        }
        FRIENDS_TEXT.text = friends_string;
        if (a.getNode() != null)
        {
            updateNodeText(a.getNode());
        }
    }

    public void resetSelectedAgentText()
    {
        AGENT_ID_TEXT.text = "ID:";
        AGE_TEXT.text = "Age:";
        GENDER_TEXT.text = "Gender:";
        HOME_OWNER_TEXT.text = "Home Owner:";
        NUM_CHILDREN_TEXT.text = "#Children:";
        NUM_FRIENDS_TEXT.text = "#Friends:";
        FRIENDS_TEXT.text = "Friends:";
    }

    public void updateNodeText(NodeScript ns)
    {
        CONNECTIVITY_TEXT.text = "Connectivity: " + ns.getDegreeOfConnectivity();
        CLUSTERING_TEXT.text = "Clustering: " + ns.getClusteringCoefficient();
        AVG_PATH_LENGTH_TEXT.text = "Avg Path: " + ns.getAvgPathLength();
        MAX_DEPTH_TEXT.text = "Max Depth: " + ns.getMaxDepth();
    }

    public void resetNodeText()
    {
        CONNECTIVITY_TEXT.text = "Connectivity:";
        CLUSTERING_TEXT.text = "Clustering:";
        AVG_PATH_LENGTH_TEXT.text = "Avg Path:";
        MAX_DEPTH_TEXT.text = "Max Depth:";
    }

    public void selectAgent()
    {
        try
        {
            //int id = int.Parse(SEARCH_AGENT_ID_TEXT.text, System.Globalization.NumberStyles.AllowLeadingWhite | System.Globalization.NumberStyles.AllowTrailingWhite);
            //selectAgent(id);
            string s = SEARCH_AGENT_ID_TEXT.text;
            string temp = "";
            for (int i=0; i<s.Length-1; ++i)
            {
                temp += s[i];
            }
            int id = System.Convert.ToInt32(temp);
            selectAgent(id);
        } catch (System.Exception e)
        {
            Debug.LogError("Invalid AgentID for the search :(");
        }
    }

    public void selectAgent(int agentID)
    {
        Landscape.Instance.setHighlightedAgent(agentID);
    }

    public void updateLandscapeText(Landscape l)
    {
        DAY_TEXT.text = "Day: " + l.getDay();
        YEAR_TEXT.text = "Year: " + l.getYear();
        TIME_TEXT.text = "Time: " + l.getTimeString();
        NUMBER_ADULTS_TEXT.text = "#Adults: " + l.getNumAdults();
        NUMBER_CHILDREN_TEXT.text = "#Children: " + l.getNumChildren();
    }

    public void updateGraphRendererText(GraphRendererScript gr)
    {
        GR_DENSITY_TEXT.text = "Density: " + gr.getDensity();
        GR_CONNECTIVITY_TEXT.text = "Connectivity: " + gr.getAverageConnectivity();
        GR_CLUSTERING_TEXT.text = "Clustering: " + gr.getAverageClusteringCoefficient();
        GR_PATH_LENGTH_TEXT.text = "Avg Path: " + gr.getAveragePathLength();
        GR_MAX_DEPTH_TEXT.text = "Max Depth: " + gr.getMaxDepth();
        GR_AVG_DEPTH_TEXT.text = "Avg Depth: " + gr.getAverageDepth();
        GR_CAN_REACH_ALL_TEXT.text = "Can Reach All: " + gr.getPercentCanReachAll();
    }
}
