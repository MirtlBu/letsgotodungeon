using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private float backstabAngle = 120f;

    private float cooldownTimer;
    private PlayerStats stats;
    private Animator animator;
    private int attackIndex;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;
        if (Keyboard.current.spaceKey.wasPressedThisFrame && cooldownTimer <= 0f)
            PerformAttack();
    }

    private void PerformAttack()
    {
        cooldownTimer = attackCooldown;

        if (animator != null)
        {
            animator.SetInteger("attackIndex", attackIndex);
            animator.SetTrigger("attack");
            attackIndex = 1 - attackIndex; // чередуем attack1 / attack2
        }

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
            bool isBackstab = angle < backstabAngle * 0.5f;
            if (isBackstab) damage *= 2f;

            // Crit
            bool isCrit = Random.value < stats.CritChance;
            if (isCrit) damage *= stats.CritMultiplier;

            health.TakeDamage(damage);
            Debug.Log($"[Attack] Hit {hit.name} for {damage} dmg (backstab={isBackstab}, crit={isCrit})");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange);
    }
}
