using UnityEngine;

public class Coin : MonoBehaviour
{
    public float rotationSpeed = 90f;
    [SerializeField] private AudioClip pickupSound;
    [SerializeField] [Range(0f, 1f)] private float pickupVolume = 0.6f;

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
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position, pickupVolume);
            Destroy(gameObject);
        }
    }
}
