using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstantiator : MonoBehaviour
{
    [SerializeField] private Texture2D[] maps;
    [SerializeField] private List<string> filenames;
    private string foldername = "Tests\\";

    private void Start()
    {
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
    }

    private void loadNextTest()
    {
        if (filenames.Count == 0) return;
        int index = filenames.Count - 1;
        System.IO.StreamReader r = new System.IO.StreamReader(filenames[index]);
        string s = r.ReadToEnd();

        JsonUtility.FromJsonOverwrite(s, Parameters.Instance);
        int selected_map_id = Parameters.Instance.SELECTED_MAP_INDEX;
        if (selected_map_id < 0 || selected_map_id >= maps.Length) selected_map_id = 0;
        Landscape.Instance.Initialise(maps[selected_map_id]);
        filenames.RemoveAt(index);
    }
}
