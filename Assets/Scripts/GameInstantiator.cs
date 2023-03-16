using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstantiator : MonoBehaviour
{
    [SerializeField] private bool SANDBOX_MODE;
    [SerializeField] private Texture2D SANDBOX_MAP;
    [SerializeField] private Texture2D[] maps;
    [SerializeField] private List<string> filenames;
    private string foldername = "Tests\\";
    private string BASELINE_TXT = "BASELINE.txt";
    [SerializeField] private bool CREATE_TEST_SCRIPTS;
    private int NUM_ITERATIONS_PER_TEST = 25;
    private bool launched_last_test;
    private int TOTAL_NUM_TESTS;

    private void Start()
    {
        launched_last_test = false;
        if (CREATE_TEST_SCRIPTS)
        {
            //createTestScripts();
            //createTestScriptsRange();
            generateMarginalisationTests();
            //generateMarginalisationTestsRange();
            return;
        }

        if (SANDBOX_MODE)
        {
            Landscape.Instance.Initialise(SANDBOX_MAP);
            return;
        }

        string[] temp = System.IO.Directory.GetFiles(foldername);
        foreach (string filename in temp)
        {
            filenames.Add(filename);
        }
        TOTAL_NUM_TESTS = filenames.Count;
    }

    private void FixedUpdate()
    {
        if (filenames.Count > 0)
        {
            if (Landscape.Instance.hasTerminated())
            {
                loadNextTest();
            }
        }
        if (launched_last_test && Landscape.Instance.hasTerminated())
        {
            launched_last_test = false;
            Debug.Log("All tests have terminated!");
        }
    }

    private void loadNextTest()
    {
        if (filenames.Count == 0) return;

        Debug.Log("Loading test " + (TOTAL_NUM_TESTS - filenames.Count + 1) + " of " + TOTAL_NUM_TESTS + ".");

        int index = filenames.Count - 1;
        System.IO.StreamReader r = new System.IO.StreamReader(filenames[index]);
        string s = r.ReadToEnd();
        r.Close();
        JsonUtility.FromJsonOverwrite(s, Parameters.Instance);
        int selected_map_id = Parameters.Instance.SELECTED_MAP_INDEX;
        if (selected_map_id < 0 || selected_map_id >= maps.Length) selected_map_id = 0;
        Landscape.Instance.Initialise(maps[selected_map_id]);
        
        filenames.RemoveAt(index);
        if (filenames.Count == 0) launched_last_test = true;
    }

    private void createTestScripts()
    {
        /* This is the part to chaenge */
        /* ============================================ */
        int min_val = 100;
        int max_val = 100;
        int step = 1;
        string test_name = "Number of years to run";
        int num_seeds = 10;
        /* ============================================ */

        print("Generating test scripts into directory: " + "\"Generated_Tests\\\"");
        for (int i = min_val; i <= max_val; i += step)
        {
            for (int seed = 1; seed <= num_seeds; seed++)
            {
                System.IO.StreamReader r = new System.IO.StreamReader(BASELINE_TXT);
                string s = r.ReadToEnd();
                r.Close();
                JsonUtility.FromJsonOverwrite(s, Parameters.Instance);
                Parameters.Instance.TEST_NAME = test_name + " (" + i + ")";
                Parameters.Instance.SEED = seed;

                /* This is the part to change */
                /* ============================================ */
                Parameters.Instance.NUM_YEARS_TO_RUN = i;
                /* ============================================ */

                string filename = "Generated_Tests\\" + Parameters.Instance.TEST_NAME.Replace(" ", "-") + " - " + seed + ".txt";
                System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, true);
                writer.Write(JsonUtility.ToJson(Parameters.Instance, true));
                writer.Close();
                print("Created test script: " + filename);
            }
        }
        print("Finished creating test scripts.");
    }

    private void createTestScriptsRange()
    {
        /* This is the part to change */
        /* ============================================ */
        int min_val = 0;
        int max_val = 15;
        int step = 1;
        string test_name = "Number of friends";
        int num_seeds = 10;
        /* ============================================ */

        print("Generating test scripts into directory: " + "\"Generated_Tests\\\"");
        for (int i = min_val; i <= max_val; i += step)
        {
            for (int j = i + step; j <= max_val; j += step)
            {
                for (int seed = 1; seed <= num_seeds; seed++)
                {
                    System.IO.StreamReader r = new System.IO.StreamReader(BASELINE_TXT);
                    string s = r.ReadToEnd();
                    r.Close();
                    JsonUtility.FromJsonOverwrite(s, Parameters.Instance);
                    Parameters.Instance.TEST_NAME = test_name + " (" + i + "," + j + ")";
                    Parameters.Instance.SEED = seed;

                    /* This is the part to change */
                    /* ============================================ */
                    Parameters.Instance.MIN_FRIENDS = i;
                    Parameters.Instance.MAX_FRIENDS = j;
                    /* ============================================ */

                    string filename = "Generated_Tests\\" + Parameters.Instance.TEST_NAME.Replace(" ", "-") + " - " + seed + ".txt";
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, true);
                    writer.Write(JsonUtility.ToJson(Parameters.Instance, true));
                    writer.Close();
                    print("Created test script: " + filename);
                }
            }
        }
        print("Finished creating test scripts.");
    }

    private void generateMarginalisationTests()
    {
        int step = ParametersBounds.Instance.STEP_NUM_YEARS_TO_RUN;
        int min_val = ParametersBounds.Instance.MIN_NUM_YEARS_TO_RUN;
        int max_val = ParametersBounds.Instance.MAX_NUM_YEARS_TO_RUN;
        string test_name = "Years to run";

        print("Generating test scripts into directory: " + "\"Generated_Tests\\\"");
        for (int val = min_val; val <= max_val; val += step)
        {
            for (int it = 1; it <= this.NUM_ITERATIONS_PER_TEST; ++it)
            {
                System.IO.StreamReader r = new System.IO.StreamReader(BASELINE_TXT);
                string s = r.ReadToEnd();
                r.Close();
                JsonUtility.FromJsonOverwrite(s, Parameters.Instance);
                Parameters.Instance.TEST_NAME = test_name + " (" + val + ") " + "(" + it + ")";

                /* Set the seed for randomizsation */
                Parameters.Instance.SEED = it;
                Random.InitState(it);

                /* Randomize all values (Reset the one that we want to keep constant afterwards) */
                Parameters.Instance.randomize();

                /* This is the part to change */
                /* ============================================ */
                Parameters.Instance.NUM_YEARS_TO_RUN = val;
                /* ============================================ */

                string filename = "Generated_Tests\\" + Parameters.Instance.TEST_NAME.Replace(" ", "-") + ".txt";
                System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, true);
                writer.Write(JsonUtility.ToJson(Parameters.Instance, true));
                writer.Close();
                print("Created test script: " + filename);
            }
        }
        print("Finished creating test scripts.");
    }

    private void generateMarginalisationTestsRange()
    {
        int step = ParametersBounds.Instance.STEP_NUM_FRIENDS;
        int min_val = ParametersBounds.Instance.MIN_NUM_FRIENDS;
        int max_val = ParametersBounds.Instance.MAX_NUM_FRIENDS;
        string test_name = "Number of friends";

        print("Generating test scripts into directory: " + "\"Generated_Tests\\\"");
        for (int val_min = min_val; val_min <= max_val; val_min += step)
        {
            for (int val_max = val_min; val_max <= max_val; val_max += step)
            {
                for (int it = 1; it <= this.NUM_ITERATIONS_PER_TEST; ++it)
                {
                    System.IO.StreamReader r = new System.IO.StreamReader(BASELINE_TXT);
                    string s = r.ReadToEnd();
                    r.Close();
                    JsonUtility.FromJsonOverwrite(s, Parameters.Instance);
                    Parameters.Instance.TEST_NAME = test_name + " (" + val_min + "-" + val_max + ") " + "(" + it + ")";

                    /* Set the seed for randomizsation */
                    Parameters.Instance.SEED = it;
                    Random.InitState(it);

                    /* Randomize all values (Reset the one that we want to keep constant afterwards) */
                    Parameters.Instance.randomize();

                    /* This is the part to change */
                    /* ============================================ */
                    Parameters.Instance.MIN_FRIENDS = val_min;
                    Parameters.Instance.MAX_FRIENDS = max_val;
                    /* ============================================ */

                    string filename = "Generated_Tests\\" + Parameters.Instance.TEST_NAME.Replace(" ", "-") + ".txt";
                    System.IO.StreamWriter writer = new System.IO.StreamWriter(filename, true);
                    writer.Write(JsonUtility.ToJson(Parameters.Instance, true));
                    writer.Close();
                    print("Created test script: " + filename);
                }
            }
        }
        print("Finished creating test scripts.");
    }
}
