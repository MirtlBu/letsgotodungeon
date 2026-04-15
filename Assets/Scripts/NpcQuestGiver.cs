using UnityEngine;

// Вешай вместо NPC на квест-NPC.
// В Inspector оставь поля "Dialogue" и "Default Dialogue" (от InteractionZone) пустыми —
// этот скрипт управляет ими сам, в зависимости от состояния квеста.
public class NpcQuestGiver : NPC
{
    [Header("Quest")]
    [SerializeField] private QuestSO quest;

    [Header("Quest Dialogues")]
    [Tooltip("NPC предлагает квест (до принятия)")]
    [SerializeField] private DialogueSO offerDialogue;
    [Tooltip("Квест активен, ещё не выполнен")]
    [SerializeField] private DialogueSO inProgressDialogue;
    [Tooltip("Квест выполнен — NPC выдаёт награду")]
    [SerializeField] private DialogueSO rewardDialogue;
    [Tooltip("После получения награды (навсегда)")]
    [SerializeField] private DialogueSO afterQuestDialogue;

    void Start()
    {
        QuestManager.Instance?.Register(quest);
    }

    // Перед каждым Update базового класса выставляем нужный диалог
    new void Update()
    {
        UpdateDialogueForQuestState();
        base.Update(); // → NPC.Update() → InteractionZone.Update()
    }

    private void UpdateDialogueForQuestState()
    {
        if (quest == null || QuestManager.Instance == null) return;

        switch (QuestManager.Instance.GetState(quest))
        {
            case QuestState.NotStarted:
                dialogue = offerDialogue;
                break;
            case QuestState.Active:
                dialogue = inProgressDialogue;
                break;
            case QuestState.Completed:
                dialogue = rewardDialogue;
                break;
            case QuestState.Rewarded:
                dialogue = afterQuestDialogue;
                break;
        }
    }

    protected override void OnDialogueEnd()
    {
        base.OnDialogueEnd(); // NPC разворачивается и уходит на место

        if (quest == null || QuestManager.Instance == null) return;

        switch (QuestManager.Instance.GetState(quest))
        {
            case QuestState.NotStarted:
                QuestManager.Instance.StartQuest(quest);
                break;
            case QuestState.Completed:
                QuestManager.Instance.GiveReward(quest);
                break;
        }
    }
}
