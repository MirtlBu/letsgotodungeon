using UnityEngine;

public enum InteractConditionType
{
    None,            // условие всегда выполнено
    HasSword,
    HasActiveBuff,   // есть активный бафф на указанный стат
    QuestCompleted,  // квест в состоянии Completed или Rewarded
}

public enum InteractRewardType
{
    None,
    GiveSword,
    ApplyBuff,
    GiveCoins,
}

// Вешай на любой объект (меч, сундук, алтарь...).
// Prompt balloon — из базового InteractionZone ("Investigate" и т.д.)
// При Enter: проверяет условие → показывает failText или successText + выдаёт награду.
public class ConditionalInteractable : InteractionZone
{
    [Header("Condition")]
    [SerializeField] private InteractConditionType conditionType;
    [SerializeField] private BuffDefinition requiredBuff;   // для HasActiveBuff
    [SerializeField] private QuestSO requiredQuest;         // для QuestCompleted

    [Header("Messages")]
    [SerializeField] [TextArea(1, 3)] private string failText    = "I don't have what it takes...";
    [SerializeField] [TextArea(1, 3)] private string successText = "You got it!";

    [Header("Reward (on success)")]
    [SerializeField] private InteractRewardType rewardType;
    [SerializeField] private BuffDefinition rewardBuff;     // для ApplyBuff
    [SerializeField] private int rewardCoins;               // для GiveCoins

    [Header("One-time")]
    [SerializeField] private bool destroyOnSuccess = false;

    private bool pendingDestroy;

    protected override void OnInteract()
    {
        if (CheckCondition())
        {
            GiveReward();
            ShowResultTimed(successText);
            if (destroyOnSuccess) pendingDestroy = true;
        }
        else
        {
            ShowResultTimed(failText);
        }
    }

    protected override void OnResultHidden()
    {
        if (!pendingDestroy) return;
        InteractionUI.Instance?.Hide();
        Destroy(gameObject);
    }

    private bool CheckCondition()
    {
        switch (conditionType)
        {
            case InteractConditionType.None:
                return true;

            case InteractConditionType.HasSword:
                return PlayerInventory.Instance != null && PlayerInventory.Instance.HasSword;

            case InteractConditionType.HasActiveBuff:
                // Проверяем есть ли активный бафф на нужный стат
                // (PlayerStats не открывает список напрямую, проверяем через ComputeStat)
                if (requiredBuff == null || PlayerStats.Instance == null) return false;
                float computed = GetStatValue(requiredBuff.statType);
                float baseVal  = GetBaseStatValue(requiredBuff.statType);
                return computed > baseVal;

            case InteractConditionType.QuestCompleted:
                if (requiredQuest == null || QuestManager.Instance == null) return false;
                var state = QuestManager.Instance.GetState(requiredQuest);
                return state == QuestState.Completed || state == QuestState.Rewarded;

            default:
                return false;
        }
    }

    private void GiveReward()
    {
        switch (rewardType)
        {
            case InteractRewardType.GiveSword:
                PlayerInventory.Instance?.GiveSword();
                break;
            case InteractRewardType.ApplyBuff:
                if (rewardBuff != null) PlayerStats.Instance?.ApplyBuff(rewardBuff);
                break;
            case InteractRewardType.GiveCoins:
                CoinCounter.Instance?.Add(rewardCoins);
                break;
        }
    }

    // ─── Helpers для проверки HasActiveBuff ───────────────────

    private float GetStatValue(StatType type)
    {
        if (PlayerStats.Instance == null) return 0f;
        return type switch
        {
            StatType.Speed          => PlayerStats.Instance.Speed,
            StatType.Damage         => PlayerStats.Instance.Damage,
            StatType.CritChance     => PlayerStats.Instance.CritChance,
            StatType.CritMultiplier => PlayerStats.Instance.CritMultiplier,
            _ => 0f
        };
    }

    private float GetBaseStatValue(StatType type)
    {
        var stats = GetComponentInParent<Stats>() ?? FindObjectOfType<Stats>();
        if (stats == null) return 0f;
        return type switch
        {
            StatType.Speed          => stats.speed,
            StatType.Damage         => stats.damage,
            StatType.CritChance     => stats.critChance,
            StatType.CritMultiplier => stats.critMultiplier,
            _ => 0f
        };
    }
}
