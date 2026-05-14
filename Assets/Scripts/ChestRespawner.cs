using UnityEngine;

// Вешай на game_chest рядом с Chest и ConditionalInteractable.
// В Inspector: ConditionalInteractable → oneShot = true
//              ConditionalInteractable → OnSuccess → добавь ChestRespawner.MarkUsed()
[RequireComponent(typeof(Chest))]
[RequireComponent(typeof(ConditionalInteractable))]
public class ChestRespawner : MonoBehaviour
{
    private Chest chest;
    private ConditionalInteractable interactable;
    private Vector3 spawnPosition;

    void Start()
    {
        spawnPosition = transform.position;
        chest = GetComponent<Chest>();
        interactable = GetComponent<ConditionalInteractable>();

        GameClock.Instance?.OnMidnight.AddListener(OnMidnight);

        // Если сундук уже был открыт до перезагрузки сцены — восстанавливаем состояние
        if (DungeonState.Instance != null && DungeonState.Instance.IsChestUsed(spawnPosition))
        {
            chest.Open();
            interactable.SetUsed();
        }
    }

    void OnDestroy()
    {
        GameClock.Instance?.OnMidnight.RemoveListener(OnMidnight);
    }

    // Подключи к ConditionalInteractable → OnSuccess в Inspector
    public void MarkUsed()
    {
        DungeonState.Instance?.RegisterChestUsed(spawnPosition);
    }

    private void OnMidnight()
    {
        // DungeonState уже очистил usedChests — сбрасываем визуал и флаг
        chest.Reset();
        interactable.ResetUsed();
    }
}
