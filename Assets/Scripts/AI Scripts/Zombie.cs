using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Linq;
using BehaviorDesigner.Runtime.Tasks.Unity.UnityGameObject;

public class ZombieAgent : Agent
{
    public Transform goal;
    public float moveSpeed = 3f;
    private float health = 100f;
    private float prevDist;
    public override void OnEpisodeBegin()
    {

        goal = GameObject.FindWithTag("Goal").GetComponent<Transform>();
        prevDist = Vector3.Distance(transform.position, goal.position);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        Vector3 dir = goal.position - transform.position;

        // Direction & distance
        sensor.AddObservation(dir.normalized);
        sensor.AddObservation(dir.magnitude);

        Vector3[] dirs = {
            transform.forward,
            -transform.forward,
            transform.right,
            -transform.right,
            (transform.forward + transform.right).normalized,
            (transform.forward - transform.right).normalized
        };

        foreach (var d in dirs)
        {
            if (Physics.Raycast(transform.position, d, out RaycastHit hit, 2f))
                sensor.AddObservation(hit.collider.CompareTag("Wall") ? 1f : 0f);
            else
                sensor.AddObservation(0f);
        }

        // swarm awareness
        Collider[] nearby = Physics.OverlapSphere(transform.position, 2f);
        int zombieCount = 0;

        foreach (var c in nearby)
        {
            if (c.CompareTag("Zombie") && c.gameObject != gameObject)
                zombieCount++;
        }

        sensor.AddObservation(zombieCount);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float x = actions.ContinuousActions[0];
        float z = actions.ContinuousActions[1];

        Vector3 move = new Vector3(x, 0, z);

        // separation
        Collider[] nearby = Physics.OverlapSphere(transform.position, 1.5f);
        Vector3 separation = Vector3.zero;

        foreach (var a in nearby)
        {
            if (a.CompareTag("Zombie") && a.gameObject != gameObject)
            {
                separation += (transform.position - a.transform.position);
            }
        }

        Vector3 finalMove = move + separation * 0.5f;

        transform.Translate(finalMove * moveSpeed * Time.deltaTime);

        float dist = Vector3.Distance(transform.position, goal.position);
        float progress = prevDist - dist;          // positive if we got closer
        AddReward(progress * 0.5f);                // tune 0.1–1.0
        prevDist = dist;

        // small time penalty to encourage speed
        AddReward(-0.001f);

        // Reward reach player
        if (dist < 1.5f)
        {
            AddReward(1f);
            EndEpisode();
        }

        int neighbors = 0;
        foreach (var c in nearby)
            if (c.CompareTag("Zombie") && c.gameObject != gameObject)
                neighbors++;

        if (neighbors > 3) AddReward(-0.01f);

        // End if too long
        if (StepCount > 3000)
            EndEpisode();
    }

    public void ResetAgentState()
    {
        health = 100f;

        gameObject.SetActive(true);
    }

    public void TakeDamage(int dmg, int turretType)
    {
        AddReward(-0.2f);

        if(turretType == 2)
        {
            AddReward(-0.4f);
        }

        health -= dmg;

        if (health <= 0)
        {
            AddReward(-0.5f);
            gameObject.SetActive(false);
        }
    }
}
