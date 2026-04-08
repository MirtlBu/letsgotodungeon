using UnityEngine;

// Делает игрока DontDestroyOnLoad — один инстанс на все сцены.
// Добавь этот компонент на префаб игрока.
// Игрока нужно держать ТОЛЬКО в Overworld сцене, не в Dungeon.
public class PlayerPersistence : MonoBehaviour
{
    public static PlayerPersistence Instance { get; private set; }

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
