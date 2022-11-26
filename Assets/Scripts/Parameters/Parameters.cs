using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parameters : MonoBehaviour
{
    public static Parameters Instance;

    /* General Simulation Parameters */
    [SerializeField] public float TURN_TIME = 0.1f;
    [SerializeField] public int DAYS_PER_YEAR = 365;

    /* Agent Parameters */
    [SerializeField] public int AGE_TURN_ADULT = 18;
    [SerializeField] public int MIN_DEATH_AGE = 70;
    [SerializeField] public int MAX_DEATH_AGE = 90;
    [SerializeField] public int MIN_OFFSPRING_AMOUNT = 0;
    [SerializeField] public int MAX_OFFSPRING_AMOUNT = 3;
    [SerializeField] public int MIN_FERTILITY_AGE = 20;
    [SerializeField] public int MAX_FERTILITY_AGE = 50;

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
