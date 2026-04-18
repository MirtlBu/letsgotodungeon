using System.Collections.Generic;
using UnityEngine;

public class DungeonPiece : MonoBehaviour
{
    public enum Role { Platform, Connector }

    [Header("Setup")]
    public Role role;
    public Transform[] ports; // port_a, port_b, port_c, port_d

    [HideInInspector] public bool[] portConnected;

    void Awake()
    {
        portConnected = new bool[ports.Length];
    }

    public List<Transform> GetFreePorts()
    {
        var result = new List<Transform>();
        for (int i = 0; i < ports.Length; i++)
            if (!portConnected[i]) result.Add(ports[i]);
        return result;
    }

    public void MarkConnected(Transform port)
    {
        for (int i = 0; i < ports.Length; i++)
            if (ports[i] == port) portConnected[i] = true;
    }

    void OnDrawGizmosSelected()
    {
        if (ports == null) return;
        foreach (var p in ports)
        {
            if (p == null) continue;
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(p.position, 0.15f);
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(p.position, p.forward * 0.5f);
        }
    }
}
