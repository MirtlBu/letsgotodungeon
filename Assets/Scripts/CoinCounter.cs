using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    public static CoinCounter Instance { get; private set; }

    private int count = 0;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Add(int amount)
    {
        count += amount;
    }

    public int GetCount() => count;
}
