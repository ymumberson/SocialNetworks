using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Agent : MonoBehaviour
{
    public enum Gender { Male, Female }
    public enum AgeState { Child, Adult };

    private static int LAST_AGENT_ID = 0;

    [SerializeField] private int AGENT_ID;
    [SerializeField] private int MAX_AGE;
    [SerializeField] private int MAX_NUM_OFFSPRING;
    [SerializeField] private NodeScript node;
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
    [SerializeField] private int num_social_meetups;
    [SerializeField] private bool is_attending_social_meetup_today;
    [SerializeField] private int total_num_social_meetups_attended;
    [SerializeField] private Social social_meetup_building;
    [SerializeField] private int social_group_index;
    private List<Vector2> path;
    //private List<Vector2> path_to_work;

    /* Temporary */
    private bool birthedChild = false;

    /* Internal stuff */
    private Transform transform;


    /* Friendship Stuff */
    [SerializeField] private int MAX_NUM_FRIENDS;
    [SerializeField] MinHeap close_friends;

    private void Awake()
    {
        this.transform = GetComponent<Transform>();
        this.AGENT_ID = Agent.LAST_AGENT_ID++;
        this.gameObject.name = "Agent: " + this.AGENT_ID;
        this.total_num_social_meetups_attended = 0;
    }

    public int getAgentID()
    {
        return this.AGENT_ID;
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
        this.MAX_NUM_OFFSPRING = Parameters.Instance.MAX_OFFSPRING_AMOUNT;
        this.offspring = new List<Agent>();
        this.personality = personality;
        this.MAX_NUM_FRIENDS = Random.Range(Parameters.Instance.MIN_FRIENDS, Parameters.Instance.MAX_FRIENDS);
        this.close_friends = new MinHeap(this, MAX_NUM_FRIENDS);
        this.num_social_meetups = 0;
        this.is_attending_social_meetup_today = false;

        this.moveTo(new Vector2(home.x, home.y));
    }

    public void dailyUpdate()
    {
        if (isAdult())
        {
            if (lookingForSpouse())
            {
                tryPickSpouseFromCoworkers();
                //if (spouse != null)
                //{
                //    Debug.Log("Found a spouse!");
                //}
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

    public void calculatePathToSocial()
    {
        path = Landscape.Instance.pathFind(new Vector2(this.x, this.y), new Vector2(social_meetup_building.x, social_meetup_building.y));
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

    public void teleportToDestination()
    {
        if (path != null && path.Count >= 0)
        {
            moveTo(path[0]);
            path.Clear();
            path = null;
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

    public Gender getGender()
    {
        return this.gender;
    }

    public string getGenderString()
    {
        if (this.gender == Gender.Male)
        {
            return "Male";
        } else
        {
            return "Female";
        }
    }

    public int getAge()
    {
        return this.age;
    }

    public int getMaxAge()
    {
        return this.MAX_AGE;
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
        //Debug.Log("I'm an adult! ...time to find a job :(");
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
        //Debug.Log("Oh no am ded :(");
        //Destroy(this.gameObject);
        (this.gameObject).SetActive(false);
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
        List<Agent> coworkers = Landscape.Instance.getCoworkers(work_school,this);
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

    public bool isHomeOwner()
    {
        return this.home_owner;
    }

    public void tryFindHouse() /* TODO: Currently if both parents are dead and still live at home then should set agent to home owner :( */
    {
        House h = Landscape.Instance.getEmptyHouse();
        if (h == null) return; /* Wasn't able to find an empty house */

        //Debug.Log("Found a house!");

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

    public int getMaxNumOffspring()
    {
        return this.MAX_NUM_OFFSPRING;
    }

    public void tryProduceOffspring() //TODO need to check if spouse if fertile as well :(
    {
        /* int age, Gender gender, House home, Building work_school, Agent[] parents, bool home_owner */
        if (numOffspring() < this.MAX_NUM_OFFSPRING
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
                //Debug.Log("A baby was born!!!!");
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

    public float comparePersonality(Agent other)
    {
        return this.comparePersonality(other.getPersonality());
    }

    public float comparePersonality(string other_personality) /* My cost function */
    {
        float common_features = 0;
        float len = Parameters.Instance.PERSONALITY_LENGTH;
        for (int i=0; i< len; ++i)
        {
            if (this.personality[i] == other_personality[i])
            {
                ++common_features;
            }
        }
        return common_features / len;
    }

    public void debug_printFriends()
    {
        this.close_friends.printHeap();
    }

    public bool tryAddFriend(Agent a)
    {
        return this.close_friends.insert(a);
    }

    public Agent[] getFriends()
    {
        return close_friends.getAgents();
    }

    public int getNumFriends()
    {
        return this.getFriends().Length;
    }

    public int getMaxNumFriends()
    {
        return this.MAX_NUM_FRIENDS;
    }

    public bool hasFriends()
    {
        return !close_friends.isEmpty();
    }

    public void setNode(NodeScript ns)
    {
        this.node = ns;
    }

    public NodeScript getNode()
    {
        return this.node;
    }

    public Vector2 getNodePosition()
    {
        if (node == null) Debug.Log("Node is null dummy");
        if (node == null)
        {
            return Vector2.positiveInfinity;
        }
        return node.getPosition();
    }

    public bool canAttendSocialMeetup()
    {
        return this.numSocialMeetupsBelowThreshold() && !isAttendingSocialMeetupToday();
    }

    public bool canAttendSocialMeetup(Social s)
    {
        return this.canAttendSocialMeetup() && s.forAdults() == this.isAdult();
    }

    public bool isAttendingSocialMeetupToday()
    {
        return this.is_attending_social_meetup_today;
    }

    public bool numSocialMeetupsBelowThreshold()
    {
        return this.num_social_meetups < Parameters.Instance.MAX_NUMBER_OF_SOCIAL_MEETUPS_PER_WEEK;
    }

    public void resetNumSocialMeetups()
    {
        this.num_social_meetups = 0;
    }

    public void incrementNumSocialMeetups()
    {
        ++num_social_meetups;
    }

    public bool tryInviteToSocial(Social s)
    {
        if (this.canAttendSocialMeetup(s))
        {
            this.setSocialMeetupBuilding(s);
            return true;
        } else
        {
            return false;
        }
    }

    public void setSocialMeetupBuilding(Social s)
    {
        if (s == null)
        {
            removeSocialMeetupBuilding();
            return;
        }
        this.social_meetup_building = s;
        this.is_attending_social_meetup_today = true;
        this.incrementNumSocialMeetups();
        ++total_num_social_meetups_attended;
    }

    public void removeSocialMeetupBuilding()
    {
        this.social_meetup_building = null;
        this.is_attending_social_meetup_today = false;
    }

    public Social getSocialMeetupBuilding()
    {
        return this.social_meetup_building;
    }

    public void setSocialGroupIndex(int index)
    {
        this.social_group_index = index;
    }

    public int getSocialGroupIndex()
    {
        return this.social_group_index;
    }
    
    public List<Agent> getSocialGroup()
    {
        if (this.social_meetup_building != null)
        {
            return this.social_meetup_building.getSocialGroup(this.social_group_index);
        }
        return null;
    }

    public void tryToArrangeSocialMeetup()
    {
        if (Random.value <= Parameters.Instance.DAILY_CHANCE_OF_SOCIAL_MEETUP)
        {
            //socialMeetupRule1();
            //socialMeetupRule2();
            socialMeetupRule3();
        }
    }

    public int getTotalNumSocialMeetupsAttended()
    {
        return this.total_num_social_meetups_attended;
    }

    private void socialMeetupRule1()
    {
        if (this.canAttendSocialMeetup())
        {
            /* TODO Pick to meet friends or family -> For now just friends */
            Agent[] agents = this.getFriends();
            List<Agent> available_agents = new List<Agent>();
            Social s;
            if (this.isAdult())
            {
                s = Landscape.Instance.getRandomAdultSocial();
            } 
            else
            {
                s = Landscape.Instance.getRandomChildSocial();
            }
            foreach (Agent a in agents)
            {
                if (a.canAttendSocialMeetup(s))
                {
                    available_agents.Add(a);
                }
            }
            if (available_agents.Count > 0) /* No need for a second loop here really */
            {
                this.setSocialMeetupBuilding(s);
                foreach (Agent a in available_agents)
                {
                    a.setSocialMeetupBuilding(s);
                }
            }
        }
    }

    private void socialMeetupRule2()
    {
        if (this.canAttendSocialMeetup())
        {
            /* TODO Pick to meet friends or family -> For now just friends */
            Agent[] agents = this.getFriends();
            List<Agent> available_agents = new List<Agent>();
            Social s;
            if (this.isAdult())
            {
                s = Landscape.Instance.getRandomAdultSocial();
            }
            else
            {
                s = Landscape.Instance.getRandomChildSocial();
            }
            foreach (Agent a in agents)
            {
                if (a.canAttendSocialMeetup(s))
                {
                    available_agents.Add(a);
                }
            }
            if (available_agents.Count > 0) /* No need for a second loop here really */
            {
                this.setSocialMeetupBuilding(s);
                foreach (Agent a in available_agents)
                {
                    a.setSocialMeetupBuilding(s);
                    a.inviteYourFriends(s);
                }
            }
        }
    }

    private void socialMeetupRule3()
    {
        if (this.canAttendSocialMeetup())
        {
            /* TODO Pick to meet friends or family -> For now just friends */
            Agent[] agents = this.getFriends();
            List<Agent> available_agents = new List<Agent>();
            Social s;
            if (this.isAdult())
            {
                s = Landscape.Instance.getRandomAdultSocial();
            }
            else
            {
                s = Landscape.Instance.getRandomChildSocial();
            }
            foreach (Agent a in agents)
            {
                if (a.canAttendSocialMeetup(s))
                {
                    available_agents.Add(a);
                }
            }
            if (available_agents.Count > 0) /* No need for a second loop here really */
            {
                int group_index = s.createNewSocialGroup();
                s.addAgentToSocialGroup(group_index, this);
                this.setSocialMeetupBuilding(s);
                this.setSocialGroupIndex(group_index);
                foreach (Agent a in available_agents)
                {
                    a.setSocialMeetupBuilding(s);
                    a.setSocialGroupIndex(group_index);
                    s.addAgentToSocialGroup(group_index, a);
                    //a.inviteYourFriends(s, group_index);
                }
                //Debug.Log("(1) Invited " + available_agents.Count + "/" + agents.Length + " of my friends.");
            }
        }
    }

    public void inviteYourFriends(Social s)
    {
        Agent[] friends = this.getFriends();
        int count = 0;
        foreach (Agent a in friends)
        {
            a.tryInviteToSocial(s);
            //if (a.tryInviteToSocial(s))
            //{
            //    ++count;
            //}
        }
        //Debug.Log("(2) Invited " + count + "/" + friends.Length + " of my friends.");
    }

    public void inviteYourFriends(Social s, int group_index)
    {
        Agent[] friends = this.getFriends();
        int count = 0;
        foreach (Agent a in friends)
        {
            if (a.tryInviteToSocial(s))
            {
                s.addAgentToSocialGroup(group_index, a);
                a.setSocialGroupIndex(group_index);
                ++count;
            }
        }
        Debug.Log("(2) Invited " + count + "/" + friends.Length + " of my friends.");
    }

    public void highlightAgentAndFriends()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.red;
        sr.sortingOrder = 2;
        if (this.node) this.node.highlightRed();
        foreach (Agent a in this.getFriends())
        {
            a.highlightAgent();
        }
    }

    public void unHighlightAgentAndFriends()
    {
        this.unHighlightAgent();
        foreach (Agent a in this.getFriends())
        {
            a.unHighlightAgent();
        }
    }

    public void highlightAgent()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.green;
        sr.sortingOrder = 1;
        if (this.node) this.node.highlightGreen();
    }

    public void unHighlightAgent()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        sr.color = Color.white;
        sr.sortingOrder = 0;
        if (this.node) this.node.unHighlight();
    }

    public string getCloseFriendsString()
    {
        return this.close_friends.toString();
    }

    public bool hasEncountered(Agent a)
    {
        return this.close_friends.hasTriedToInsert(a);
    }

    public float getWorstFriendValue()
    {
        return this.close_friends.getMinValue();
    }

    public string getFriendIDs()
    {
        return this.close_friends.getAgentIDs();
    }
}
