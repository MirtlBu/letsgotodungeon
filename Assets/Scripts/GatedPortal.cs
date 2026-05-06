using UnityEngine;

// Портал с условием — не пускает пока игрок не получил меч.
// Вместо телепортации запускает диалог от имени NPC (например Varis).
public class GatedPortal : Portal
{
    [Header("Blocked Dialogue")]
    [SerializeField] private DialogueSO blockedDialogue;
    [SerializeField] private Transform dialogueSource; // Transform Varis-а

    protected override void OnInteract()
    {
        if (PlayerInventory.Instance == null || !PlayerInventory.Instance.HasSword)
        {
            if (blockedDialogue != null)
                DialogueManager.Instance?.StartDialogue(blockedDialogue, dialogueSource ?? transform,
                    () => skipNextEnter = true);
            return;
        }

        base.OnInteract();
    }
}
