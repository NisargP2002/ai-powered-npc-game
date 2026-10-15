using UnityEngine;
using UnityEngine.AI;

public class NPCPatrol : MonoBehaviour
{
    public Transform[] waypoints;
    public float waitTime = 0.5f;

    private NavMeshAgent agent;
    private Animator animator;
    private int currentWaypoint = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private float originalSpeed;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        if (waypoints.Length == 0)
        {
            Debug.LogWarning("NPCPatrol: No waypoints assigned!");
            return;
        }
        originalSpeed = agent.speed;
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
                // Slow down instead of hard stop
                agent.speed = originalSpeed * 0.3f;
                if (animator != null)
                    animator.speed = 0.3f;
            }
            else
            {
                waitTimer += Time.deltaTime;
                if (waitTimer >= waitTime)
                {
                    isWaiting = false;
                    agent.speed = originalSpeed;
                    currentWaypoint = (currentWaypoint + 1) % waypoints.Length;
                    agent.SetDestination(waypoints[currentWaypoint].position);
                    if (animator != null)
                        animator.speed = 1f;
                }
            }
        }
        else
        {
            // Smoothly match animation speed to movement
            float speedRatio = agent.velocity.magnitude / originalSpeed;
            if (animator != null)
                animator.speed = Mathf.Lerp(animator.speed, Mathf.Max(speedRatio, 0.5f), Time.deltaTime * 5f);
        }
    }
}