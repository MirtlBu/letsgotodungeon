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
    [SerializeField] private GameObject objectToDestroyOnSuccess; // уничтожается после баллона
    [SerializeField] private GameObject objectToHideImmediately;  // скрывается сразу (SetActive false)

    private bool pendingDestroy;
    private bool pendingShowWeapon;

    protected override void OnInteract()
    {
        if (CheckCondition())
        {
            GiveReward();
            ShowResultTimed(successText, 2f);
            if (destroyOnSuccess) pendingDestroy = true;
        }
        else
        {
            ShowResultTimed(failText);
        }
    }

    protected override void OnResultHidden()
    {
        if (!pendingDestroy && !pendingShowWeapon) return;
        InteractionUI.Instance?.Hide();
        if (pendingDestroy) Destroy(gameObject);
        if (pendingShowWeapon)
        {
            pendingShowWeapon = false;
            GameObject.FindWithTag("Player")?.GetComponent<WeaponVisibility>()?.SetAlwaysVisible();
        }
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
                if (requiredBuff == null || PlayerStats.Instance == null) return false;
                foreach (var b in PlayerStats.Instance.ActiveBuffs)
                    if (b.definition == requiredBuff) return true;
                return false;

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
                pendingShowWeapon = true; // меч в руках появится только после скрытия game_sword
                if (objectToHideImmediately != null) objectToHideImmediately.SetActive(false);
                break;
            case InteractRewardType.ApplyBuff:
                if (rewardBuff != null) PlayerStats.Instance?.ApplyBuff(rewardBuff);
                break;
            case InteractRewardType.GiveCoins:
                CoinCounter.Instance?.Add(rewardCoins);
                break;
        }
    }

}
