using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstantiator : MonoBehaviour
{
    [SerializeField] private Texture2D[] maps;
    private string foldername = "Tests\\";

    private void Start()
    {
        //string s = JsonUtility.ToJson(Parameters.Instance,true);
        //print(s);
        //JsonUtility.FromJsonOverwrite(s,Parameters.Instance);

        System.IO.StreamReader r = new System.IO.StreamReader(foldername + "Test1.txt");
        string s = r.ReadToEnd();
        //print(s);
        //print(JsonUtility.ToJson(Parameters.Instance,true));
        //s = s.Replace("\r\n", "\n").Replace("\n","").Replace(" ","");
        //print(s);
        JsonUtility.FromJsonOverwrite(s, Parameters.Instance);
        int selected_map_id = Parameters.Instance.SELECTED_MAP_INDEX;
        if (selected_map_id < 0 || selected_map_id >= maps.Length) selected_map_id = 0;
        Landscape.Instance.Initialise(maps[selected_map_id]);
    }
}
