using UnityEngine;

// Setup:
//  1. Create a Plane (or Quad scaled up) above your scene — Y doesn't matter much, just above geometry
//  2. Give it a transparent-capable material (Shader: Universal Render Pipeline/Unlit, Surface: Transparent)
//  3. Add this script to any GameObject, drag the plane into FogPlane
//  4. Player is found automatically by tag
[RequireComponent(typeof(MeshFilter))]
public class FogOfWar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject fogPlane;
    [SerializeField] private Transform player;         // auto-found if empty

    [Header("Visibility")]
    [SerializeField] private float sightRadius  = 8f;  // fully clear
    [SerializeField] private float fadeWidth    = 3f;  // transition ring

    [Header("Colors")]
    [SerializeField] private Color unexplored = new Color(0, 0, 0, 1f);
    [SerializeField] private Color explored   = new Color(0, 0, 0, 0.5f);
    [SerializeField] private Color visible    = new Color(0, 0, 0, 0f);

    private Mesh mesh;
    private Vector3[] vertices;   // local space
    private Color[] colors;
    private float[] minAlpha;     // tracks explored state (lowest alpha seen)

    void Start()
    {
        if (player == null)
        {
            var go = GameObject.FindWithTag("Player");
            if (go != null) player = go.transform;
        }

        if (fogPlane == null) { Debug.LogWarning("FogOfWar: fogPlane not assigned"); return; }

        mesh = fogPlane.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;

        colors  = new Color[vertices.Length];
        minAlpha = new float[vertices.Length];

        for (int i = 0; i < colors.Length; i++)
        {
            colors[i]   = unexplored;
            minAlpha[i] = unexplored.a;
        }

        mesh.colors = colors;
    }

    void Update()
    {
        if (mesh == null || player == null) return;

        bool changed = false;

        for (int i = 0; i < vertices.Length; i++)
        {
            // Convert vertex to world XZ, ignore Y (plane can be at any height)
            Vector3 worldPos = fogPlane.transform.TransformPoint(vertices[i]);
            float dist = new Vector2(worldPos.x - player.position.x,
                                     worldPos.z - player.position.z).magnitude;

            float targetAlpha;
            if (dist <= sightRadius)
                targetAlpha = visible.a;
            else if (dist <= sightRadius + fadeWidth)
                targetAlpha = Mathf.Lerp(visible.a, explored.a,
                              (dist - sightRadius) / fadeWidth);
            else
                targetAlpha = Mathf.Max(explored.a, minAlpha[i]);

            // Explored state: once revealed, never goes darker than explored
            if (targetAlpha < minAlpha[i])
                minAlpha[i] = targetAlpha;

            float finalAlpha = Mathf.Max(targetAlpha, minAlpha[i]);

            if (!Mathf.Approximately(colors[i].a, finalAlpha))
            {
                colors[i].a = finalAlpha;
                changed = true;
            }
        }

        if (changed) mesh.colors = colors;
    }
}
