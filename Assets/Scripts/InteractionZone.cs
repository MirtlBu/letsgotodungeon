using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionZone : MonoBehaviour
{
    [SerializeField] private string promptText = "...";
    [SerializeField] protected DialogueSO dialogue;
    [SerializeField] private DialogueSO defaultDialogue;

    private bool playerInRange;
    private bool waitingForDismiss;
    private bool skipNextEnter;

    protected virtual void OnInteract() { }
    protected virtual void OnDialogueStart() { }
    protected virtual void OnDialogueEnd() { }

    protected void ShowResult(string text)
    {
        waitingForDismiss = true;
        InteractionUI.Instance.Show(text, transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive) return;
        playerInRange = true;
        waitingForDismiss = false;
        if (InteractionUI.Instance == null) { Debug.LogWarning("InteractionUI not found — add BalloonUI to Overworld scene"); return; }
        InteractionUI.Instance.Show(promptText, transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        waitingForDismiss = false;
        if (DialogueManager.Instance == null || !DialogueManager.Instance.IsActive)
            InteractionUI.Instance?.Hide();
    }

    protected virtual void Update()
    {
        if (!playerInRange) return;

        // Диалог уже идёт — DialogueManager сам обрабатывает ввод
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive) return;

        if (!Keyboard.current.enterKey.wasPressedThisFrame) return;

        if (skipNextEnter) { skipNextEnter = false; return; }

        if (waitingForDismiss)
        {
            waitingForDismiss = false;
            InteractionUI.Instance.Show(promptText, transform);
            return;
        }

        if (dialogue != null)
        {
            InteractionUI.Instance?.Hide();
            OnDialogueStart();
            DialogueManager.Instance?.StartDialogue(dialogue, transform, () =>
            {
                dialogue = defaultDialogue;
                skipNextEnter = true;
                OnDialogueEnd();
                if (playerInRange)
                    InteractionUI.Instance?.Show(promptText, transform);
            });
        }
        else
        {
            OnInteract();
        }
    }
}
