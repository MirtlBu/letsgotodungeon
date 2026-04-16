using UnityEngine;

// Торговец. Оставь поля "Dialogue" и "Default Dialogue" (от InteractionZone) пустыми.
// Схема диалогов в Inspector:
//   offerDialogue  — NPC предлагает товар, 2 choices:
//                    choice[0] "Yes!" → nextDialogue = acceptDialogue
//                    choice[1] "No thanks." → nextDialogue = declineDialogue (или null)
//   acceptDialogue — "Great, here you go!" (игрок согласился)
//   declineDialogue — "Maybe next time." (игрок отказался)
public class NpcTrader : NPC
{
    [Header("Shop")]
    [SerializeField] private ShopItemSO item;

    [Header("Trade Dialogues")]
    [Tooltip("NPC предлагает купить, 2 choices: Yes / No")]
    [SerializeField] private DialogueSO offerDialogue;
    [Tooltip("Игрок согласился (choice[0].nextDialogue должен вести сюда)")]
    [SerializeField] private DialogueSO acceptDialogue;
    [Tooltip("Игрок отказался (choice[1].nextDialogue или null)")]
    [SerializeField] private DialogueSO declineDialogue;
    [Tooltip("Не хватает монет — вместо acceptDialogue будет этот")]
    [SerializeField] private DialogueSO noMoneyDialogue;

    protected override bool RestoreRootMotionAfterReturn => false;

    // Каждый кадр выставляем dialogue = offerDialogue, чтобы InteractionZone
    // сам обработал конец диалога (skipNextEnter + balloon) — как у обычного NPC
    new void Update()
    {
        dialogue = offerDialogue;
        base.Update();
    }

    protected override void OnDialogueStart()
    {
        base.OnDialogueStart(); // ставит "talking" = true
        GetComponent<Animator>()?.SetBool("talk", true);
        DialogueManager.Instance.OnChoiceConfirmed += OnChoiceSelected;
    }

    private void OnChoiceSelected(int index)
    {
        DialogueManager.Instance.OnChoiceConfirmed -= OnChoiceSelected;

        // index 0 = "Yes" — проверяем монеты ДО перехода к acceptDialogue
        if (index != 0 || item == null) return;

        int coins = CoinCounter.Instance?.GetCount() ?? 0;
        if (coins < item.price && noMoneyDialogue != null)
            DialogueManager.Instance.OverrideNextDialogue(noMoneyDialogue);
    }

    protected override void OnDialogueEnd()
    {
        GetComponent<Animator>()?.SetBool("talk", false);
        base.OnDialogueEnd(); // NPC разворачивается и уходит на место
        DialogueManager.Instance.OnChoiceConfirmed -= OnChoiceSelected; // на случай cancel

        if (item == null) return;
        if (DialogueManager.Instance?.LastFinishedDialogue != acceptDialogue) return;

        CoinCounter.Instance.Add(-item.price);
        if (item.buff != null)
        {
            PlayerStats.Instance?.ApplyBuff(item.buff);
            Debug.Log($"[Trader] Buff applied: {item.buff.buffName} | stat={item.buff.statType} | x{item.buff.multiplier} +{item.buff.flatBonus} | duration={item.buff.duration}s");
        }
        ShowResult($"{item.itemName} purchased!");
    }
}
