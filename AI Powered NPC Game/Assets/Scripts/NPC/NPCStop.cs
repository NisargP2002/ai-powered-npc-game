using UnityEngine;
using UnityEngine.AI;

// Attached to: Eolindra
// Stops her NavMesh movement and smoothly rotates her to face the player.
public class NPCStop : MonoBehaviour
{
    public float turnSpeed = 3f;

    private NavMeshAgent agent;
    private Transform playerTransform;
    private bool isFacingPlayer = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    public void FacePlayer(Transform player)
    {
        playerTransform = player;
        isFacingPlayer = true;
        if (agent != null)
        {
            agent.ResetPath();
            agent.velocity = Vector3.zero;
        }
    }

    public void StopFacingPlayer()
    {
        isFacingPlayer = false;
        playerTransform = null;
    }

    void Update()
    {
        if (!isFacingPlayer || playerTransform == null) return;

        Vector3 direction = playerTransform.position - transform.position;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            Quaternion target = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(
                transform.rotation, target, turnSpeed * Time.deltaTime);
        }
    }
}