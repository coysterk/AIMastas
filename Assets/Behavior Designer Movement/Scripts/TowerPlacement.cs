using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class TowerPlacement : MonoBehaviour
{
    //References
    public PerlinNoiseMapGenerator perlinNoiseMapGenerator;
    public GameObject prefab_Cannon;
    public GameObject prefab_Balista;
    public GameObject prefab_MachineGun;
    //public GameObject prefab_CrystalBall; //Might implement later

    //Tower Limits & UI
    public int maxTowers = 10;
    private int currentTowers = 0;
    public TextMeshProUGUI towerCounterText;
    public float dropCooldown = 2f;
    private GameObject heldTower;

    private Dictionary<int, GameObject> towerDictionary = new Dictionary<int, GameObject>(); //Dictonary to store the towers.
    private bool[,] occupiedTiles; //Keeps track of tiles with towers on them.
    private int activeTowerID = 0; //The ID of the tower being placed.
    private Key[] numberKeys = { Key.Digit1, Key.Digit2, Key.Digit3, Key.Digit4, Key.Digit5, Key.Digit6, Key.Digit7, Key.Digit8, Key.Digit9 }; //Number bar to select towers.

    void Awake()
    {
        towerDictionary = new Dictionary<int, GameObject>(); //Dictonary to store the towers.
        towerDictionary.Add(0, prefab_Cannon);
        towerDictionary.Add(1, prefab_Balista);
        towerDictionary.Add(2, prefab_MachineGun);
        //towerDictionary.Add(3, prefab_CrystalBall); //Might implement later
    }

    void Start()
    {
        UpdateUI(); //Places the tower counter on the top left of the screen.
    }

    void Update()
    {
        if (Keyboard.current != null) HandleTowerSelection();

        //Map our inputs for easier reading
        bool leftClick = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        bool rightClick = Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame;
        bool isShiftPressed = Keyboard.current != null && Keyboard.current.shiftKey.isPressed;

        if (heldTower != null) //Are we currently holding a tower?
        {
            Vector3 worldMouse = GetMouseWorldPosition();
            heldTower.transform.position = new Vector3(worldMouse.x, worldMouse.y, -2f);

            if (leftClick)
            {
                DropHeldTower(); //Place it on tile.
            }
            else if (rightClick)
            {
                //Right click while holding a tower deletes it directly from your hand.
                Destroy(heldTower);
                heldTower = null;
                currentTowers--;
                UpdateUI(); //Updates tower counter.
            }
        }
        else //Are our hands empty?
        {
            if (leftClick)
            {
                if (isShiftPressed)
                {
                    TryPickupTower(); //If holding Shift, try to pick it up.
                }
                else
                {
                    AttemptPlacement(); //Otherwise build a new tower.
                }
            }
            else if (rightClick)
            {
                TryDeleteTower(); //Right click an existing tower to delete it.
            }
        }
    }

    void HandleTowerSelection() //Manages what tower is chosen by the number bar.
    {
        for (int i = 0; i < towerDictionary.Count; i++)
        {
            if (i < numberKeys.Length && Keyboard.current[numberKeys[i]]?.wasPressedThisFrame == true)
            {
                if (towerDictionary.ContainsKey(i))
                {
                    activeTowerID = i;
                    Debug.Log("Equipped Tower: " + towerDictionary[activeTowerID].name);
                }
            }
        }
    }

    Vector3 GetMouseWorldPosition() //Tracks the mouse for correct placement.
    {
        Vector2 mousePosScreen = Mouse.current.position.ReadValue();
        float depth = Mathf.Abs(Camera.main.transform.position.z);
        return Camera.main.ScreenToWorldPoint(new Vector3(mousePosScreen.x, mousePosScreen.y, depth));
    }

    void UpdateUI() //Updates the counter for the tower.
    {
        if (towerCounterText != null)
        {
            towerCounterText.text = currentTowers + " / " + maxTowers;
        }
    }

    bool TryPickupTower() //Picks up the tower from that tile and allows it to move with the mouse.
    {
        Vector3 worldPos = GetMouseWorldPosition();
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Tower")) //If mouse is over a tower.
        {
            heldTower = hit.collider.gameObject;

            Vector3 localPos = perlinNoiseMapGenerator.transform.InverseTransformPoint(heldTower.transform.position);
            int gridX = Mathf.RoundToInt(localPos.x);
            int gridY = Mathf.RoundToInt(localPos.y);

            if (occupiedTiles != null) occupiedTiles[gridX, gridY] = false; //Sets the tile the tower was on to avaliable.

            SetTowerActiveState(heldTower, false); //Disables tower's function.
            return true;
        }
        return false;
    }

    void TryDeleteTower() //If there is a tower on the tile you want gone.
    {
        Vector3 worldPos = GetMouseWorldPosition();
        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Tower")) //If mouse is over a tower.
        {
            GameObject towerToDelete = hit.collider.gameObject;

            //Figure out what tile it was sitting on, and mark that tile as free.
            Vector3 localPos = perlinNoiseMapGenerator.transform.InverseTransformPoint(towerToDelete.transform.position);
            int gridX = Mathf.RoundToInt(localPos.x);
            int gridY = Mathf.RoundToInt(localPos.y);

            if (occupiedTiles != null && gridX >= 0 && gridX < perlinNoiseMapGenerator.map_width && gridY >= 0 && gridY < perlinNoiseMapGenerator.map_height) //If tower is in the map's bounds.
            {
                occupiedTiles[gridX, gridY] = false; //Sets the tile the tower was on to avaliable.
            }

            Destroy(towerToDelete);
            currentTowers--; //Decrement tower counter.
            UpdateUI(); //Update the visual counter.
        }
    }

    void DropHeldTower() //Allows picked up towers to be placed back down as long as there is grass or no other tower in the same tile.
    {
        Vector3 localMousePos = perlinNoiseMapGenerator.transform.InverseTransformPoint(GetMouseWorldPosition());
        int gridX = Mathf.RoundToInt(localMousePos.x);
        int gridY = Mathf.RoundToInt(localMousePos.y);

        if (gridX >= 0 && gridX < perlinNoiseMapGenerator.map_width && gridY >= 0 && gridY < perlinNoiseMapGenerator.map_height) //If tower is in the map's bounds.
        {
            if (perlinNoiseMapGenerator.noise_grid[gridX][gridY] == 1 && occupiedTiles[gridX, gridY] == false) //If tile is grass and has no tower.
            {
                Vector3 localSpawnPos = new Vector3(gridX, gridY, 0f);
                Vector3 worldSpawnPos = perlinNoiseMapGenerator.transform.TransformPoint(localSpawnPos);
                worldSpawnPos.z = -1f;

                heldTower.transform.position = worldSpawnPos;
                occupiedTiles[gridX, gridY] = true; //Sets tile to occupied.

                SetTowerActiveState(heldTower, true); //Enables tower.
                heldTower = null; //Allows to pick up other towers.
            }
            else
            {
                Debug.Log("Cannot drop here! Spot is taken or not grass.");
            }
        }
    }

    void SetTowerActiveState(GameObject tower, bool isActive) //When tower is picked up, it is disabled, when placed back down it is enabled. Uses this to help.
    {
        if (tower.TryGetComponent(out CannonTower cannon)) 
        { 
            cannon.enabled = isActive;
            if (isActive) cannon.fireCooldown = dropCooldown; 
        }
        if (tower.TryGetComponent(out BallistaTower ballista)) 
        { 
            ballista.enabled = isActive; 
            if (isActive) ballista.fireCooldown = dropCooldown; 
        }
        if (tower.TryGetComponent(out MachineGunTower mg)) 
        { 
            mg.enabled = isActive; 
            if (isActive) mg.fireCooldown = dropCooldown; 
        }
    }

    void AttemptPlacement() //Checks number of towers, tile type, and if there is another tower on that tile before placeing a tower.
    {
        if (towerDictionary.Count == 0) return; //If there is no towers in the dictonary, return.

        if (currentTowers >= maxTowers) //If the tower cap is reached, send debug message and return.
        {
            Debug.Log("Max towers reached! Delete or move an existing tower instead.");
            return;
        }

        if (occupiedTiles == null)
        {
            occupiedTiles = new bool[perlinNoiseMapGenerator.map_width, perlinNoiseMapGenerator.map_height];
        }

        Vector3 localMousePos = perlinNoiseMapGenerator.transform.InverseTransformPoint(GetMouseWorldPosition());
        int gridX = Mathf.RoundToInt(localMousePos.x);
        int gridY = Mathf.RoundToInt(localMousePos.y);

        if (gridX >= 0 && gridX < perlinNoiseMapGenerator.map_width && gridY >= 0 && gridY < perlinNoiseMapGenerator.map_height) //If in the bounds of the map.
        {
            if (perlinNoiseMapGenerator.noise_grid[gridX][gridY] == 1) //If the tile is grass.
            {
                if (occupiedTiles[gridX, gridY] == false) //If tile doesn't have any other tower.
                {
                    GameObject towerToSpawn = towerDictionary[activeTowerID];

                    if (towerToSpawn != null) //If there is a tower picked.
                    {
                        Vector3 localSpawnPos = new Vector3(gridX, gridY, 0f);
                        Vector3 worldSpawnPos = perlinNoiseMapGenerator.transform.TransformPoint(localSpawnPos);
                        worldSpawnPos.z = -1f;

                        Instantiate(towerToSpawn, worldSpawnPos, Quaternion.identity, perlinNoiseMapGenerator.transform); //Spawn tower.

                        occupiedTiles[gridX, gridY] = true; //Makes tile occupied.

                        currentTowers++; //Increases tower count.
                        UpdateUI(); //Updates visual counter.
                    }
                }
            }
        }
    }
}