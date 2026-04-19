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
    [SerializeField] private float blockSize         = 2f;
    [SerializeField] private int   platformCount     = 3;
    [SerializeField] private int   minConnectors     = 4;
    [SerializeField] private int   maxConnectors     = 8;
    [SerializeField] private float turnChance        = 0.3f;
    [SerializeField] private int   platformClearance = 3; // мин. дистанция между платформами в клетках

    [Header("Coins")]
    public GameObject coinPrefab;
    [SerializeField] private int   coinsPerCorridor = 5;
    [SerializeField] private float coinSpacing      = 1f;
    [SerializeField] private float coinHeight       = 1f;

    [Header("Environment")]
    public GameObject[] envPrefabs;
    [SerializeField] private int   minEnvPerPlatform = 1;
    [SerializeField] private int   maxEnvPerPlatform = 3;
    [SerializeField] private float envRadius         = 1.5f; // разброс от центра платформы

    [Header("Enemies")]
    public GameObject enemyPrefab;
    [SerializeField] private int minEnemies = 2;
    [SerializeField] private int maxEnemies = 5;

    [Header("Portals")]
    public GameObject entrancePortalPrefab;
    public GameObject exitPortalPrefab;
    [SerializeField] private float portalInset = 1f; // сдвиг назад от порта вглубь платформы

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
        Vector3 entrancePortDir = fromPort.forward;

        // ── Segments: коридор из кубов → round ───────────────────────
        var intermediatePlatforms = new List<GameObject>();
        var intermediateDirs      = new List<Vector3>();
        var corridorPaths         = new List<List<Vector3>>();
        int totalSegments         = platformCount + 1;
        GameObject exitGO         = null;
        Vector3    exitPortDir    = Vector3.forward;

        for (int seg = 0; seg < totalSegments; seg++)
        {
            int  count      = Random.Range(minConnectors, maxConnectors + 1);
            bool justTurned = false;
            var  segPath    = new List<Vector3>();

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
                segPath.Add(cubeGO.transform.position);

                Transform exitPort  = null;
                bool actuallyTurned = false;

                if (i >= 3 && !justTurned && Random.value < turnChance)
                {
                    exitPort = GetTurnPort(cubePiece, fromPort);
                    if (exitPort != null) actuallyTurned = true;
                }

                exitPort ??= GetOppositePort(cubePiece, fromPort);
                if (exitPort == null)
                {
                    spawned.Remove(cubeGO);
                    Destroy(cubeGO);
                    goto nextSegment;
                }

                justTurned = actuallyTurned;
                cubePiece.MarkConnected(exitPort);
                fromPort = exitPort;
            }

            nextSegment:
            if (segPath.Count > 0) corridorPaths.Add(segPath);
            var roundGO      = Snap(roundPrefab, fromPort);
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
                intermediateDirs.Add(fromPort.forward);

                Transform nextPort = GetOppositePort(roundPiece, fromPort);
                if (nextPort == null) { exitGO = roundGO; exitPortDir = -fromPort.forward; break; }
                roundPiece.MarkConnected(nextPort);
                fromPort = nextPort;
            }
            else
            {
                exitGO = roundGO;
                exitPortDir = -fromPort.forward;
            }
        }

        // ── NavMesh ───────────────────────────────────────────────────
        surface?.BuildNavMesh();

        // ── Coins вдоль коридоров ─────────────────────────────────────
        foreach (var path in corridorPaths)
            SpawnCoinsAlongPath(path);

        // ── Environment на всех платформах ───────────────────────────
        SpawnEnvObjects(startGO.transform.position);
        foreach (var p in intermediatePlatforms)
            SpawnEnvObjects(p.transform.position);
        if (exitGO != null)
            SpawnEnvObjects(exitGO.transform.position);

        // ── Enemies на промежуточных платформах ──────────────────────
        for (int e = 0; e < intermediatePlatforms.Count; e++)
            SpawnEnemies(intermediatePlatforms[e].transform.position, intermediateDirs[e]);

        // ── Portals ───────────────────────────────────────────────────
        SpawnPortal(entrancePortalPrefab, startGO.transform.position, entrancePortDir);
        if (exitGO != null)
            SpawnPortal(exitPortalPrefab, exitGO.transform.position, exitPortDir);
    }

    private GameObject Snap(GameObject prefab, Transform fromPort)
    {
        var go    = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        spawned.Add(go);

        var piece = go.GetComponent<DungeonPiece>();
        if (piece == null) return go;

        var free = piece.GetFreePorts();
        if (free.Count == 0) return go;

        Transform attachPort   = free[0];
        piece.MarkConnected(attachPort);

        Quaternion desiredRot  = Quaternion.LookRotation(-fromPort.forward, Vector3.up);
        go.transform.rotation  = desiredRot * Quaternion.Inverse(attachPort.localRotation);
        go.transform.position += fromPort.position - attachPort.position;

        return go;
    }

    private bool TooCloseToPlatform(Vector3Int cell)
    {
        for (int x = -platformClearance; x <= platformClearance; x++)
        for (int z = -platformClearance; z <= platformClearance; z++)
        {
            if (x == 0 && z == 0) continue;
            if (platformCells.Contains(new Vector3Int(cell.x + x, cell.y, cell.z + z)))
                return true;
        }
        return false;
    }

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

    private void SpawnCoinsAlongPath(List<Vector3> path)
    {
        if (coinPrefab == null || path.Count < 2 || coinsPerCorridor <= 0) return;

        // Считаем общую длину пути
        float totalLen = 0f;
        for (int i = 0; i < path.Count - 1; i++)
            totalLen += Vector3.Distance(path[i], path[i + 1]);

        float span        = (coinsPerCorridor - 1) * coinSpacing;
        float startOffset = Mathf.Max(0f, (totalLen - span) * 0.5f); // центрируем

        // Вспомогательная функция: получить позицию на дистанции d от начала пути
        Vector3 PointAtDistance(float d)
        {
            float acc = 0f;
            for (int i = 0; i < path.Count - 1; i++)
            {
                float seg = Vector3.Distance(path[i], path[i + 1]);
                if (acc + seg >= d)
                    return Vector3.Lerp(path[i], path[i + 1], (d - acc) / seg);
                acc += seg;
            }
            return path[^1];
        }

        for (int c = 0; c < coinsPerCorridor; c++)
        {
            float   dist = startOffset + c * coinSpacing;
            Vector3 pos  = PointAtDistance(dist);
            pos.y += coinHeight;
            spawned.Add(Instantiate(coinPrefab, pos, Quaternion.identity));
        }
    }

    private void SpawnEnvObjects(Vector3 center)
    {
        if (envPrefabs == null || envPrefabs.Length == 0) return;
        int count = Random.Range(minEnvPerPlatform, maxEnvPerPlatform + 1);
        for (int i = 0; i < count; i++)
        {
            var prefab = envPrefabs[Random.Range(0, envPrefabs.Length)];
            if (prefab == null) continue;
            Vector2 r   = Random.insideUnitCircle * envRadius;
            Vector3 pos = center + new Vector3(r.x, 5f, r.y);
            float   yRot = Random.Range(0f, 360f);
            spawned.Add(Instantiate(prefab, pos, Quaternion.Euler(0f, yRot, 0f)));
        }
    }

    private void SpawnEnemies(Vector3 center, Vector3 corridorDir)
    {
        if (enemyPrefab == null) return;
        Quaternion rot = corridorDir != Vector3.zero
            ? Quaternion.LookRotation(corridorDir, Vector3.up)
            : Quaternion.identity;
        int count = Random.Range(minEnemies, maxEnemies + 1);
        for (int i = 0; i < count; i++)
        {
            Vector2 r   = Random.insideUnitCircle;
            Vector3 pos = center + new Vector3(r.x, 1f, r.y);
            if (NavMesh.SamplePosition(pos, out NavMeshHit hit, 4f, NavMesh.AllAreas))
                pos = hit.position;
            spawned.Add(Instantiate(enemyPrefab, pos, rot));
        }
    }

    private void SpawnPortal(GameObject prefab, Vector3 pos, Vector3 direction)
    {
        if (prefab == null) return;
        float yAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        Vector3 spawnPos = pos + Vector3.up * 10f - direction.normalized * portalInset;
        spawned.Add(Instantiate(prefab, spawnPos, Quaternion.Euler(90f, yAngle, 0f)));
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
