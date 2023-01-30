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
    [SerializeField] private TextMeshProUGUI AGE_TEXT;
    [SerializeField] private TextMeshProUGUI GENDER_TEXT;
    [SerializeField] private TextMeshProUGUI HOME_OWNER_TEXT;
    [SerializeField] private TextMeshProUGUI NUM_CHILDREN_TEXT;
    [SerializeField] private TextMeshProUGUI NUM_FRIENDS_TEXT;

    private Button selected_button;


    private void Awake()
    {
        Time.timeScale = 1f;
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
        AGE_TEXT.text = "Age: " + a.getAge() + "/" + a.getMaxAge();
        GENDER_TEXT.text = "Gender: " + a.getGenderString();
        HOME_OWNER_TEXT.text = (a.isHomeOwner()) ? "Home Owner: Yes" : "Home Owner: No";
        NUM_CHILDREN_TEXT.text = "#Children: " + a.numOffspring() + "/" + a.getMaxNumOffspring();
        NUM_FRIENDS_TEXT.text = "#Friends: " + a.getNumFriends() + "/" + a.getMaxNumFriends();
    }

    public void resetSelectedAgentText()
    {
        AGE_TEXT.text = "Age:";
        GENDER_TEXT.text = "Gender:";
        HOME_OWNER_TEXT.text = "Home Owner:";
        NUM_CHILDREN_TEXT.text = "#Children:";
        NUM_FRIENDS_TEXT.text = "#Friends:";
    }
}
