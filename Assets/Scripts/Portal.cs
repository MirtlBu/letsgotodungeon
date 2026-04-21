using UnityEngine;

public class Portal : InteractionZone
{
    [Header("Identity")]
    public string portalId;

    [Header("Destination")]
    [SerializeField] private string targetScene;
    [SerializeField] private string targetPortalId;

    // Where the player appears when arriving through this portal
    public Vector3 SpawnPoint
    {
        get
        {
            Vector3 pos = transform.position;
            Vector3 origin = pos + Vector3.up * 5f;
            if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 10f, Physics.AllLayers, QueryTriggerInteraction.Ignore))
            {

                pos.y = hit.point.y + 0.5f;
            }
            return pos;
        }
    }

    protected override void OnInteract()
    {
        PortalManager.Instance.Travel(targetScene, targetPortalId);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(SpawnPoint, 0.3f);
    }
}
