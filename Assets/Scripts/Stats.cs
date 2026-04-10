using UnityEngine;

/// <summary>
/// Все базовые статы персонажа — игрока или врага.
/// PlayerStats читает отсюда базовые значения и применяет баффы сверху.
/// HealthSystem читает maxHealth отсюда при инициализации.
/// </summary>
public class Stats : MonoBehaviour
{
    [Header("Health")]
    public float maxHealth = 100f;

    [Header("Movement")]
    public float speed = 5f;

    [Header("Attack")]
    public float damage = 25f;
    public float attackCooldown = 0.5f;

    [Header("Critical")]
    [Range(0f, 1f)] public float critChance = 0f;
    public float critMultiplier = 2f;

    [Header("Backstab")]
    [Tooltip("Угол конуса позади цели, в который нужно попасть для бэкстаба")]
    public float backstabAngle = 120f;
}
