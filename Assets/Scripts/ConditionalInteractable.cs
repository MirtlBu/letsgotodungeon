using UnityEngine;
using UnityEngine.Events;

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

    [Header("On Success")]
    [SerializeField] private UnityEvent onSuccess;

    [Header("One-time")]
    [SerializeField] private bool destroyOnSuccess = false;
    [SerializeField] private GameObject objectToDestroyOnSuccess; // уничтожается после баллона
    [SerializeField] private GameObject objectToHideImmediately;  // скрывается сразу (SetActive false)
    [SerializeField] private bool oneShot = false;
    [SerializeField] [TextArea(1, 2)] private string usedText = "Nothing more here...";

    [Header("Persistent Destroy")]
    [Tooltip("Если не пусто — при destroyOnSuccess сохраняет этот ключ в PlayerPrefs и не появляется снова никогда")]
    [SerializeField] private string persistKey = "";

    private bool pendingDestroy;
    private bool pendingShowWeapon;
    private bool used;

    void OnEnable()
    {
        if (!string.IsNullOrEmpty(persistKey) && PlayerPrefs.GetInt(persistKey, 0) == 1)
            Destroy(gameObject);
    }

    protected override void OnInteract()
    {
        if (used)
        {
            ShowResultTimed(usedText);
            return;
        }

        if (CheckCondition())
        {
            GiveReward();
            onSuccess?.Invoke();
            ShowResultTimed(successText, 2f);
            if (oneShot) used = true;
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
        if (pendingDestroy)
        {
            if (!string.IsNullOrEmpty(persistKey))
            {
                PlayerPrefs.SetInt(persistKey, 1);
                PlayerPrefs.Save();
            }
            Destroy(gameObject);
        }
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
