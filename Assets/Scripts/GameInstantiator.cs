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
    private bool launched_last_test;

    private void Start()
    {
        launched_last_test = false;
        if (CREATE_TEST_SCRIPTS)
        {
            createTestScripts();
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
        /* This is the part to change */
        /* ============================================ */
        float min_val = 1;
        float max_val = 100;
        float step = 1;
        string test_name = "Personality length";
        int num_seeds = 10;
        /* ============================================ */

        print("Generating test scripts into directory: " + "\"Generated_Tests\\\"");
        for (float i=min_val; i<=max_val; i+=step)
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
                Parameters.Instance.PERSONALITY_LENGTH = (int)i;
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
}
