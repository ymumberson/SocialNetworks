using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InGameScript : MonoBehaviour
{

    [SerializeField] private Button PAUSE_BUTTON;
    [SerializeField] private Button NORMAL_SPEED_BUTTON;
    [SerializeField] private Button HALF_SPEED_BUTTON;
    [SerializeField] private Button DOUBLE_SPEED_BUTTON;
    [SerializeField] private Button QUADRUPLE_SPEED_BUTTON;
    [SerializeField] private TextMeshProUGUI AGENT_ID_TEXT;
    [SerializeField] private TextMeshProUGUI AGE_TEXT;
    [SerializeField] private TextMeshProUGUI GENDER_TEXT;
    [SerializeField] private TextMeshProUGUI HOME_OWNER_TEXT;
    [SerializeField] private TextMeshProUGUI NUM_CHILDREN_TEXT;
    [SerializeField] private TextMeshProUGUI NUM_FRIENDS_TEXT;
    [SerializeField] private TextMeshProUGUI FRIENDS_TEXT;
    [SerializeField] private TextMeshProUGUI CONNECTIVITY_TEXT;
    [SerializeField] private TextMeshProUGUI CLUSTERING_TEXT;
    [SerializeField] private TextMeshProUGUI AVG_PATH_LENGTH_TEXT;
    [SerializeField] private TextMeshProUGUI MAX_DEPTH_TEXT;

    private Button selected_button;


    private void Awake()
    {
        Time.timeScale = 1f;
        selectButton(NORMAL_SPEED_BUTTON);
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
            friends_string += friends[0].getAgentID();
            for (int i=1; i<friends.Length; ++i)
            {
                friends_string += ", " + friends[i].getAgentID();
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
        CONNECTIVITY_TEXT.text = "Connectivity:" + ns.getDegreeOfConnectivity();
        CLUSTERING_TEXT.text = "Clustering:" + ns.getClusteringCoefficient();
        AVG_PATH_LENGTH_TEXT.text = "Avg Path:" + ns.getAvgPathLength();
        MAX_DEPTH_TEXT.text = "Max Depth:" + ns.getMaxDepth();
    }

    public void resetNodeText()
    {
        CONNECTIVITY_TEXT.text = "Connectivity:";
        CLUSTERING_TEXT.text = "Clustering:";
        AVG_PATH_LENGTH_TEXT.text = "Avg Path:";
        MAX_DEPTH_TEXT.text = "Max Depth:";
    }
}
