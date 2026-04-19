using System.Collections.Generic;
using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class DungeonGenerator : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject roundPrefab; // платформа, 4 порта
    public GameObject cubePrefab;  // коннектор, 4 порта

    [Header("Generation")]
    [SerializeField] private float blockSize     = 2f;
    [SerializeField] private int   platformCount = 3;
    [SerializeField] private int   minConnectors = 4;
    [SerializeField] private int   maxConnectors = 8;
    [SerializeField] private float turnChance    = 0.3f;

    [Header("Enemies")]
    public GameObject enemyPrefab;
    [SerializeField] private int minEnemies = 2;
    [SerializeField] private int maxEnemies = 5;

    [Header("Portals")]
    public GameObject entrancePortalPrefab;
    public GameObject exitPortalPrefab;

    private readonly List<GameObject>    spawned       = new();
    private readonly HashSet<Vector3Int> occupiedCells = new();
    private readonly HashSet<Vector3Int> platformCells = new();
    private NavMeshSurface surface;

    void Start()
    {
        surface = GetComponent<NavMeshSurface>();
        Generate();
    }

    public void Generate()
    {
        if (roundPrefab == null || cubePrefab == null)
        {
            Debug.LogError("[DungeonGenerator] roundPrefab / cubePrefab не назначены!", this);
            return;
        }

        Clear();

        // ── Start round ───────────────────────────────────────────────
        var startGO    = Place(roundPrefab, Vector3.zero, Quaternion.identity);
        var startPiece = startGO.GetComponent<DungeonPiece>();
        if (startPiece == null) { Debug.LogError("Нет DungeonPiece на roundPrefab!", this); return; }

        var initPorts = startPiece.GetFreePorts();
        if (initPorts.Count == 0) { Debug.LogError("У round нет портов!", this); return; }

        var startCell = WorldToCell(startGO.transform.position);
        occupiedCells.Add(startCell);
        platformCells.Add(startCell);

        Transform fromPort = initPorts[0];
        startPiece.MarkConnected(fromPort);

        // ── Segments: коридор из кубов → round ───────────────────────
        var cubePieces            = new List<GameObject>();
        var intermediatePlatforms = new List<GameObject>();
        int totalSegments         = platformCount + 1;
        GameObject exitGO         = null;

        for (int seg = 0; seg < totalSegments; seg++)
        {
            int count      = Random.Range(minConnectors, maxConnectors + 1);
            bool justTurned = false;

            for (int i = 0; i < count; i++)
            {
                var cubeGO    = Snap(cubePrefab, fromPort);
                var cubePiece = cubeGO.GetComponent<DungeonPiece>();

                Vector3Int cell = WorldToCell(cubeGO.transform.position);
                if (occupiedCells.Contains(cell))
                {
                    spawned.Remove(cubeGO);
                    Destroy(cubeGO);
                    goto nextSegment;
                }
                occupiedCells.Add(cell);
                cubePieces.Add(cubeGO);

                Transform exitPort    = null;
                bool actuallyTurned   = false;

                if (!justTurned && Random.value < turnChance)
                {
                    exitPort = GetTurnPort(cubePiece, fromPort);
                    if (exitPort != null) actuallyTurned = true;
                }

                exitPort ??= GetOppositePort(cubePiece, fromPort);
                if (exitPort == null) goto nextSegment;

                justTurned = actuallyTurned;
                cubePiece.MarkConnected(exitPort);
                fromPort = exitPort;
            }

            nextSegment:
            var roundGO    = Snap(roundPrefab, fromPort);
            Vector3Int roundCell = WorldToCell(roundGO.transform.position);
            if (occupiedCells.Contains(roundCell) || TooCloseToPlatform(roundCell))
            {
                spawned.Remove(roundGO);
                Destroy(roundGO);
                break;
            }
            occupiedCells.Add(roundCell);
            platformCells.Add(roundCell);

            var roundPiece = roundGO.GetComponent<DungeonPiece>();

            if (seg < totalSegments - 1)
            {
                intermediatePlatforms.Add(roundGO);

                Transform nextPort = Random.value < turnChance
                    ? GetTurnPort(roundPiece, fromPort)
                    : GetOppositePort(roundPiece, fromPort);
                nextPort ??= GetOppositePort(roundPiece, fromPort);
                if (nextPort == null) { exitGO = roundGO; break; }
                roundPiece.MarkConnected(nextPort);
                fromPort = nextPort;
            }
            else
            {
                exitGO = roundGO;
            }
        }

        // ── NavMesh ───────────────────────────────────────────────────
        surface?.BuildNavMesh();

        // ── Enemies на промежуточных платформах ──────────────────────
        foreach (var platform in intermediatePlatforms)
            SpawnEnemies(platform.transform.position);

        // ── Portals ───────────────────────────────────────────────────
        SpawnPortal(entrancePortalPrefab, startGO.transform.position);
        if (exitGO != null)
            SpawnPortal(exitPortalPrefab, exitGO.transform.position);
    }

    private GameObject Snap(GameObject prefab, Transform fromPort)
    {
        var go    = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        spawned.Add(go);

        var piece = go.GetComponent<DungeonPiece>();
        if (piece == null) return go;

        var free = piece.GetFreePorts();
        if (free.Count == 0) return go;

        Transform attachPort      = free[0];
        piece.MarkConnected(attachPort);

        Quaternion desiredRot     = Quaternion.LookRotation(-fromPort.forward, Vector3.up);
        go.transform.rotation     = desiredRot * Quaternion.Inverse(attachPort.localRotation);
        go.transform.position    += fromPort.position - attachPort.position;

        return go;
    }

    // Возвращает true если cell находится в радиусе 1 клетки от любой платформы
    private bool TooCloseToPlatform(Vector3Int cell)
    {
        for (int x = -1; x <= 1; x++)
        for (int z = -1; z <= 1; z++)
        {
            if (x == 0 && z == 0) continue;
            if (platformCells.Contains(new Vector3Int(cell.x + x, cell.y, cell.z + z)))
                return true;
        }
        return false;
    }

    // Порт наиболее совпадающий с fromPort.forward (прямо)
    private Transform GetOppositePort(DungeonPiece piece, Transform fromPort)
    {
        Transform best    = null;
        float     bestDot = -2f;
        foreach (var p in piece.GetFreePorts())
        {
            float dot = Vector3.Dot(p.forward, fromPort.forward);
            if (dot > bestDot) { bestDot = dot; best = p; }
        }
        return best;
    }

    // Порт под углом 90° к fromPort.forward (поворот)
    private Transform GetTurnPort(DungeonPiece piece, Transform fromPort)
    {
        var candidates = new List<Transform>();
        foreach (var p in piece.GetFreePorts())
        {
            float dot = Vector3.Dot(p.forward, fromPort.forward);
            if (Mathf.Abs(dot) < 0.5f) candidates.Add(p);
        }
        return candidates.Count > 0 ? candidates[Random.Range(0, candidates.Count)] : null;
    }

    private Vector3Int WorldToCell(Vector3 pos) => new Vector3Int(
        Mathf.RoundToInt(pos.x / blockSize),
        Mathf.RoundToInt(pos.y / blockSize),
        Mathf.RoundToInt(pos.z / blockSize)
    );

    private GameObject Place(GameObject prefab, Vector3 pos, Quaternion rot)
    {
        var go = Instantiate(prefab, pos, rot);
        spawned.Add(go);
        return go;
    }

    private void SpawnEnemies(Vector3 center)
    {
        if (enemyPrefab == null) return;
        int count = Random.Range(minEnemies, maxEnemies + 1);
        for (int i = 0; i < count; i++)
        {
            Vector2 r   = Random.insideUnitCircle;
            Vector3 pos = center + new Vector3(r.x, 1f, r.y);
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 4f, NavMesh.AllAreas))
                pos = hit.position;
            spawned.Add(Instantiate(enemyPrefab, pos, Quaternion.identity));
        }
    }

    private void SpawnPortal(GameObject prefab, Vector3 pos)
    {
        if (prefab == null) return;
        spawned.Add(Instantiate(prefab, pos + Vector3.up * 5f, Quaternion.Euler(90f, 0f, 0f)));
    }

    public void Clear()
    {
        foreach (var go in spawned)
            if (go != null) Destroy(go);
        spawned.Clear();
        occupiedCells.Clear();
        platformCells.Clear();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
}
