using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public enum Gender { Male, Female }
    public enum AgeState { Child, Adult };

    [SerializeField] private int MAX_AGE;
    [SerializeField] private Agent[] parents;
    [SerializeField] private int age;
    [SerializeField] private AgeState age_state;
    [SerializeField] private Gender gender;
    [SerializeField] private House home;
    [SerializeField] private Building work_school;
    [SerializeField] private int x, y;
    [SerializeField] private bool home_owner;
    [SerializeField] private Agent spouse;
    [SerializeField] private List<Agent> offspring;
    [SerializeField] private string personality;
    private List<Vector2> path;
    //private List<Vector2> path_to_work;

    /* Temporary */
    private bool birthedChild = false;

    /* Internal stuff */
    private Transform transform;

    private void Awake()
    {
        this.transform = GetComponent<Transform>();
    }

    public void initialise_agent(int age, Gender gender, House home, Building work_school, Agent[] parents, bool home_owner, string personality)
    {
        this.age = age;
        this.age_state = (this.age >= Parameters.Instance.AGE_TURN_ADULT) ? AgeState.Adult : AgeState.Child;
        if (this.age_state == AgeState.Adult)
        {
            transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        }
        else
        {
            transform.localScale = new Vector3(0.4f, 0.4f, 0.4f);
        }
        this.gender = gender;
        this.home = home;
        this.work_school = work_school;
        this.parents = parents;
        this.home_owner = home_owner;
        this.spouse = null;
        this.MAX_AGE = Random.Range(Parameters.Instance.MIN_DEATH_AGE, Parameters.Instance.MAX_DEATH_AGE);
        this.offspring = new List<Agent>();
        this.personality = personality;
    }

    public void dailyUpdate()
    {
        if (isAdult())
        {
            if (lookingForSpouse())
            {
                tryPickSpouseFromCoworkers();
                if (spouse != null)
                {
                    Debug.Log("Found a spouse!");
                }
            }
            else if (spouse != null) /* Else if (hasSpouse()) { */
            {
                if (!home_owner)
                {
                    tryFindHouse();
                }
                else
                {
                    tryProduceOffspring();
                }
            }
        }

    }

    public void calculatePathToHome()
    {
        path = Landscape.Instance.pathFind(new Vector2(this.x, this.y), new Vector2(home.x, home.y));
    }

    public void calculatePathToWorkSchool()
    {
        path = Landscape.Instance.pathFind(new Vector2(this.x, this.y), new Vector2(work_school.x, work_school.y));
    }

    public void moveAlongPath()
    {
        if (path != null) // Could change to call !reachedDestination()?
        {
            moveTo(path[path.Count - 1]);
            path.RemoveAt(path.Count - 1);
            if (path.Count == 0) path = null;
        }
    }

    public bool reachedDestination()
    {
        return path == null;
    }

    public void moveTo(Vector2 pos)
    {
        x = (int)pos.x;
        y = (int)pos.y;
        transform.position = pos;
    }

    public House getHome()
    {
        return home;
    }

    public Building getWorkSchool()
    {
        return this.work_school;
    }

    public void setSpouse(Agent a)
    {
        this.spouse = a;
    }

    public Agent getSpouse()
    {
        return this.spouse;
    }

    public int getAge()
    {
        return this.age;
    }

    public bool isAdult()
    {
        return this.age_state == AgeState.Adult;
    }

    public void incrementAge()
    {
        ++age;
        if (age_state == AgeState.Child && age >= Parameters.Instance.AGE_TURN_ADULT)
        {
            becomeAdult();
        }
        if (age >= this.MAX_AGE)
        {
            die();
        }
    }

    public void becomeAdult()
    {
        Debug.Log("I'm an adult! ...time to find a job :(");
        age_state = AgeState.Adult;
        transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
        findJob();
    }

    public void findJob()
    {
        Workplace[] workplaces = Landscape.Instance.getAllWorkplaces();
        this.work_school = workplaces[Random.Range(0, workplaces.Length)];
    }

    public void die()
    {
        home.removeOccupant(this);
        Landscape.Instance.removeAgent(this);
        Debug.Log("Oh no am ded :(");
        Destroy(this.gameObject);
    }

    public bool lookingForSpouse() /* Redefine later */
    {
        return spouse == null && isAdult(); /* ie = single && adult */
    }

    public bool spouseCondition1() /* Very temporary */
    {
        return isAdult();
    }

    public bool matingCondition1() /* Very temporary */
    {
        return home_owner;
    }

    public void tryPickSpouseFromCoworkers()
    {
        List<Agent> coworkers = Landscape.Instance.getCoworkers(work_school);
        if (coworkers.Count != 0)
        {
            Agent selected_coworker = coworkers[Random.Range(0, coworkers.Count - 1)];
            if (selected_coworker.gender != this.gender && selected_coworker.spouse == null)
            {
                this.spouse = selected_coworker;
                selected_coworker.spouse = this;
            }
        }
    }

    public void tryFindHouse() /* TODO: Currently if both parents are dead and still live at home then should set agent to home owner :( */
    {
        House h = Landscape.Instance.getEmptyHouse();
        if (h == null) return; /* Wasn't able to find an empty house */

        Debug.Log("Found a house!");

        home.removeOccupant(this);
        spouse.home.removeOccupant(spouse);

        this.home = h;
        spouse.home = h;

        this.home_owner = true;
        spouse.home_owner = true;

        home.addOccupant(this);
        spouse.home.addOccupant(spouse);

        //Debug.Log("Moved into the house!: " + home.x + "," + home.y);
    }

    public void addOffspring(Agent a)
    {
        offspring.Add(a);
    }

    public void removeOffspring(Agent a)
    {
        offspring.Remove(a);
    }

    public int numOffspring()
    {
        return offspring.Count;
    }

    public void tryProduceOffspring() //TODO need to check if spouse if fertile as well :(
    {
        /* int age, Gender gender, House home, Building work_school, Agent[] parents, bool home_owner */
        if (numOffspring() < Parameters.Instance.MAX_OFFSPRING_AMOUNT
            && inFertilityAgeRange()) /* If room for children and is fertile */
        {
            if (offspringCondition1())
            {
                Agent offspring = Landscape.Instance.instantiateAgent(
                    0,
                    (Random.Range(0, 2) == 0) ? Agent.Gender.Male : Agent.Gender.Female,
                    this.home,
                    Landscape.Instance.getRandomSchool(),
                    new Agent[] { this, this.spouse },
                    false,
                    Landscape.Instance.generateRandomPersonality(this.personality, spouse.personality)
                    );

                this.addOffspring(offspring);
                spouse.addOffspring(offspring);
                Debug.Log("A baby was born!!!!");
            }
        }
    }

    public bool inFertilityAgeRange()
    {
        return (age > Parameters.Instance.MIN_FERTILITY_AGE)
            && (age < Parameters.Instance.MAX_FERTILITY_AGE);
    }

    public bool offspringCondition1()
    {
        return Random.Range(0, 100) < 10;
    }

    public char getPersonalityAtIndex(int index)
    {
        return this.personality[index];
    }

    public string getPersonality()
    {
        return this.personality;
    }
}
