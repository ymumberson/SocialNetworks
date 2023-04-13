using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters : MonoBehaviour
{
    public static Parameters Instance;

    /* Testing Parameters */
    [SerializeField] public int NUM_YEARS_TO_RUN = 100;
    [SerializeField] public bool DISABLE_PATHFINDING = false;
    [SerializeField] public string TEST_NAME = "1";
    [SerializeField] public bool IS_SEEDED = true;
    [SerializeField] public int SEED = 123456789;
    
    /* General Simulation Parameters */
    [SerializeField] public float TURN_TIME = 0.1f;
    [SerializeField] public int DAYS_PER_YEAR = 1;
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
    [SerializeField] public int SELECTED_MAP_INDEX = 0;
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
            print("Destroying duplicate instance of Parameters.");
            Destroy(this.gameObject);
        } else
        {
            Instance = this;
        }
    }

    public string titlesCSV()
    {
        return
        "NUM_YEARS_TO_RUN" + "," +
        "DISABLE_PATHFINDING" + "," +
        "TEST_NAME" + "," +
        "IS_SEEDED" + "," +
        "SEED" + "," +
        "TURN_TIME" + "," +
        "DAYS_PER_YEAR" + "," +
        "ENABLE_PERSONALITY_TRANSMISSION" + "," +
        "AGE_TURN_ADULT" + "," +
        "MIN_DEATH_AGE" + "," +
        "MAX_DEATH_AGE" + "," +
        "MIN_OFFSPRING_AMOUNT" + "," +
        "MAX_OFFSPRING_AMOUNT" + "," +
        "MIN_FERTILITY_AGE" + "," +
        "MAX_FERTILITY_AGE" + "," +
        "MIN_FRIENDS" + "," +
        "MAX_FRIENDS" + "," +
        "PERSONALITY_LENGTH" + "," +
        "PERSONALITY_THRESHOLD" + "," +
        "MAX_NUMBER_OF_SOCIAL_MEETUPS_PER_WEEK" + "," +
        "DAILY_CHANCE_OF_SOCIAL_MEETUP" + "," +
        "TIME_AT_WORK_SCHOOL" + "," +
        "TIME_AT_HOME" + "," +
        "TIME_AT_SOCIAL" + "," +
        "MIN_INITIAL_CHILDREN" + "," +
        "MAX_INITIAL_CHILDREN" + "," +
        "MIN_INITIAL_PARENT_AGE" + "," +
        "MAX_INITIAL_PARENT_AGE" + "," +
        "MIN_INITIAL_CHILD_AGE" + "," +
        "MAX_INITIAL_CHILD_AGE" + "," +
        "SELECTED_MAP_INDEX" + "," +
        "PERCENT_CHILD_SOCIAL_BUILDINGS" + ",";
    }

    public string toCSV()
    {
        return
        NUM_YEARS_TO_RUN + "," +
        DISABLE_PATHFINDING + "," +
        TEST_NAME + "," +
        IS_SEEDED + "," +
        SEED + "," +
        TURN_TIME + "," +
        DAYS_PER_YEAR + "," +
        ENABLE_PERSONALITY_TRANSMISSION + "," +
        AGE_TURN_ADULT + "," +
        MIN_DEATH_AGE + "," +
        MAX_DEATH_AGE + "," +
        MIN_OFFSPRING_AMOUNT + "," +
        MAX_OFFSPRING_AMOUNT + "," +
        MIN_FERTILITY_AGE + "," +
        MAX_FERTILITY_AGE + "," +
        MIN_FRIENDS + "," +
        MAX_FRIENDS + "," +
        PERSONALITY_LENGTH + "," +
        PERSONALITY_THRESHOLD + "," +
        MAX_NUMBER_OF_SOCIAL_MEETUPS_PER_WEEK + "," +
        DAILY_CHANCE_OF_SOCIAL_MEETUP + "," +
        TIME_AT_WORK_SCHOOL + "," +
        TIME_AT_HOME + "," +
        TIME_AT_SOCIAL + "," +
        MIN_INITIAL_CHILDREN + "," +
        MAX_INITIAL_CHILDREN + "," +
        MIN_INITIAL_PARENT_AGE + "," +
        MAX_INITIAL_PARENT_AGE + "," +
        MIN_INITIAL_CHILD_AGE + "," +
        MAX_INITIAL_CHILD_AGE + "," +
        SELECTED_MAP_INDEX + "," +
        PERCENT_CHILD_SOCIAL_BUILDINGS + ",";
    }

    public void randomize()
    {
        this.NUM_YEARS_TO_RUN = Random.Range(ParametersBounds.Instance.MIN_NUM_YEARS_TO_RUN, ParametersBounds.Instance.MAX_NUM_YEARS_TO_RUN);
        this.ENABLE_PERSONALITY_TRANSMISSION = Random.value >= 0.5;
        this.MIN_FRIENDS = Random.Range(ParametersBounds.Instance.MIN_NUM_FRIENDS, ParametersBounds.Instance.MAX_NUM_FRIENDS);
        this.MAX_FRIENDS = Random.Range(this.MIN_FRIENDS, ParametersBounds.Instance.MAX_NUM_FRIENDS);
        this.PERSONALITY_LENGTH = Random.Range(ParametersBounds.Instance.MIN_PERSONALITY_LENGTH, ParametersBounds.Instance.MAX_PERSONALITY_LENGTH);
        this.PERSONALITY_THRESHOLD = Random.Range(ParametersBounds.Instance.MIN_PERSONALITY_THRESHOLD, ParametersBounds.Instance.MAX_PERSONALITY_THRESHOLD);
        this.DAILY_CHANCE_OF_SOCIAL_MEETUP = Random.Range(ParametersBounds.Instance.MIN_DAILY_CHANCE_OF_SOCIAL_MEETUP, ParametersBounds.Instance.MAX_DAILY_CHANCE_OF_SOCIAL_MEETUP);
        this.SELECTED_MAP_INDEX = Random.Range(ParametersBounds.Instance.MIN_SELECTED_MAP_INDEX, ParametersBounds.Instance.MAX_SELECTED_MAP_INDEX+1);
    }
}
