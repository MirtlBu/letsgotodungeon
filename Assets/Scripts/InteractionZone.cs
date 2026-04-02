using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionZone : MonoBehaviour
{
    [SerializeField] private string promptText = "Interact";

    private bool playerInRange;

    protected virtual void OnInteract() { }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = true;
        InteractionUI.Instance.Show(promptText, transform);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInRange = false;
        InteractionUI.Instance.Hide();
    }

    private void Update()
    {
        if (playerInRange && Keyboard.current.enterKey.wasPressedThisFrame)
            OnInteract();
    }
}
