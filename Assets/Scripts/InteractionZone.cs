using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionZone : MonoBehaviour
{
    [SerializeField] private string promptText = "...";
    [SerializeField] protected DialogueSO dialogue;

    private bool playerInRange;
    private bool waitingForDismiss;

    protected virtual void OnInteract() { }
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

    private void Update()
    {
        if (!playerInRange) return;

        // Диалог уже идёт — DialogueManager сам обрабатывает ввод
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsActive) return;

        if (!Keyboard.current.enterKey.wasPressedThisFrame) return;

        if (waitingForDismiss)
        {
            waitingForDismiss = false;
            InteractionUI.Instance.Show(promptText, transform);
            return;
        }

        if (dialogue != null)
        {
            InteractionUI.Instance?.Hide();
            DialogueManager.Instance?.StartDialogue(dialogue, transform, () =>
            {
                OnDialogueEnd();
                // После диалога показываем prompt снова если игрок ещё в зоне
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
