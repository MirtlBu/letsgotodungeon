using UnityEngine;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private string respawnPortalId = "overworld-dungeon-exit";

    void Start()
    {
        GetComponent<HealthSystem>()?.OnDeath.AddListener(OnDeath);
    }

    private void OnDeath()
    {
        GetComponent<HealthSystem>()?.Heal(float.MaxValue);
        PortalManager.Instance.Travel("Overworld", respawnPortalId);
    }
}
