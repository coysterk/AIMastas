using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerlinNoiseMapGenerator : MonoBehaviour
{
    Dictionary<int, GameObject> tileSet;
    Dictionary<int, GameObject> tileGroups;

    public GameObject prefabGrass; //Grass tile.
    public GameObject prefabStone; //Stone tile.
    public GameObject prefabWall; //Wall tile.

    public int mapWidth = 40; //Width of map.
    public int mapHeight = 20;

    public List<List<int>> noiseGrid = new List<List<int>>();
    List<List<GameObject>> tileGrid = new List<List<GameObject>>();

    float magnification = 4.0f;
    public int randomXOffset;
    public int randomYOffset;

    void Start()
    {
        CreateTileSet();
        CreateTileGroups();
        GenerateValidMap();
    }

    void CreateTileSet()
    {
        tileSet = new Dictionary<int, GameObject>(); //Dictonary to hold tiles.
        tileSet.Add(0, prefabStone);
        tileSet.Add(1, prefabGrass);
        tileSet.Add(2, prefabWall);
    }

    void CreateTileGroups()
    {
        tileGroups = new Dictionary<int, GameObject>();
        foreach (KeyValuePair<int, GameObject> prefabPair in tileSet)
        {
            GameObject tileGroup = new GameObject(prefabPair.Value.name);
            tileGroup.transform.parent = gameObject.transform;
            tileGroup.transform.localPosition = new Vector3(0, 0, 0);
            tileGroups.Add(prefabPair.Key, tileGroup);
        }
    }

    void GenerateValidMap()
    {
        bool isValid = false;
        int attempts = 0; //Counter for map attempts
        int maxAttempts = 1000;

        while (!isValid && attempts < maxAttempts) //If current map not valid and below max attempts.
        {
            attempts++; //Increment attempt counter.

            randomXOffset = Random.Range(-10000, 10000); //Make random x offset
            randomYOffset = Random.Range(-10000, 10000); //Make random y offset

            isValid = GenerateAndValidateNoiseData();
        }

        if (attempts >= maxAttempts)
        {
            Debug.LogError("Could not find a valid map after " + maxAttempts + " attempts! Your magnification might be too high or low.");
        }
        else
        {
            Debug.Log("Success! Valid map found after " + attempts + " attempts.");
            BuildMapVisuals();
        }
    }

    bool GenerateAndValidateNoiseData()
    {
        noiseGrid.Clear(); //Start a fresh grid.

        for (int x = 0; x < mapWidth; x++) //Generate the math grid and do the quick Column Check.
        {
            noiseGrid.Add(new List<int>());

            int stoneCountInColumn = 0; //Track how many stone tiles are in this specific column.

            for (int y = 0; y < mapHeight; y++)
            {
                int tileId;

                if (y == 0 || y == mapHeight - 1)
                {
                    tileId = 2; //Walls on top and bottom.
                }
                else
                {
                    tileId = GetIdUsingPerlin(x, y);
                }

                noiseGrid[x].Add(tileId);

                if (tileId == 0) //If it's a stone tile (0) then count it.
                {
                    stoneCountInColumn++;
                }
            }

            if (stoneCountInColumn < 3) //Checks if there is enough stone tiles per row to make the map more open.
            {
                return false;
            }
        }

        return CheckPathExists(); //Run the BFS algorithm to ensure the stones actually connect from left to right.
    }

    bool CheckPathExists()
    {
        Queue<Vector2Int> queue = new Queue<Vector2Int>();
        bool[,] visited = new bool[mapWidth, mapHeight];

        //Find all starting stone tiles on the left edge (x = 0).
        for (int y = 1; y < mapHeight - 1; y++)
        {
            if (noiseGrid[0][y] == 0) //0 is Stone.
            {
                queue.Enqueue(new Vector2Int(0, y));
                visited[0, y] = true;
            }
        }

        //Perform Breadth-First Search (BFS)
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

        while (queue.Count > 0)
        {
            Vector2Int current = queue.Dequeue();

            if (current.x == mapWidth - 1) //Did we reach the right edge of the map?
            {
                return true; //A valid continuous path exists.
            }

            foreach (Vector2Int dir in directions) //Check all 4 neighbors.
            {
                int nextX = current.x + dir.x;
                int nextY = current.y + dir.y;

                //Make sure the neighbor is inside the map boundaries.
                if (nextX >= 0 && nextX < mapWidth && nextY >= 0 && nextY < mapHeight)
                {
                    //If it is a stone (0) and we haven't visited it yet.
                    if (noiseGrid[nextX][nextY] == 0 && !visited[nextX, nextY])
                    {
                        visited[nextX, nextY] = true;
                        queue.Enqueue(new Vector2Int(nextX, nextY));
                    }
                }
            }
        }

        return false; //If the queue empties out and we never hit the right edge, the path is blocked.
    }

    void BuildMapVisuals()
    {
        tileGrid.Clear();

        for (int x = 0; x < mapWidth; x++)
        {
            tileGrid.Add(new List<GameObject>());
            for (int y = 0; y < mapHeight; y++)
            {
                int tileId = noiseGrid[x][y];
                CreateTile(tileId, x, y);
            }
        }
    }

    int GetIdUsingPerlin(int x, int y)
    {
        float rawPerlin = Mathf.PerlinNoise(
            (x - randomXOffset) / magnification,
            (y - randomYOffset) / magnification
        );

        float clampPerlin = Mathf.Clamp01(rawPerlin);
        float scaledPerlin = clampPerlin * (tileSet.Count - 1);

        if (scaledPerlin == (tileSet.Count - 1))
        {
            scaledPerlin = (tileSet.Count - 2);
        }

        return Mathf.FloorToInt(scaledPerlin);
    }

    void CreateTile(int tileId, int x, int y)
    {
        GameObject tilePrefab = tileSet[tileId];
        GameObject tileGroup = tileGroups[tileId];
        GameObject tile = Instantiate(tilePrefab, tileGroup.transform);

        tile.name = string.Format("tile_x{0}_y{1}", x, y);
        tile.transform.localPosition = new Vector3(x, y, 0);

        tileGrid[x].Add(tile);
    }
}