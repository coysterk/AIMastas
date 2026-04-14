using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Danndx 2021 (youtube.com/danndx)
From video: youtu.be/qNZ-0-7WuS8
thanks - delete me! :) */

public class PerlinNoiseMapGenerator : MonoBehaviour
{
    Dictionary<int, GameObject> tileset;
    Dictionary<int, GameObject> tile_groups;
    public GameObject prefab_Grass;
    public GameObject prefab_Stone;
    public GameObject prefab_Wall;

    public int map_width = 40;
    public int map_height = 20;

    public List<List<int>> noise_grid = new List<List<int>>();
    List<List<GameObject>> tile_grid = new List<List<GameObject>>();

    // recommend 4 to 20
    float magnification = 4.0f;

    int x_offset = 0; // <- +>
    int y_offset = -5; // v- +^

    void Start()
    {
        CreateTileset();
        CreateTileGroups();
        GenerateMap();
    }

    void CreateTileset()
    {
        /** Collect and assign ID codes to the tile prefabs, for ease of access.
            Best ordered to match land elevation. **/

        tileset = new Dictionary<int, GameObject>();
        tileset.Add(0, prefab_Stone);
        tileset.Add(1, prefab_Grass);
        tileset.Add(2, prefab_Wall);
    }

    void CreateTileGroups()
    {
        /** Create empty gameobjects for grouping tiles of the same type, ie
            forest tiles **/


        tile_groups = new Dictionary<int, GameObject>();
        foreach (KeyValuePair<int, GameObject> prefab_pair in tileset)
        {
            GameObject tile_group = new GameObject(prefab_pair.Value.name);
            tile_group.transform.parent = gameObject.transform;
            tile_group.transform.localPosition = new Vector3(0, 0, 0);
            tile_groups.Add(prefab_pair.Key, tile_group);
        }
    }

    void GenerateMap()
    {
        /** Generate a 2D grid using the Perlin noise fuction, storing it as
           both raw ID values and tile gameobjects **/

        for (int x = 0; x < map_width; x++)
        {
            noise_grid.Add(new List<int>());
            tile_grid.Add(new List<GameObject>());

            for (int y = 0; y < map_height; y++)
            {
                int tile_id;

                // Check if we are at the bottom (y=0) or the top (y=map_height-1)
                if (y == 0 || y == map_height - 1)
                {
                    tile_id = 2;
                }
                else
                {
                    tile_id = GetIdUsingPerlin(x, y);
                }

                noise_grid[x].Add(tile_id);
                CreateTile(tile_id, x, y);
            }
        }
    }

    int GetIdUsingPerlin(int x, int y)
    {
        /** Using a grid coordinate input, generate a Perlin noise value to be
           converted into a tile ID code. Rescale the normalised Perlin value
           to the number of tiles available. **/

        float raw_perlin = Mathf.PerlinNoise(
            (x - x_offset) / magnification,
            (y - y_offset) / magnification
        );

        float clamp_perlin = Mathf.Clamp01(raw_perlin);

        /* We subtract 1 from tileset.Count so that the Perlin noise 
         only picks between Stone (0) and Grass (1), leaving Wall (2) 
         for our manual placement. */
        float scaled_perlin = clamp_perlin * (tileset.Count - 1);

        if (scaled_perlin == (tileset.Count - 1))
        {
            scaled_perlin = (tileset.Count - 2);
        }

        return Mathf.FloorToInt(scaled_perlin);
    }

    void CreateTile(int tile_id, int x, int y)
    {
        /** Creates a new tile using the type id code, group it with common
           tiles, set it's position and store the gameobject. **/

        GameObject tile_prefab = tileset[tile_id];
        GameObject tile_group = tile_groups[tile_id];
        GameObject tile = Instantiate(tile_prefab, tile_group.transform);

        tile.name = string.Format("tile_x{0}_y{1}", x, y);
        tile.transform.localPosition = new Vector3(x, y, 0);

        tile_grid[x].Add(tile);
    }
}