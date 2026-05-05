using UnityEngine;

// Вешай на prefab фаербола.
// Prefab: Sphere (или меш) + SphereCollider (Is Trigger = true) + Fireball.cs
// Слой фаербола НЕ должен коллайдиться с самим врагом (настрой Layer Collision Matrix).
[RequireComponent(typeof(Collider))]
public class Fireball : MonoBehaviour
{
    [SerializeField] private float speed = 10f;
    [SerializeField] private float lifetime = 4f;

    private Vector3 direction;
    private float damage;

    public void Init(Vector3 dir, float dmg)
    {
        direction = dir.normalized;
        damage = dmg;
        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        other.GetComponent<HealthSystem>()?.TakeDamage(damage);
        DamageNumbersUI.Instance?.Show(damage, transform.position, false, isPlayerDamage: true);
        Destroy(gameObject);
    }
}
