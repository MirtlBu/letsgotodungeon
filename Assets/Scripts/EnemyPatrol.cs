using UnityEngine;
using UnityEngine.AI;

public class EnemyPatrol : MonoBehaviour
{
    [SerializeField] private float patrolDistance = 4f;
    [SerializeField] private float waitTime = 1f;

    private NavMeshAgent agent;
    private Animator animator;
    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 currentTarget;
    private float waitTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        pointA = transform.position + transform.forward * patrolDistance;
        pointB = transform.position - transform.forward * patrolDistance;

        currentTarget = pointA;
        agent.SetDestination(currentTarget);
        waitTimer = waitTime;
    }

    void Update()
    {
        float speed = agent.velocity.magnitude > 0.1f ? 0.5f : 0f;
        animator?.SetFloat("speed", speed);

        if (agent.pathPending) return;

        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                currentTarget = currentTarget == pointA ? pointB : pointA;
                agent.SetDestination(currentTarget);
                waitTimer = waitTime;
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 a = transform.position + transform.forward * patrolDistance;
        Vector3 b = transform.position - transform.forward * patrolDistance;
        Gizmos.DrawSphere(a, 0.2f);
        Gizmos.DrawSphere(b, 0.2f);
        Gizmos.DrawLine(a, b);
    }
}
