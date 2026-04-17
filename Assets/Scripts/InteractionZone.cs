using System.Collections;
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
    protected virtual void OnDialogueCancelled() { }
    protected virtual void OnResultHidden() { }

    protected void ShowResult(string text)
    {
        waitingForDismiss = true;
        InteractionUI.Instance.Show(text, transform);
    }

    // Исчезает по Enter ИЛИ через duration секунд — что наступит раньше
    protected void ShowResultTimed(string text, float duration = 3f)
    {
        ShowResult(text);
        StartCoroutine(AutoHideResult(duration));
    }

    private IEnumerator AutoHideResult(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (!waitingForDismiss) yield break; // Enter уже нажали раньше
        waitingForDismiss = false;
        InteractionUI.Instance?.Hide();
        if (playerInRange)
            InteractionUI.Instance?.Show(promptText, transform);
        OnResultHidden();
    }

    // Вызывай после ручного StartDialogue (OnInteract-flow), чтобы не было мгновенного перезапуска
    protected void FinishInteraction()
    {
        skipNextEnter = true;
        if (playerInRange)
            InteractionUI.Instance?.Show(promptText, transform);
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
        skipNextEnter = false;
        bool wasActive = DialogueManager.Instance != null && DialogueManager.Instance.IsActive;
        DialogueManager.Instance?.CancelDialogue();
        if (wasActive) OnDialogueCancelled();
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
            InteractionUI.Instance?.Hide();
            if (playerInRange)
                InteractionUI.Instance?.Show(promptText, transform);
            OnResultHidden();
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
