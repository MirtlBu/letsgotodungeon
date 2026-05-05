using System.Collections;
using UnityEngine;
using UnityEngine.AI;

// Ranged enemy: держит дистанцию, кидает фаерболы, убегает когда игрок слишком близко.
// Заменяет EnemyAI на ranged-враге. Stats, HealthSystem, NavMeshAgent — те же компоненты.
public class RangedEnemyAI : MonoBehaviour
{
    [Header("Patrol")]
    [SerializeField] private float patrolDistance = 4f;
    [SerializeField] private float waitTime = 1f;

    [Header("Aggro")]
    [SerializeField] private float aggroRadius = 10f;
    [SerializeField] private float rotationSpeed = 5f;
    [SerializeField] private int coinReward = 15;

    [Header("Shooting")]
    [SerializeField] private float preferredRange = 7f;   // идеальная дистанция для стрельбы
    [SerializeField] private float shootRadius = 9f;      // максимальная дистанция стрельбы
    [SerializeField] private float fleeRadius = 3f;       // ближе этого — убегать
    [SerializeField] private Transform fireballSpawn;     // точка спауна фаербола (child-объект)
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float fireballDelay = 0.3f;  // задержка от анимации до спауна
    [SerializeField] private float predictSpeed = 10f;   // должен совпадать со speed в Fireball

    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float knockbackDuration = 0.2f;
    [SerializeField] private float knockbackDelay = 0.08f;
    [SerializeField] private float longKnockbackForce = 6f;
    [SerializeField] private float longKnockbackDuration = 0.4f;

    private enum State { Patrol, Shoot, Retreat }
    private State state = State.Patrol;

    private NavMeshAgent agent;
    private Animator animator;
    private Transform player;
    private Stats stats;

    private Vector3 pointA, pointB, currentPatrolTarget;
    private float waitTimer;
    private float attackTimer;
    private bool isShooting;
    private Vector3 retreatTarget;
    private bool hasRetreatTarget;
    private float retreatRecalcTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        stats = GetComponent<Stats>();
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
                if (dist <= aggroRadius) state = State.Shoot;
                break;

            case State.Shoot:
                if (dist < fleeRadius)
                {
                    retreatRecalcTimer = 0f;
                    hasRetreatTarget = false;
                    state = State.Retreat;
                    break;
                }
                // Стоим на месте, стреляем если в радиусе
                agent.stoppingDistance = 0f;
                agent.SetDestination(transform.position);
                FacePlayer();
                if (dist <= shootRadius && attackTimer <= 0f && !isShooting)
                    StartCoroutine(ShootRoutine());
                break;

            case State.Retreat:
                retreatRecalcTimer -= Time.deltaTime;
                if (!hasRetreatTarget || retreatRecalcTimer <= 0f)
                {
                    retreatTarget = FindRetreatTarget();
                    hasRetreatTarget = true;
                    retreatRecalcTimer = 0.5f;
                    agent.SetDestination(retreatTarget);
                }
                if (dist >= preferredRange)
                {
                    hasRetreatTarget = false;
                    state = State.Shoot;
                }
                break;
        }

        float speed = agent.desiredVelocity.magnitude > 0.1f ? 1f : 0f;
        animator?.SetFloat("speed", speed);
    }

    private IEnumerator ShootRoutine()
    {
        isShooting = true;
        attackTimer = stats.attackCooldown;

        animator?.ResetTrigger("attack");
        animator?.SetTrigger("attack");

        yield return new WaitForSeconds(fireballDelay);

        // Спауним фаербол в направлении игрока
        if (fireballSpawn != null && fireballPrefab != null && player != null)
        {
            // Предсказываем позицию игрока — стреляем туда где он будет когда долетит снаряд
            Vector3 aimTarget = player.position + Vector3.up * 0.5f;
            var cc = player.GetComponent<CharacterController>();
            if (cc != null)
            {
                float travelTime = Vector3.Distance(fireballSpawn.position, aimTarget) / predictSpeed;
                aimTarget += cc.velocity * travelTime;
            }

            Vector3 dir = (aimTarget - fireballSpawn.position).normalized;
            var fb = Instantiate(fireballPrefab, fireballSpawn.position, Quaternion.LookRotation(dir));
            fb.GetComponent<Fireball>()?.Init(dir, stats.damage);
        }

        isShooting = false;
    }

    private Vector3 FindRetreatTarget()
    {
        Vector3 awayDir = (transform.position - player.position).normalized;
        float[] angles = { 0f, 45f, -45f, 90f, -90f, 135f, -135f, 180f };
        float targetDist = preferredRange + 2f;

        foreach (float angle in angles)
        {
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * awayDir;
            Vector3 candidate = transform.position + dir * targetDist;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, 3f, NavMesh.AllAreas))
            {
                var path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                    return hit.position;
            }
        }

        return transform.position; // некуда бежать — стоим
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
        agent.stoppingDistance = 0f;
        agent.SetDestination(currentPatrolTarget);
        waitTimer = waitTime;
    }

    private void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation,
                Quaternion.LookRotation(dir), rotationSpeed * Time.deltaTime);
    }

    private void OnImpact()
    {
        animator?.SetTrigger("impact");
        if (player != null) StartCoroutine(KnockbackRoutine());
    }

    private IEnumerator KnockbackRoutine()
    {
        Vector3 dir = (transform.position - player.position).normalized;
        dir.y = 0f;
        yield return new WaitForSeconds(knockbackDelay);
        agent.enabled = false;
        float elapsed = 0f;
        while (elapsed < knockbackDuration)
        {
            float t = 1f - elapsed / knockbackDuration;
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

        if (player != null)
        {
            Vector3 dir = (transform.position - player.position).normalized;
            dir.y = 0f;
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

        yield return null;
        var info = animator.GetCurrentAnimatorStateInfo(0);
        float duration = info.IsName("enemy_dying") ? info.length : 1.5f;
        yield return new WaitForSeconds(duration + 2f);

        var respawner = GetComponent<EnemyRespawner>();
        if (respawner != null) respawner.TriggerDeath();
        else Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, aggroRadius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, shootRadius);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredRange);
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, fleeRadius);
    }
}
