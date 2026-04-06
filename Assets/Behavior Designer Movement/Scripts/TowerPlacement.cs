using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using UnityEngine;

public class TowerPlacement : MonoBehaviour
{
    Dictionary<int, GameObject> towerDictionary = new Dictionary<int, GameObject>();
    public PerlinNoiseMapGenerator perlinNoiseMapGenerator; //Reference to your map generator script.
    public GameObject prefab_Cannon;
    public GameObject prefab_Balista;
    public GameObject prefab_MachineGun;
    public GameObject prefab_CrystalBall;

    private int activeTowerID = 0; //The ID of the tower the player currently has equipped.
    private bool[,] occupiedTiles; //A 2D grid to remember where we put towers.

    //Array holding the new Input System Key references for 1 through 9.
    private Key[] numberKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9 };

    void Start()
    {
        towerDictionary = new Dictionary<int, GameObject>(); //Dictionary to store all the towers
        towerDictionary.Add(0, prefab_Cannon);
        towerDictionary.Add(1, prefab_Balista);
        towerDictionary.Add(2, prefab_MachineGun);
        towerDictionary.Add(3, prefab_CrystalBall);
    }

    void Update()
    {
        if (Keyboard.current != null) //Safety check to ensure keyboard and mouse exist before reading from them.
        {
            HandleTowerSelection();
        }

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            AttemptPlacement();
        }
    }

    void HandleTowerSelection()
    {
        for (int i = 0; i < towerDictionary.Count; i++)
        {
            //Check if 'i' is within our numberKeys array to prevent out-of-bounds errors, then check if that specific number key was pressed.
            if (i < numberKeys.Length && Keyboard.current[numberKeys[i]].wasPressedThisFrame)
            {
                if (towerDictionary.ContainsKey(i))
                {
                    activeTowerID = i;
                    Debug.Log("Equipped Tower: " + towerDictionary[activeTowerID].name); //Makes Debugging easier, and seeing what tower is selected in testing.
                }
            }
        }
    }

    void AttemptPlacement()
    {
        if (towerDictionary.Count == 0) { 
            return; 
        }

        if (occupiedTiles == null) //If no tower on the tile.
        {
            occupiedTiles = new bool[perlinNoiseMapGenerator.map_width, perlinNoiseMapGenerator.map_height];
        }

        //Get the mouse position with proper Z-depth.
        Vector2 mousePosScreen = Mouse.current.position.ReadValue();
        float distanceFromCameraToMap = Mathf.Abs(Camera.main.transform.position.z);
        Vector3 mousePosWithDepth = new Vector3(mousePosScreen.x, mousePosScreen.y, distanceFromCameraToMap);

        //Get the World position of the mouse.
        Vector3 worldMousePos = Camera.main.ScreenToWorldPoint(mousePosWithDepth);

        //Convert World position to the Map's Local position to align the grid no mater where the map is compared to the camera.
        Vector3 localMousePos = perlinNoiseMapGenerator.transform.InverseTransformPoint(worldMousePos);

        //Round the LOCAL coordinates to get the correct array index
        int gridX = Mathf.RoundToInt(localMousePos.x);
        int gridY = Mathf.RoundToInt(localMousePos.y);

        //Check if the click is inside the actual map boundaries
        if (gridX >= 0 && gridX < perlinNoiseMapGenerator.map_width && gridY >= 0 && gridY < perlinNoiseMapGenerator.map_height)
        {
            int clickedTileID = perlinNoiseMapGenerator.noise_grid[gridX][gridY];

            if (clickedTileID == 1) //Is it Grass (ID 1)?
            {
                if (occupiedTiles[gridX, gridY] == false)
                {
                    GameObject towerToSpawn = towerDictionary[activeTowerID];

                    if (towerToSpawn != null)
                    {
                        Vector3 localSpawnPos = new Vector3(gridX, gridY, 0f); //Create the local spawn position
                        Vector3 worldSpawnPos = perlinNoiseMapGenerator.transform.TransformPoint(localSpawnPos); //Convert it back to World Space so it spawns exactly on the visual tile.
                        worldSpawnPos.z = -1f; //Push it forward so it renders on top of the map
                        Instantiate(towerToSpawn, worldSpawnPos, Quaternion.identity, perlinNoiseMapGenerator.transform); //Spawn the tower AND parent it to the map so they stay grouped together.
                        occupiedTiles[gridX, gridY] = true; //Lock the tile.
                    }
                }
                else
                {
                    Debug.Log("Invalid placement: A tower is already built here!"); //Throws if there is already a tower in the tile you are placing.
                }
            }
            else
            {
                Debug.Log($"Invalid placement: You clicked ({gridX}, {gridY}). That tile ID is {clickedTileID}, which is not grass!"); //Throws when placing tower not on grass.
            }
        }
    }
}