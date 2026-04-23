using UnityEngine;

// Вешай на каждого врага вместе с EnemyAI.
// Хранит стартовую позицию и HP. При смерти деактивирует объект.
// При полночи (GameClock.OnMidnight) — восстанавливает и активирует снова.
[RequireComponent(typeof(HealthSystem))]
public class EnemyRespawner : MonoBehaviour
{
    [SerializeField] private bool respawnable = true;

    private Vector3 spawnPosition;
    private HealthSystem health;
    private bool isDead;

    void Start()
    {
        spawnPosition = transform.position;
        health = GetComponent<HealthSystem>();

        // Подписываемся в Start (не OnEnable) — слушатель остаётся активным
        // даже когда gameObject неактивен (SetActive false не удаляет подписку)
        GameClock.Instance?.OnMidnight.AddListener(OnMidnight);
    }

    void OnDestroy()
    {
        GameClock.Instance?.OnMidnight.RemoveListener(OnMidnight);
    }

    // Вызывается из EnemyAI.DyingRoutine вместо Destroy
    public void TriggerDeath()
    {
        isDead = true;
        if (respawnable)
            gameObject.SetActive(false);
        else
            Destroy(gameObject);
    }

    private void OnMidnight()
    {
        if (!isDead || !respawnable) return;
        isDead = false;
        transform.position = spawnPosition;
        health.Heal(health.MaxHealth);
        gameObject.SetActive(true);
    }
}
