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
    [SerializeField] private float rotationSpeed = 3f;
    [SerializeField] private int coinReward = 10;

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float knockbackDelay = 0.08f;

    [Header("Long Knockback")]
    [SerializeField] private float longKnockbackForce = 6f;
    [SerializeField] private float longKnockbackDuration = 0.4f;

    private enum State { Patrol, Chase, Attack }
    private State state = State.Patrol;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private Stats combat;

    private Vector3 pointA;
    private Vector3 pointB;
    private Vector3 currentPatrolTarget;
    private float waitTimer;
    private float attackTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        combat = GetComponent<Stats>();
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
        if (player.GetComponent<HealthSystem>()?.CurrentHealth <= 0f)
        {
            animator?.SetFloat("speed", 0f);
            agent.SetDestination(transform.position);
            return;
        }

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
        {
            Quaternion target = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, target, rotationSpeed * Time.deltaTime);
        }
    }

    private void PerformAttack()
    {
        attackTimer = combat.attackCooldown;
        animator?.ResetTrigger("attack");
        animator?.SetTrigger("attack");

        Vector3 dirToPlayer = player.position - transform.position;
        dirToPlayer.y = 0f;
        float angle = Vector3.Angle(transform.forward, dirToPlayer);
        if (angle > 70f) return;

        player.GetComponent<PlayerCombat>()?.RecordAttacker(transform);
        player.GetComponent<HealthSystem>()?.TakeDamage(combat.damage);
        player.GetComponent<PlayerCombat>()?.ApplyKnockback(transform);
        Debug.Log($"[Enemy] {gameObject.name} нанёс {combat.damage} урона игроку");
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
        QuestManager.Instance?.ReportKill();
        StartCoroutine(DyingRoutine());
    }

    private IEnumerator DyingRoutine()
    {
        enabled = false;
        agent.enabled = false;
        foreach (var col in GetComponentsInChildren<Collider>())
            col.enabled = false;

        if (animator) animator.applyRootMotion = false;
        animator?.SetTrigger("dying");

        // Long knockback при смерти
        if (player != null)
        {
            Vector3 dir = transform.position - player.position;
            dir.y = 0f;
            dir.Normalize();

            yield return new WaitForSeconds(knockbackDelay);

            float elapsed = 0f;
            while (elapsed < longKnockbackDuration)
            {
                float t = 1f - elapsed / longKnockbackDuration;
                transform.position += dir * (longKnockbackForce * t * Time.deltaTime);
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Ждём до конца анимации
        yield return null;
        var info = animator.GetCurrentAnimatorStateInfo(0);
        float duration = info.IsName("enemy_dying") ? info.length : 1.5f;
        yield return new WaitForSeconds(duration + 2f);

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
