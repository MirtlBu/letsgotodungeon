using UnityEngine;
using UnityEngine.Events;

public class HealthSystem : MonoBehaviour
{
    private float maxHealth = 100f;
    private float currentHealth;

    public float HealthPercent => currentHealth / maxHealth;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    public UnityEvent OnDeath;
    public UnityEvent OnDamaged;

    void Awake()
    {
        var s = GetComponent<Stats>();
        if (s != null) maxHealth = s.maxHealth;
        currentHealth = maxHealth;
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Max(0f, currentHealth - amount);
        if (currentHealth <= 0f)
            OnDeath?.Invoke();
        else
            OnDamaged?.Invoke();
    }

    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
    }
}
