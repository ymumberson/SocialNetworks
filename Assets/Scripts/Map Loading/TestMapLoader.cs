using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class TestMapLoader : MonoBehaviour
{
    [SerializeField] private Sprite img;
    [SerializeField] private Texture2D texture;
    [SerializeField] private GameObject tile_template;
    private GameObject[,] terrain;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(texture.height);
        Debug.Log(texture.width);
        terrain = new GameObject[texture.width,texture.height];

        Color green = new Color(0, 0.6f, 0, 1);
        Color black = new Color(0, 0, 0, 1);
        Color yellow = new Color(0.925f, 0.901f, 0, 1);
        Color blue = new Color(0, 0, 0.925f, 1);
        Color pink = new Color(0.871f, 0, 0.925f,1);
        Color cyan = new Color(0,0.925f,0.8155f,1);

        Color STEFS_DARK_GREEN = new Color(0,0.4f,0,1);

        Color current_pixel_colour;
        for (int j=0; j<texture.height; ++j)
        {
            for (int i=0; i<texture.width; ++i)
            {
                current_pixel_colour = texture.GetPixel(i, j);
                terrain[i, j] = Instantiate(tile_template);
                terrain[i, j].transform.position = new Vector2(i, j);
                if (colour_equals(current_pixel_colour, black))
                {
                    Debug.Log("Road");
                    terrain[i, j].GetComponent<SpriteRenderer>().color = Color.black;
                    terrain[i, j].GetComponent<TileInfo>().tile = new Road(i,j);
                } else if (colour_equals(current_pixel_colour, green))
                {
                    Debug.Log("Terrain");
                    terrain[i, j].GetComponent<SpriteRenderer>().color = STEFS_DARK_GREEN;
                    terrain[i, j].GetComponent<TileInfo>().tile = new Offroad(i, j);
                } else if (colour_equals(current_pixel_colour, yellow))
                {
                    Debug.Log("Social");
                    terrain[i, j].GetComponent<SpriteRenderer>().color = Color.yellow;
                    terrain[i, j].GetComponent<TileInfo>().tile = new Social(i, j, (UnityEngine.Random.value <= Parameters.Instance.PERCENT_CHILD_SOCIAL_BUILDINGS));
                } else if (colour_equals(current_pixel_colour, blue))
                {
                    Debug.Log("Workplace");
                    terrain[i, j].GetComponent<SpriteRenderer>().color = Color.blue;
                    terrain[i, j].GetComponent<TileInfo>().tile = new Workplace(i, j);
                } else if (colour_equals(current_pixel_colour, pink))
                {
                    Debug.Log("School");
                    terrain[i, j].GetComponent<SpriteRenderer>().color = Color.magenta;
                    terrain[i, j].GetComponent<TileInfo>().tile = new School(i, j);
                } else if (colour_equals(current_pixel_colour, cyan))
                {
                    Debug.Log("Housing");
                    terrain[i, j].GetComponent<SpriteRenderer>().color = Color.cyan;
                    terrain[i, j].GetComponent<TileInfo>().tile = new House(i, j);
                } else
                {
                    Debug.Log(((int)(current_pixel_colour.r * 1000)) + "," + ((int)(current_pixel_colour.g * 1000)) + "," + ((int)(current_pixel_colour.b * 1000)));
                }
            }
        }
    }

    private bool colour_equals(Color c1, Color c2)
    {
        return ((int)(c1.r * 1000) == (int)(c2.r * 1000))
            && ((int)(c1.g * 1000) == (int)(c2.g * 1000))
            && ((int)(c1.b * 1000) == (int)(c2.b * 1000));
    }
}
