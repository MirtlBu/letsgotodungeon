using System.Collections.Generic;
using UnityEngine;

// Хранит состояние данжа между загрузками сцены.
// Добавь на любой объект в Overworld сцене.
public class DungeonState : MonoBehaviour
{
    public static DungeonState Instance { get; private set; }

    private readonly HashSet<Vector3Int> deadEnemies    = new();
    private readonly HashSet<Vector3Int> collectedCoins = new();
    private readonly HashSet<Vector3Int> usedChests     = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        GameClock.Instance?.OnMidnight.AddListener(OnMidnight);
    }

    void OnDestroy()
    {
        GameClock.Instance?.OnMidnight.RemoveListener(OnMidnight);
    }

    private void OnMidnight()
    {
        deadEnemies.Clear();
        usedChests.Clear();
    }

    public void Reset()
    {
        deadEnemies.Clear();
        collectedCoins.Clear();
        usedChests.Clear();
    }

    public bool IsEnemyDead(Vector3 pos)       => deadEnemies.Contains(Key(pos));
    public void RegisterEnemyDeath(Vector3 pos) => deadEnemies.Add(Key(pos));

    public bool IsChestUsed(Vector3 pos)       => usedChests.Contains(Key(pos));
    public void RegisterChestUsed(Vector3 pos) => usedChests.Add(Key(pos));

    public bool IsCoinCollected(Vector3 pos)       => collectedCoins.Contains(Key(pos));
    public void RegisterCoinCollected(Vector3 pos) => collectedCoins.Add(Key(pos));

    // Позиция с точностью до 0.1 единицы — достаточно для статических объектов
    private static Vector3Int Key(Vector3 v) => new Vector3Int(
        Mathf.RoundToInt(v.x * 10),
        Mathf.RoundToInt(v.y * 10),
        Mathf.RoundToInt(v.z * 10)
    );
}
