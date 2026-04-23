using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Тестовый скрипт. Tab → телепортирует игрока к порталу с указанным portalId.
/// Удали или отключи перед релизом.
/// </summary>
public class DebugTeleport : MonoBehaviour
{
    [SerializeField] private string targetPortalId = "overworld";

    [Header("Debug Cheat (Q)")]
    [SerializeField] private BuffDefinition debugBuff; // перетащи нужный бафф

    void Update()
    {
        if (Keyboard.current.qKey.wasPressedThisFrame && debugBuff != null)
        {
            PlayerStats.Instance?.ApplyBuff(debugBuff);
            Debug.Log("[Debug] Buff applied");
        }

        if (!Keyboard.current.tabKey.wasPressedThisFrame) return;

        Portal portal = FindPortal(targetPortalId);
        if (portal == null)
        {
            Debug.LogWarning($"[DebugTeleport] Portal '{targetPortalId}' не найден");
            return;
        }

        var cc = GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        transform.position = portal.SpawnPoint;
        if (cc != null) cc.enabled = true;

        GetComponent<CharacterMovement>()?.ForceUnground();
        FindObjectOfType<CameraController>()?.SnapToTarget();

        Debug.Log($"[DebugTeleport] Телепорт к '{targetPortalId}'");
    }

    private Portal FindPortal(string id)
    {
        foreach (var p in FindObjectsByType<Portal>(FindObjectsSortMode.None))
            if (p.portalId == id) return p;
        return null;
    }
}
