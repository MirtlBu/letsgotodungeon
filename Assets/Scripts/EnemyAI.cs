using System.Collections;
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

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float knockbackDelay = 0.08f;

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
        health?.OnDamaged.AddListener(OnImpact);
        EnemyHealthBarUI.Instance?.Register(health, transform);
    }

    void Update()
    {
        if (player == null || !agent.enabled) return;

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
        return animator.GetCurrentAnimatorStateInfo(0).IsName("enemy_attack");
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
        animator?.SetTrigger("attack");
        player.GetComponent<HealthSystem>()?.TakeDamage(attackDamage);
        Debug.Log($"[Enemy] {gameObject.name} нанёс {attackDamage} урона игроку");
    }

    private void OnImpact()
    {
        animator?.SetTrigger("impact");
        if (player != null)
            StartCoroutine(KnockbackRoutine());
    }

    private IEnumerator KnockbackRoutine()
    {
        Vector3 dir = (transform.position - player.position);
        dir.y = 0f;
        dir.Normalize();

        yield return new WaitForSeconds(knockbackDelay);

        agent.enabled = false;

        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            float t = 1f - elapsed / knockbackDuration; // затухает к концу
            transform.position += dir * (knockbackForce * t * Time.deltaTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        agent.enabled = true;
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
