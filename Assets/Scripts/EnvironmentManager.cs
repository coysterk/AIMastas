using Unity.VisualScripting;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public int zombieAmount = 20;
    public float maxEpisodeTime = 5f;

    public GameObject zombiePrefab;
    public PerlinNoiseMapGenerator generator;
    private Transform spawnPoint;

    private float timer;
    private ZombieAgent[] agents;

    private void Start()
    {
        if (generator == null)
        {
            Debug.LogError("EnvironmentManager: generator is not assigned!");
            enabled = false;
            return;
        }

        generator.GenerateValidMap();

        var startObj = GameObject.FindGameObjectWithTag("Start");
        if (startObj == null)
        {
            Debug.LogError("EnvironmentManager: No GameObject with tag 'start' found. Cannot spawn zombies.");
            enabled = false;
            return;
        }

        spawnPoint = startObj.transform;

        if (zombiePrefab == null)
        {
            Debug.LogError("EnvironmentManager: zombiePrefab is not assigned!");
            enabled = false;
            return;
        }

        timer = 0f;
        SpawnZombies();
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer > maxEpisodeTime)
        {
            ResetALL();
        }
    }

    public void ResetALL()
    {
        timer = 0f;

        generator.GenerateValidMap();

        spawnPoint = GameObject.FindGameObjectWithTag("Start").transform;

        for (int i = 0; i < agents.Length; i++)
        {
            agents[i].transform.position = spawnPoint.position;
            agents[i].ResetAgentState();
        }
    }

    public void SpawnZombies()
    {
        Debug.Log($"SpawnZombies called. zombieAmount={zombieAmount}, spawnPoint={(spawnPoint ? spawnPoint.name : "NULL")}, prefab={(zombiePrefab ? zombiePrefab.name : "NULL")}");

        agents = new ZombieAgent[zombieAmount];

        for (int i = 0; i < zombieAmount; i++)
        {
            GameObject z = Instantiate(zombiePrefab, spawnPoint.position, Quaternion.identity);
            z.name = $"Zombie_{i}";
            agents[i] = z.GetComponent<ZombieAgent>();

            if (agents[i] == null)
                Debug.LogError("Spawned zombie prefab is missing ZombieAgent component!");
        }
    }
}
