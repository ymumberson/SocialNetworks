using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters : MonoBehaviour
{
    public static Parameters Instance;

    /* General Simulation Parameters */
    [SerializeField] public float TURN_TIME = 0.1f;
    [SerializeField] public int DAYS_PER_YEAR = 365;
    [SerializeField] public bool ENABLE_PERSONALITY_TRANSMISSION = false;

    /* Agent Parameters */
    [SerializeField] public int AGE_TURN_ADULT = 18;
    [SerializeField] public int MIN_DEATH_AGE = 70;
    [SerializeField] public int MAX_DEATH_AGE = 90;
    [SerializeField] public int MIN_OFFSPRING_AMOUNT = 0;
    [SerializeField] public int MAX_OFFSPRING_AMOUNT = 3;
    [SerializeField] public int MIN_FERTILITY_AGE = 20;
    [SerializeField] public int MAX_FERTILITY_AGE = 50;

    /* Agent Social Paramaters */
    [SerializeField] public int MIN_FRIENDS = 3;
    [SerializeField] public int MAX_FRIENDS = 7;
    [SerializeField] public int PERSONALITY_LENGTH = 5; /* Probably change to 100 later */
    [SerializeField] public float PERSONALITY_THRESHOLD = 0.5f;
    [SerializeField] public int MAX_NUMBER_OF_SOCIAL_MEETUPS_PER_WEEK = 2;
    [SerializeField] public float DAILY_CHANCE_OF_SOCIAL_MEETUP = 0.1f; /* Percentage chance */

    /* Daily Parameters */
    [SerializeField] public float TIME_AT_WORK_SCHOOL = 1f;
    [SerializeField] public float TIME_AT_HOME = 1f;
    [SerializeField] public float TIME_AT_SOCIAL = 1f;

    /* Initial Pop Gen Parameters */
    [SerializeField] public int MIN_INITIAL_CHILDREN = 0;
    [SerializeField] public int MAX_INITIAL_CHILDREN = 3;
    [SerializeField] public int MIN_INITIAL_PARENT_AGE = 20;
    [SerializeField] public int MAX_INITIAL_PARENT_AGE = 40;
    [SerializeField] public int MIN_INITIAL_CHILD_AGE = 0;
    [SerializeField] public int MAX_INITIAL_CHILD_AGE = 17;

    /* Terrain Generation Parameters */
    [SerializeField] public float PERCENT_CHILD_SOCIAL_BUILDINGS = 0.2f;

    /* Colours for highlighting */
    [SerializeField] public Color HIGHLIGHTED_AGENT_COLOUR = Color.red;
    [SerializeField] public Color HIGHLIGHTED_FRIEND_COLOUR = Color.blue;
    [SerializeField] public Color HIGHLIGHTED_IDEAL_FRIEND_COLOUR = Color.yellow;
    [SerializeField] public Color UNHIGHLIGHTED_AGENT_COLOR = Color.white;
    [SerializeField] public Color HIGHLIGHT_PERFECT_FRIEND = Color.green;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
        } else
        {
            Instance = this;
        }
    }
}
