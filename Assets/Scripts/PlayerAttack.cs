using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float spamCooldown = 0.2f;
    [SerializeField] private float impactDuration = 0.35f;
    [SerializeField] private float critImpactBonus = 0.4f;
    [SerializeField] private float combatIdleTimeout = 3f;
    [SerializeField] private AudioClip attackSound;
    [SerializeField] [Range(0f, 1f)] private float attackVolume = 0.8f;

    private float cooldownTimer;
    private float combatIdleTimer;
    private float hitStunTimer;
    private PlayerStats stats;
    private Stats combat;
    private Animator animator;
    private CharacterMovement movement;

    void Start()
    {
        stats = GetComponent<PlayerStats>();
        combat = GetComponent<Stats>();
        animator = GetComponent<Animator>();
        movement = GetComponent<CharacterMovement>();

        var health = GetComponent<HealthSystem>();
        health?.OnDamaged.AddListener(OnHit);
        health?.OnCritDamaged.AddListener(OnCritHit);
    }

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (hitStunTimer > 0f)
        {
            hitStunTimer -= Time.deltaTime;
            if (hitStunTimer <= 0f)
                movement.IsLocked = false;
            return;
        }

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
        animator?.Play("impact", 0, 0f);
        hitStunTimer = impactDuration;
        if (movement != null) movement.IsLocked = true;
    }

    private void OnCritHit()
    {
        hitStunTimer += critImpactBonus;
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
        cooldownTimer = spamCooldown;

        FaceNearestEnemy();
        animator?.Play("attack", 0, 0f);
        SetCombatActive();
        if (attackSound != null)
            AudioSource.PlayClipAtPoint(attackSound, transform.position, attackVolume);

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

            health.TakeDamage(damage, isCrit);
            DamageNumbersUI.Instance?.Show(damage, hit.transform.position + Vector3.up * 1.5f, isCrit);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward * (attackRange * 0.5f), attackRange);
    }
}
