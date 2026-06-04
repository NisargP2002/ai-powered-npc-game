using UnityEngine;
using UnityEngine.AI;

// Makes Eolindra walk between waypoints in a loop using NavMesh.
public class NPCPatrol : MonoBehaviour
{
    // Drag your Waypoint objects into this list in the Inspector
    public Transform[] waypoints;

    // How long Eolindra waits at each waypoint before moving on
    public float waitTime = 3f;

    private NavMeshAgent agent;
    private int currentWaypoint = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("NPCPatrol: No waypoints assigned!");
            return;
        }
        agent.SetDestination(waypoints[0].position);
    }

    void Update()
    {
        if (waypoints.Length == 0) return;

        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            if (!isWaiting)
            {
                isWaiting = true;
                waitTimer = 0f;
            }
            else
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTime)
                {
                    isWaiting = false;
                    currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
                    agent.SetDestination(waypoints[currentWaypoint].position);
                }
            }
        }
    }
}