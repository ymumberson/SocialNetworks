using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParametersBounds : MonoBehaviour
{
    public static ParametersBounds Instance;

    /* Testing Parameters */
    [SerializeField] public int MIN_NUM_YEARS_TO_RUN = 1;
    [SerializeField] public int MAX_NUM_YEARS_TO_RUN = 501;
    [SerializeField] public int STEP_NUM_YEARS_TO_RUN = 50;

    /* General Simulation Parameters */
    [SerializeField] public int MIN_DAYS_PER_YEAR = 1;
    [SerializeField] public int MAX_DAYS_PER_YEAR = 365;
    [SerializeField] public int STEP_DAYS_PER_YEAR = 1;

    [SerializeField] public int MIN_ENABLE_PERSONALITY_TRANSMISSION = 0;
    [SerializeField] public int MAX_ENABLE_PERSONALITY_TRANSMISSION = 1;

    /* Agent Social Paramaters */
    [SerializeField] public int MIN_NUM_FRIENDS = 0;
    [SerializeField] public int MAX_NUM_FRIENDS = 20;
    [SerializeField] public int STEP_NUM_FRIENDS = 1;

    [SerializeField] public int MIN_PERSONALITY_LENGTH = 1;
    [SerializeField] public int MAX_PERSONALITY_LENGTH = 100;
    [SerializeField] public int STEP_PERSONALITY_LENGTH = 5;

    [SerializeField] public float MIN_PERSONALITY_THRESHOLD = 0f;
    [SerializeField] public float MAX_PERSONALITY_THRESHOLD = 1f;
    [SerializeField] public float STEP_PERSONALITY_THRESHOLD = 0.05f;

    [SerializeField] public float MIN_DAILY_CHANCE_OF_SOCIAL_MEETUP = 0; /* Percentage chance */
    [SerializeField] public float MAX_DAILY_CHANCE_OF_SOCIAL_MEETUP = 1; /* Percentage chance */
    [SerializeField] public float STEP_DAILY_CHANCE_OF_SOCIAL_MEETUP = 0.05f; /* Percentage chance */

    /* Terrain Generation Parameters */
    [SerializeField] public int MIN_SELECTED_MAP_INDEX = 0;
    [SerializeField] public int MAX_SELECTED_MAP_INDEX = 3;
    [SerializeField] public int STEP_SELECTED_MAP_INDEX = 1;

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
}
