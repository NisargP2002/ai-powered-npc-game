using UnityEngine;
using UnityEngine.AI;

public class NPCStop : MonoBehaviour
{
    public float turnSpeed = 3f;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform playerTransform;
    private bool isFacingPlayer = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
    }

    public void FacePlayer(Transform player)
    {
        playerTransform = player;
        isFacingPlayer = true;

        if (agent != null)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        if (animator != null)
            animator.speed = 0f;
    }

    public void StopFacingPlayer()
    {
        isFacingPlayer = false;
        playerTransform = null;

        if (agent != null)
            agent.isStopped = false;

        if (animator != null)
            animator.speed = 1f;
    }

    public void PlayAttack()
    {
        if (animator != null)
        {
            animator.speed = 1f;
            animator.SetTrigger("Attack");
        }
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