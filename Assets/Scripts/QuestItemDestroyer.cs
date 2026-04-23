using UnityEngine;

// Вешай на квестовый предмет (game_sword, chest и т.д.)
// 1. Задай уникальный key (например "quest_sword")
// 2. Подключи Collect() к OnSuccess UnityEvent в ConditionalInteractable
// При старте сцены: если предмет уже был собран — объект уничтожается мгновенно.
public class QuestItemDestroyer : MonoBehaviour
{
    [SerializeField] private string key; // уникальный ключ, например "quest_sword"

    void Start()
    {
        if (IsCollected())
            Destroy(gameObject);
    }

    // Подключи к OnSuccess в ConditionalInteractable
    public void Collect()
    {
        if (string.IsNullOrEmpty(key)) return;
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        Destroy(gameObject);
    }

    public bool IsCollected() =>
        !string.IsNullOrEmpty(key) && PlayerPrefs.GetInt(key, 0) == 1;

#if UNITY_EDITOR
    // Сброс для тестирования — вызови через контекстное меню компонента
    [ContextMenu("Reset collected state")]
    private void ResetState()
    {
        PlayerPrefs.DeleteKey(key);
        PlayerPrefs.Save();
        Debug.Log($"[QuestItemDestroyer] Key '{key}' reset");
    }
#endif
}
