using UnityEngine;

public class CoinCounter : MonoBehaviour
{
    public static CoinCounter Instance { get; private set; }

    private int count = 0;

    [Header("Debug")]
    public int debugAddCoins = 0;

    void Update()
    {
        if (debugAddCoins != 0)
        {
            count += debugAddCoins;
            debugAddCoins = 0;
        }
    }

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

    public void Reset() => count = 0;
}
