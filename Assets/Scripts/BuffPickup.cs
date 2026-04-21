using UnityEngine;

// Добавь на GameObject с VFX + Collider (Is Trigger = true).
// При касании игрока — применяет бафф и уничтожает объект.
public class BuffPickup : MonoBehaviour
{
    [SerializeField] private BuffDefinition buff;

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        PlayerStats.Instance?.ApplyBuff(buff);
        Destroy(gameObject);
    }
}
