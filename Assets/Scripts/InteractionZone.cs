using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionZone : MonoBehaviour
{
    [SerializeField] private string promptText = "Interact";

    private bool playerInRange;
    private bool waitingForDismiss;

    protected virtual void OnInteract() { }

    protected void ShowResult(string text)
    {
        waitingForDismiss = true;
        InteractionUI.Instance.Show(text, transform);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
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
        InteractionUI.Instance?.Hide();
    }

    private void Update()
    {
        if (!playerInRange) return;
        if (!Keyboard.current.enterKey.wasPressedThisFrame) return;

        if (waitingForDismiss)
        {
            waitingForDismiss = false;
            InteractionUI.Instance.Show(promptText, transform);
            return;
        }

        OnInteract();
    }
}
