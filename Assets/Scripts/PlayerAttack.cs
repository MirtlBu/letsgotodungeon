using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float combatIdleTimeout = 3f;

    private float cooldownTimer;
    private float combatIdleTimer;
    private PlayerStats stats;
    private Stats combat;
    private Animator animator;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<Stats>();
        animator = GetComponent<Animator>();

        var health = GetComponent<HealthSystem>();
        health?.OnDamaged.AddListener(OnHit);
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (combatIdleTimer > 0f)
        {
            combatIdleTimer -= Time.deltaTime;
            if (combatIdleTimer <= 0f)
                animator?.SetBool("inCombat", false);
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame && cooldownTimer <= 0f)
            PerformAttack();
    }

    private void OnHit()
    {
        SetCombatActive();
    }

    private void SetCombatActive()
    {
        animator?.SetBool("inCombat", true);
        combatIdleTimer = combatIdleTimeout;
    }

    private void FaceNearestEnemy()
    {
        Collider[] nearby = Physics.OverlapSphere(transform.position, attackRange * 1.5f, LayerMask.GetMask("Enemy"));
        float bestDist = float.MaxValue;
        Transform nearest = null;
        foreach (var col in nearby)
        {
            float d = Vector3.Distance(transform.position, col.transform.position);
            if (d < bestDist) { bestDist = d; nearest = col.transform; }
        }
        if (nearest == null) return;
        Vector3 dir = nearest.position - transform.position;
        dir.y = 0f;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dir);
    }

    private void PerformAttack()
    {
        cooldownTimer = combat.attackCooldown;

        FaceNearestEnemy();
        animator?.SetTrigger("attack");
        SetCombatActive();

        Collider[] hits = Physics.OverlapSphere(
            transform.position + transform.forward * (attackRange * 0.5f),
            attackRange,
            LayerMask.GetMask("Enemy"));

        foreach (var hit in hits)
        {
            var health = hit.GetComponent<HealthSystem>();
            if (health == null) continue;

            float damage = stats != null ? stats.Damage : 25f;

            // Backstab: attack from behind
            Vector3 dirToPlayer = (transform.position - hit.transform.position).normalized;
            float dot = Vector3.Dot(hit.transform.forward, dirToPlayer);
            float angle = Mathf.Acos(Mathf.Clamp(dot, -1f, 1f)) * Mathf.Rad2Deg;
            bool isBackstab = angle > (180f - combat.backstabAngle * 0.5f);
            if (isBackstab) damage *= 2f;

            // Crit
            bool isCrit = Random.value < stats.CritChance;
            if (isCrit) damage *= stats.CritMultiplier;

            health.TakeDamage(damage);
            DamageNumbersUI.Instance?.Show(damage, hit.transform.position + Vector3.up * 1.5f, isCrit);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange);
    }
}
