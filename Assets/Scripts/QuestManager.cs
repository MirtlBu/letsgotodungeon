using UnityEngine;

public enum QuestState { NotStarted, Active, Completed, Rewarded }

public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ─── State ────────────────────────────────────────────────

    public QuestState GetState(QuestSO quest)
        => (QuestState)PlayerPrefs.GetInt(StateKey(quest), 0);

    private void SetState(QuestSO quest, QuestState state)
        => PlayerPrefs.SetInt(StateKey(quest), (int)state);

    public int GetProgress(QuestSO quest)
        => PlayerPrefs.GetInt(ProgressKey(quest), 0);

    private void SetProgress(QuestSO quest, int value)
        => PlayerPrefs.SetInt(ProgressKey(quest), value);

    private static string StateKey(QuestSO q)    => $"Quest_{q.questId}";
    private static string ProgressKey(QuestSO q) => $"Quest_{q.questId}_progress";

    // ─── Actions ──────────────────────────────────────────────

    public void StartQuest(QuestSO quest)
    {
        if (GetState(quest) != QuestState.NotStarted) return;
        SetState(quest, QuestState.Active);
        PlayerPrefs.Save();
        Debug.Log($"[Quest] Started: {quest.questName}");
    }

    // Called by EnemyAI on death — advances all active KillEnemies quests
    public void ReportKill()
    {
        // We don't have a registry, so QuestManager finds active quests via known instances.
        // NpcQuestGiver registers their quest on Start.
        foreach (var q in registeredQuests)
        {
            if (GetState(q) != QuestState.Active) continue;
            if (q.objectiveType != QuestObjectiveType.KillEnemies) continue;

            int progress = GetProgress(q) + 1;
            SetProgress(q, progress);
            Debug.Log($"[Quest] {q.questName}: {progress}/{q.targetCount} kills");

            if (progress >= q.targetCount)
            {
                SetState(q, QuestState.Completed);
                Debug.Log($"[Quest] Completed: {q.questName}");
            }
        }
        PlayerPrefs.Save();
    }

    public void GiveReward(QuestSO quest)
    {
        if (GetState(quest) != QuestState.Completed) return;
        SetState(quest, QuestState.Rewarded);

        switch (quest.rewardType)
        {
            case QuestRewardType.Sword:
                PlayerInventory.Instance?.GiveSword();
                break;
            case QuestRewardType.Coins:
                CoinCounter.Instance?.Add(quest.rewardCoins);
                break;
        }

        PlayerPrefs.Save();
        Debug.Log($"[Quest] Reward given for: {quest.questName}");
    }

    // ─── Registry ─────────────────────────────────────────────
    // NpcQuestGiver registers quests so ReportKill() can find them

    private readonly System.Collections.Generic.List<QuestSO> registeredQuests = new();

    public void Register(QuestSO quest)
    {
        if (quest != null && !registeredQuests.Contains(quest))
            registeredQuests.Add(quest);
    }
}
