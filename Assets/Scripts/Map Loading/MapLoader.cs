using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    public TileInfo[,] textureToGameArray(Texture2D texture, GameObject tile_template)
    {
        TileInfo[,] terrain = new TileInfo[texture.width, texture.height];

        /* Colours from the template image */
        Color green = new Color(0, 0.6f, 0, 1);
        Color black = new Color(0, 0, 0, 1);
        Color yellow = new Color(0.925f, 0.901f, 0, 1);
        Color blue = new Color(0, 0, 0.925f, 1);
        Color pink = new Color(0.871f, 0, 0.925f, 1);
        Color cyan = new Color(0, 0.925f, 0.8155f, 1);

        /* Just a colour for the green that I'm using */
        Color STEFS_DARK_GREEN = new Color(0, 0.4f, 0, 1);

        Color current_pixel_colour;
        GameObject go;
        List<Social> social_ls = new List<Social>();
        for (int j = 0; j < texture.height; ++j)
        {
            for (int i = 0; i < texture.width; ++i)
            {
                current_pixel_colour = texture.GetPixel(i, j);
                go = Instantiate(tile_template);
                go.transform.position = new Vector2(i, j);
                terrain[i, j] = go.GetComponent<TileInfo>();
                if (colour_equals(current_pixel_colour, black))
                {
                    terrain[i, j].GetComponent<SpriteRenderer>().color = Color.black;
                    terrain[i, j].GetComponent<TileInfo>().tile = new Road(i, j);
                }
                else if (colour_equals(current_pixel_colour, green))
                {
                    terrain[i, j].GetComponent<SpriteRenderer>().color = STEFS_DARK_GREEN;
                    terrain[i, j].GetComponent<TileInfo>().tile = new Offroad(i, j);
                }
                else if (colour_equals(current_pixel_colour, yellow))
                {
                    terrain[i, j].GetComponent<SpriteRenderer>().color = Color.yellow;
                    terrain[i, j].GetComponent<TileInfo>().tile = new Social(i, j, false); /* Default to forAdults */
                    social_ls.Add((Social)terrain[i, j].tile);
                }
                else if (colour_equals(current_pixel_colour, blue))
                {
                    terrain[i, j].GetComponent<SpriteRenderer>().color = Color.blue;
                    terrain[i, j].GetComponent<TileInfo>().tile = new Workplace(i, j);
                }
                else if (colour_equals(current_pixel_colour, pink))
                {
                    terrain[i, j].GetComponent<SpriteRenderer>().color = Color.magenta;
                    terrain[i, j].GetComponent<TileInfo>().tile = new School(i, j);
                }
                else if (colour_equals(current_pixel_colour, cyan))
                {
                    terrain[i, j].GetComponent<SpriteRenderer>().color = Color.cyan;
                    terrain[i, j].GetComponent<TileInfo>().tile = new House(i, j);
                }
                else
                {
                    Debug.Log(((int)(current_pixel_colour.r * 1000)) + "," + ((int)(current_pixel_colour.g * 1000)) + "," + ((int)(current_pixel_colour.b * 1000)));
                }
                Debug.Log("Tile size: " + terrain[i, j].GetComponent<SpriteRenderer>().size);
            }
        }

        /* Count total number of social buildings, and calculate number to set to child only */
        if (social_ls.Count <= 1) return null; /* ie error */
        int num_child_socials = (int) Mathf.Ceil(social_ls.Count * Parameters.Instance.PERCENT_CHILD_SOCIAL_BUILDINGS);
        if (num_child_socials == 0 || social_ls.Count - num_child_socials == 0) return null; /* ie error */

        /* Shuffle the list */
        for (int i=0; i<social_ls.Count; ++i)
        {
            int randomIndex = Random.Range(0, social_ls.Count);
            Social temp = social_ls[i];
            social_ls[i] = social_ls[randomIndex];
            social_ls[randomIndex] = temp;
        }
        
        /* Set the first 'num_child_social' many Socials to forChildren */
        for (int i=0; i<num_child_socials; ++i)
        {
            social_ls[i].setForChildren(true);
        }

        return terrain;
    }

    private bool colour_equals(Color c1, Color c2)
    {
        return ((int)(c1.r * 1000) == (int)(c2.r * 1000))
            && ((int)(c1.g * 1000) == (int)(c2.g * 1000))
            && ((int)(c1.b * 1000) == (int)(c2.b * 1000));
    }
}
