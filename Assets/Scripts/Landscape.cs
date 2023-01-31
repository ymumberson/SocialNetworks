using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Landscape : MonoBehaviour
{
    public static Landscape Instance;
    
    public enum TimeState { Morning, Midday, WalkingToSocial, SocialTime, HomeTime,  NightTime }

    /* Settings */
    [SerializeField] private bool AGENT_PATHFINDING;

    /* Externally visible variables */
    [SerializeField] private Texture2D MAP_IMAGE;
    [SerializeField] private GameObject TILE_TEMPLATE;
    [SerializeField] private GameObject AGENT_TEMPLATE;
    [SerializeField] private GraphRendererScript graphRenderer;
    [SerializeField] private InGameScript inGameUI;
    [SerializeField] private int day;
    [SerializeField] private int year;
    [SerializeField] private TimeState time;

    /* Internal variables */
    private TileInfo[,] terrain;
    private int width, height;
    [SerializeField] private List<Agent> agents;
    private float turn_timer = 0f;
    [SerializeField] private float timer;
    [SerializeField] private int num_adults;
    [SerializeField] private int num_children;
    [SerializeField] private Agent highlighted_agent;

    private void Awake()
    {
        if (Instance)
        {
            Destroy(this.gameObject);
        } else
        {
            Instance = this;
        }
        AGENT_PATHFINDING = true;
        terrain = new MapLoader().textureToGameArray(MAP_IMAGE,TILE_TEMPLATE); // TODO -> Doesn't like the 'new' keyword!
        this.width = terrain.GetLength(0); // Right now map is 50x50 so idk if these dimensions are correct :(
        this.height = terrain.GetLength(1);

        agents = new List<Agent>();

        InitialPopulationRule1();
        initialiseGraphRenderer();

        day = 1;
        year = 1;
        time = TimeState.Morning;
        setAllAgentPathsToWorkSchool();
    }

    private void Start()
    {
        /* Centre the camera */
        float tileWidth = 1f;
        float cameraHigh = tileWidth * height;
        Vector3 camera_centre = new Vector3(tileWidth * width / 2f, cameraHigh / 2f, -10f);
        Camera.main.transform.position = camera_centre;
        Camera.main.orthographicSize = (cameraHigh / 2f) * 1.1f;
    }

    private void Update()
    {
        updateHighlightedAgent();
        inGameUI.updateLandscapeText(this);
        inGameUI.updateGraphRendererText(graphRenderer);
    }

    private void FixedUpdate()
    {

        turn_timer += Time.fixedDeltaTime;
        if (turn_timer >= Parameters.Instance.TURN_TIME)
        {
            turn_timer = 0f;
            updateAgentPaths();

            //num_adults = 0;
            //num_children = 0;
            //foreach (Agent a in agents)
            //{
            //    if (a.isAdult())
            //    {
            //        ++num_adults;
            //    } else
            //    {
            //        ++num_children;
            //    }
            //}
        }
        
    }

    private void updateAgentPaths()
    {
        switch (time)
        {
            case TimeState.Morning:
                updateAllAgentPaths();
                //teleportAgentsToDestinations();
                if (allAgentsReachedDestination())
                {
                    time = TimeState.Midday;
                }
                break;
            case TimeState.Midday:

                timer += Time.fixedDeltaTime;
                if (timer > Parameters.Instance.TIME_AT_WORK_SCHOOL)
                {
                    dailyUpdateAllAgents(); /* Update their states halfway through the day ie at work/school */
                    timer = 0;
                    time = TimeState.WalkingToSocial;
                    updateWorkSchoolFriends();
                    tryArrangeSocialMeetups();
                    setAllAgentPathsToHomeOrSocial();
                    updateSocialNetworkGraph();
                }
                break;
            case TimeState.WalkingToSocial:
                updateAllAgentPaths();
                if (allAgentsReachedDestination())
                {
                    time = TimeState.SocialTime;
                }
                break;
            case TimeState.SocialTime:
                timer += Time.fixedDeltaTime;
                if (timer > Parameters.Instance.TIME_AT_SOCIAL)
                {
                    timer = 0;
                    time = TimeState.HomeTime;
                    setAllAgentPathsToHome();
                    removeAllSocialMeetupBuildings();
                    updateSocialBuildingFriends();
                    updateSocialNetworkGraph();
                }
                break;
            case TimeState.HomeTime:
                updateAllAgentPaths();
                if (allAgentsReachedDestination())
                {
                    time = TimeState.NightTime;
                }
                break;
            case TimeState.NightTime:
                timer += Time.fixedDeltaTime;
                if (timer > Parameters.Instance.TIME_AT_HOME)
                {
                    timer = 0;
                    time = TimeState.Morning;
                    setAllAgentPathsToWorkSchool();
                    ++day;
                    if (day >= Parameters.Instance.DAYS_PER_YEAR)
                    {
                        day = 0;
                        ++year;
                        ageAllAgents();
                    }
                    resetAllAgentSocialMeetupCounters();
                }
                break;
        }
    }

    public void dailyUpdateAllAgents()
    {
        //foreach (Agent a in agents) /* Causes an issue bc list is modified by dailyUpdate() */
        //{
        //    a.dailyUpdate();
        //}
        Agent[] arr = agents.ToArray();
        for (int i=0; i<arr.Length; ++i)
        {
            arr[i].dailyUpdate();
        }
    }

    public void updateWorkSchoolFriends()
    {
        Agent[] arr = agents.ToArray();
        for (int i = 0; i < arr.Length; ++i)
        {
            List<Agent> coworkers = getCoworkers(arr[i].getWorkSchool(),arr[i]);
            foreach (Agent a in coworkers) {
                arr[i].tryAddFriend(a);
            }
            //arr[i].debug_printFriends();
        }
    }

    /// <summary>
    /// For each agent, tries to add every other agent who is in the same building to
    /// the agent's close friends list.
    /// </summary>
    public void updateSocialBuildingFriends()
    {
        foreach (Agent a in agents)
        {
            if (!a.isAttendingSocialMeetupToday()) continue;
            foreach (Agent other in agents)
            {
                if (other == a) continue;
                if (a.getSocialMeetupBuilding() == other.getSocialMeetupBuilding())
                {
                    //Debug.Log("Trying to make a friend from the social");
                    a.tryAddFriend(other);
                }
            }
        }
    }

    public void tryArrangeSocialMeetups()
    {
        foreach (Agent a in agents)
        {
            a.tryToArrangeSocialMeetup();
        }
    }

    public void removeAllSocialMeetupBuildings()
    {
        foreach(Agent a in agents)
        {
            a.removeSocialMeetupBuilding();
        }
    }
    
    public void resetAllAgentSocialMeetupCounters()
    {
        foreach (Agent a in agents)
        {
            a.resetNumSocialMeetups();
        }
    }

    public void ageAllAgents()
    {
        num_adults = 0;
        num_children = 0;
        foreach (Agent a in agents.ToArray()) /* Convert to array to avoid collection modified enumeration error */
        {
            a.incrementAge();
            if (a.isAdult())
            {
                ++num_adults;
            } else
            {
                ++num_children;
            }
        }
    }

    public bool updateAllAgentPaths()
    {
        if (!AGENT_PATHFINDING) return teleportAgentsToDestinations();

        bool allreached = true;
        foreach (Agent a in agents)
        {
            a.moveAlongPath();
            if (!a.reachedDestination()) allreached = false;
        }
        return allreached;
    }

    public bool teleportAgentsToDestinations()
    {
        foreach (Agent a in agents)
        {
            a.teleportToDestination();
        }
        return true;
    }

    public void setAllAgentPathsToWorkSchool()
    {
        foreach (Agent a in agents)
        {
            a.calculatePathToWorkSchool();
        }
    }

    public void setAllAgentPathsToHome()
    {
        foreach (Agent a in agents)
        {
            a.calculatePathToHome();
        }
    }

    public void trySetAllAgentPathsToSocial()
    {
        foreach (Agent a in agents)
        {
            if (a.isAttendingSocialMeetupToday())
            {
                a.calculatePathToSocial();
            }
        }
    }

    public void setAllAgentPathsToHomeOrSocial()
    {
        foreach (Agent a in agents)
        {
            if (a.isAttendingSocialMeetupToday())
            {
                a.calculatePathToSocial();
            } 
            else
            {
                a.calculatePathToHome();
            }
        }
    }

    public bool allAgentsReachedDestination()
    {
        foreach (Agent a in agents)
        {
            if (!a.reachedDestination()) return false;
        }
        return true;
    }

    public House[] getAllHouses() /* Awkward method to make generic bc of casting problems :( */
    {
        List<House> ls = new List<House>();

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                if (terrain[i, j].tile.tile_type == Tile.TileType.House)
                {
                    ls.Add((House)terrain[i, j].tile);
                }
            }
        }
        return ls.ToArray();
    }

    public House getEmptyHouse()
    {
        foreach (House h in getAllHouses())
        {
            if (h.isEmpty()) return h;
        }
        return null;
    }

    public List<Agent> getCoworkers(Building workplace_school, Agent self) /* also returns agent who called it */
    {
        List<Agent> ls = new List<Agent>();
        foreach (Agent a in agents)
        {
            if (a == self) continue; /* Skips so doesn't add 'self' to list */ 
            if (a.getWorkSchool() == workplace_school)
            {
                ls.Add(a);
            }
        }
        return ls;
    }

    public List<Agent> getEmployees(Building workplace_school) /* also returns agent who called it */
    {
        List<Agent> ls = new List<Agent>();
        foreach (Agent a in agents)
        {
            if (a.getWorkSchool() == workplace_school)
            {
                ls.Add(a);
            }
        }
        return ls;
    }

    public Workplace[] getAllWorkplaces()
    {
        List<Workplace> ls = new List<Workplace>();

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                if (terrain[i, j].tile.tile_type == Tile.TileType.Workplace)
                {
                    ls.Add((Workplace)terrain[i, j].tile);
                }
            }
        }
        return ls.ToArray();
    }

    public School[] getAllSchools()
    {
        List<School> ls = new List<School>();

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                if (terrain[i, j].tile.tile_type == Tile.TileType.School)
                {
                    ls.Add((School)terrain[i, j].tile);
                }
            }
        }
        return ls.ToArray();
    }

    public Social[] getAllSocials()
    {
        List<Social> ls = new List<Social>();

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                if (terrain[i, j].tile.tile_type == Tile.TileType.Social)
                {
                    ls.Add((Social)terrain[i, j].tile);
                }
            }
        }
        return ls.ToArray();
    }

    public Social[] getAllChildSocials()
    {
        List<Social> ls = new List<Social>();

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                if (terrain[i, j].tile.tile_type == Tile.TileType.Social
                    && ((Social)(terrain[i, j].tile)).forChildren())
                {
                    ls.Add((Social)terrain[i, j].tile);
                }
            }
        }
        return ls.ToArray();
    }

    public Social[] getAllAdultSocials()
    {
        List<Social> ls = new List<Social>();

        for (int j = 0; j < height; ++j)
        {
            for (int i = 0; i < width; ++i)
            {
                if (terrain[i, j].tile.tile_type == Tile.TileType.Social
                    && ((Social)(terrain[i, j].tile)).forAdults())
                {
                    ls.Add((Social)terrain[i, j].tile);
                }
            }
        }
        return ls.ToArray();
    }

    public School getRandomSchool()
    {
        School[] schools = getAllSchools();
        return schools[Random.Range(0, schools.Length)];
    }

    public Workplace getRandomWorkplace()
    {
        Workplace[] workplaces = getAllWorkplaces();
        return workplaces[Random.Range(0, workplaces.Length)];
    }

    public Social getRandomSocial()
    {
        Social[] socials = getAllSocials();
        return socials[Random.Range(0, socials.Length)];
    }

    public Social getRandomChildSocial()
    {
        Social[] socials = getAllChildSocials();
        return socials[Random.Range(0, socials.Length)];
    }

    public Social getRandomAdultSocial()
    {
        Social[] socials = getAllAdultSocials();
        return socials[Random.Range(0, socials.Length)];
    }

    public void removeAgent(Agent a)
    {
        this.agents.Remove(a);
        removeAgentFromGraphRenderer(a);
    }

    public void addAgent(Agent a)
    {
        this.agents.Add(a);
        addAgentToGraphRenderer(a);
    }

    public Agent instantiateAgent(int age, Agent.Gender gender, House home, Building work_school, Agent[] parents, bool home_owner, string personality)
    {
        Agent a = Instantiate(AGENT_TEMPLATE).GetComponent<Agent>();
        a.initialise_agent(age,gender,home,work_school,parents,home_owner,personality);
        addAgent(a);
        return a;
    }

    //public House[] getAllHouses()
    //{
    //    Tile[] tile_ls = getAll(Tile.TileType.House);
    //    House[] house_ls = new House[tile_ls.Length];
    //    for (int i=0; i<tile_ls.Length; ++i)
    //    {
    //        house_ls[i] = (House)(tile_ls[i]);
    //    }
    //    return house_ls;
    //}

    //public Tile[] getAll(Tile.TileType tile_type)
    //{
    //    List<Tile> ls = new List<Tile>();

    //    for (int j = 0; j < height; ++j)
    //    {
    //        for (int i = 0; i < width; ++i)
    //        {
    //            if (terrain[i, j].tile.tile_type == tile_type)
    //            {
    //                ls.Add(terrain[i, j].tile);
    //            }
    //        }
    //    }
    //    return ls.ToArray();
    //}

    public int getDay()
    {
        return day;
    }

    public int getYear()
    {
        return year;
    }

    public TimeState getTime()
    {
        return this.time;
    }

    public string getTimeString()
    {
        switch (this.time)
        {
            case TimeState.HomeTime:
                return "Home Time";
            case TimeState.Midday:
                return "Midday";
            case TimeState.Morning:
                return "Morning";
            case TimeState.NightTime:
                return "Night Time";
            case TimeState.SocialTime:
                return "Home";
            case TimeState.WalkingToSocial:
                return "Home";
        }
        return "TimE iSNt rEaL MaN";
    }

    public int getNumChildren()
    {
        return num_children;
    }

    public int getNumAdults()
    {
        return this.num_adults;
    }

    public Vector2 getLocation(Tile t)
    {
        for (int j=0; j<height; ++j)
        {
            for (int i=0; i<width; ++i)
            {
                if (t == terrain[i, j].tile) return new Vector2(i, j);
            }
        }
        return new Vector2(-1,-1); //TODO This will crash the program if tile is not found and not checked!
    }

    public bool inBounds(int x, int y)
    {
        return (x >= 0 && x < width) && (y >= 0 && y < height);
    }

    public List<Vector2> pathFind(Vector2 start, Vector2 finish)
    {
        /* Need to reset all TileInfo values to null for whole landscape first */
        TileInfo temp;
        for (int j=0; j<height; ++j)
        {
            for (int i=0; i<width; ++i)
            {
                temp = terrain[i, j];
                temp.g = 0;
                temp.h = 0;
                temp.parent = null;
            }
        }
        
        List<TileInfo> open_ls = new List<TileInfo>();
        List<TileInfo> closed_ls = new List<TileInfo>();

        /* start */
        TileInfo starting_tile = terrain[(int)start.x, (int)start.y];
        starting_tile.g = 0;
        starting_tile.h = (int)(Mathf.Pow(finish.x - start.x,2) + Mathf.Pow(finish.y - start.y ,2));
        starting_tile.parent = null;
        open_ls.Add(starting_tile);


        while (open_ls.Count > 0) /* Change to exit condition!!!!!!!! */
        {
            /* Look for lowest F cost on the open list */
            TileInfo current_square = open_ls[0]; // Might cause a crash??
            int lowest_f = int.MaxValue;
            foreach (TileInfo t in open_ls)
            {
                if (t.f() < lowest_f)
                {
                    lowest_f = t.f();
                    current_square = t;
                }
            }

            /* Move it from the open list to the closed list */
            open_ls.Remove(current_square);
            closed_ls.Add(current_square);

            /* Check if current square is target tile */
            if (current_square.tile.x == finish.x && current_square.tile.y == finish.y)
            {
                //Debug.Log("Found target tile!");
                break;
            }/* It should exit the while loop here... */

            /* Check the adjacent 8 tiles of the current square */
            for (int i=-1; i<=1; ++i)
            {
                for (int j=-1; j<=1; ++j)
                {
                    if (Mathf.Abs(i) == Mathf.Abs(j)) continue; //Skips corners

                    int x1 = current_square.tile.x + i;
                    int y1 = current_square.tile.y + j;
                    if (!(i==0 && j==0) && (inBounds(x1,y1)) && terrain[x1,y1].tile.can_walk_on) /* Skip over if looking at current tile */
                    {
                        TileInfo adj = terrain[x1, y1];
                        adj.g = current_square.g + 1;
                        adj.h = (int)(Mathf.Pow(finish.x - current_square.tile.x, 2) + Mathf.Pow(finish.y - current_square.tile.y, 2));

                        /* Check current tile is not on closed list */
                        bool tile_in_closed_list = false;
                        foreach (TileInfo t in closed_ls)
                        {
                            if (t.tile.x == adj.tile.x && t.tile.y == adj.tile.y)
                            {
                                tile_in_closed_list = true;
                                break;
                            }/* Break as tile is in closed list */
                        }
                        if (tile_in_closed_list) continue;
                        /* Reached here so tile is not in closed list */

                        bool in_open_list = false;
                        foreach (TileInfo t in open_ls)
                        {
                            if (t.tile.x == adj.tile.x && t.tile.y == adj.tile.y)
                            {
                                in_open_list = true;
                                if (current_square.g < t.parent.g)
                                {
                                    t.g = current_square.g + 1;
                                    t.parent = current_square;
                                }
                            }
                        }
                        /* If adjacent tile wasn't in open list then add it to the open list */
                        if (!in_open_list)
                        {
                            open_ls.Add(adj);
                            adj.parent = current_square;
                        }
                    }
                }
            }
        }

        /* Found tile so check convert closed list into a vector2 list */
        List<Vector2> ls = new List<Vector2>();
        TileInfo current = terrain[(int)finish.x, (int)finish.y];

        while (current != null)
        {
            ls.Add(new Vector2(current.tile.x, current.tile.y));
            current = current.parent;
        }
        return ls;
    }

    private void InitialPopulationRule1() /* Random Generation */
    {
        House[] houses = getAllHouses();
        Workplace[] workplaces = getAllWorkplaces();
        School[] schools = getAllSchools();
        House h;
        //foreach (House h in houses)
        for (int j=0; j<houses.Length/2; ++j) /* Default is full half of the houses */
        {
            h = houses[j];
            Agent mum = Instantiate(AGENT_TEMPLATE).GetComponent<Agent>();
            mum.initialise_agent(
                Random.Range(Parameters.Instance.MIN_INITIAL_PARENT_AGE, Parameters.Instance.MAX_INITIAL_PARENT_AGE),
                Agent.Gender.Female,
                h,
                workplaces[Random.Range(0, workplaces.Length)],
                null,
                true,
                generateRandomPersonality()
                );
            mum.moveTo(getLocation(mum.getHome()));
            agents.Add(mum);
            h.addOccupant(mum);

            Agent dad = Instantiate(AGENT_TEMPLATE).GetComponent<Agent>();
            dad.initialise_agent(
                Random.Range(Parameters.Instance.MIN_INITIAL_PARENT_AGE, Parameters.Instance.MAX_INITIAL_PARENT_AGE),
                Agent.Gender.Male,
                h,
                workplaces[Random.Range(0, workplaces.Length - 1)],
                null,
                true,
                generateRandomPersonality()
                );
            dad.moveTo(getLocation(dad.getHome()));
            agents.Add(dad);
            h.addOccupant(dad);

            mum.setSpouse(dad);
            dad.setSpouse(mum);

            School household_school = schools[Random.Range(0, schools.Length)];
            for (int i = 0; i < Random.Range(Parameters.Instance.MIN_INITIAL_CHILDREN, Parameters.Instance.MAX_INITIAL_CHILDREN); ++i)
            {
                Agent child = Instantiate(AGENT_TEMPLATE).GetComponent<Agent>();
                child.initialise_agent(
                    Random.Range(Parameters.Instance.MIN_INITIAL_CHILD_AGE, Parameters.Instance.MAX_INITIAL_CHILD_AGE),
                    (Random.Range(0, 2) == 0) ? Agent.Gender.Male : Agent.Gender.Female,
                    h,
                    household_school,
                    new Agent[] { mum, dad },
                    false,
                    generateRandomPersonality(dad.getPersonality(), mum.getPersonality())
                    );
                child.moveTo(getLocation(child.getHome()));
                agents.Add(child);
                h.addOccupant(child);
            }
        }
    }

    public string generateRandomPersonality()
    {
        string str = "";
        for (int i=0; i<Parameters.Instance.PERSONALITY_LENGTH; ++i)
        {
            if (Random.Range(0f,1f) > 0.5f)
            {
                str += '1';
            }
            else
            {
                str += '0';
            }
        }
        return str;
    }

    public string generateRandomPersonality(string dad, string mum)
    {
        string str = "";
        for (int i = 0; i < Parameters.Instance.PERSONALITY_LENGTH; ++i)
        {
            if (Random.Range(0f, 1f) > 0.5f)
            {
                str += dad[i];
            }
            else
            {
                str += mum[i];
            }
        }
        return str;
    }

    private void initialiseGraphRenderer()
    {
        foreach (Agent a in agents)
        {
            graphRenderer.addAgent(a);
        }
    }

    public void addAgentToGraphRenderer(Agent a)
    {
        graphRenderer.addAgent(a);
    }

    public void removeAgentFromGraphRenderer(Agent a)
    {
        graphRenderer.removeAgent(a);
    }

    public void updateSocialNetworkGraph()
    {
        graphRenderer.updateGraph();
    }

    public GraphRendererScript getGraphRenderer()
    {
        return this.graphRenderer;
    }

    public void setHighlightedAgent(Agent a)
    {
        if (highlighted_agent != null)
        {
            highlighted_agent.unHighlightAgentAndFriends();
        }
        this.highlighted_agent = a;
    }

    private void updateHighlightedAgent()
    {
        if (highlighted_agent == null) return;
        highlighted_agent.unHighlightAgentAndFriends();
        highlighted_agent.highlightAgentAndFriends();
        inGameUI.updateSelectedAgentText(highlighted_agent);
    }

    public void setHighlightedAgent(int agentID)
    {
        foreach (Agent a in agents)
        {
            if (a.getAgentID() == agentID)
            {
                this.setHighlightedAgent(a);
            }
        }
    }
}
