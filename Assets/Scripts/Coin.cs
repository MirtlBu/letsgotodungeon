using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 90f;

    void Start()
    {
        if (DungeonState.Instance != null && DungeonState.Instance.IsCoinCollected(transform.position))
            Destroy(gameObject);
    }

    void Update()
    {
        transform.Rotate(0f, rotationSpeed * Time.deltaTime, 0f, Space.World);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            DungeonState.Instance?.RegisterCoinCollected(transform.position);
            CoinCounter.Instance.Add(1);
            Destroy(gameObject);
        }
    }
}
