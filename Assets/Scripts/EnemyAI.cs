using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float patrolDistance = 4f;
    [SerializeField] private float waitTime = 1f;

    [Header("Combat")]
    [SerializeField] private float aggroRadius = 6f;
    [SerializeField] private float attackRadius = 1.5f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private int coinReward = 10;

    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;

    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 currentPatrolTarget;
    private float waitTimer;
    private float attackTimer;
    private int attackIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;

        pointA = transform.position + transform.forward * patrolDistance;
        pointB = transform.position - transform.forward * patrolDistance;
        currentPatrolTarget = pointA;
        agent.SetDestination(currentPatrolTarget);
        waitTimer = waitTime;

        var health = GetComponent<HealthSystem>();
        health?.OnDeath.AddListener(OnDeath);
        EnemyHealthBarUI.Instance?.Register(health, transform);
    }

    void Update()
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        attackTimer -= Time.deltaTime;

        switch (state)
        {
            case State.Patrol:
                UpdatePatrol();
                if (dist <= aggroRadius)
                    state = State.Chase;
                break;

            case State.Chase:
                agent.SetDestination(player.position);
                if (dist <= attackRadius)
                    state = State.Attack;
                else if (dist > aggroRadius * 1.5f)
                    ReturnToPatrol();
                break;

            case State.Attack:
                agent.SetDestination(transform.position);
                FacePlayer();
                if (dist > attackRadius * 1.2f)
                    state = State.Chase;
                else if (attackTimer <= 0f && !IsPlayingAttack())
                    PerformAttack();
                break;
        }

        float speed = 0f;
        if (agent.desiredVelocity.magnitude > 0.1f)
            speed = state == State.Chase ? 1f : 0.5f;
        animator?.SetFloat("speed", speed);
    }

    private void UpdatePatrol()
    {
        if (agent.pathPending) return;
        if (!agent.hasPath || agent.remainingDistance <= agent.stoppingDistance + 0.1f)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                currentPatrolTarget = currentPatrolTarget == pointA ? pointB : pointA;
                agent.SetDestination(currentPatrolTarget);
                waitTimer = waitTime;
            }
        }
    }

    private void ReturnToPatrol()
    {
        state = State.Patrol;
        agent.SetDestination(currentPatrolTarget);
        waitTimer = waitTime;
    }

    private bool IsPlayingAttack()
    {
        if (animator == null) return false;
        var info = animator.GetCurrentAnimatorStateInfo(0);
        return info.IsName("skeleton_attack1") || info.IsName("skeleton_attack2");
    }

    private void FacePlayer()
    {
        Vector3 dir = (player.position - transform.position);
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void PerformAttack()
    {
        attackTimer = attackCooldown;
        animator?.ResetTrigger("attack");
        animator?.SetInteger("attackIndex", attackIndex);
        animator?.SetTrigger("attack");
        attackIndex = 1 - attackIndex;
        player.GetComponent<HealthSystem>()?.TakeDamage(attackDamage);
    }

    private void OnDeath()
    {
        EnemyHealthBarUI.Instance?.Unregister(GetComponent<HealthSystem>());
        CoinCounter.Instance?.Add(coinReward);
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Vector3 a = transform.position + transform.forward * patrolDistance;
        Vector3 b = transform.position - transform.forward * patrolDistance;
        Gizmos.DrawSphere(a, 0.2f);
        Gizmos.DrawSphere(b, 0.2f);
        Gizmos.DrawLine(a, b);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, attackRadius);
    }
}
