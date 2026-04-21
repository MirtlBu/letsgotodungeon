using UnityEngine;

public class ShadowController : MonoBehaviour
{
    public Transform player;
    public LayerMask shadowLayer;
    public float shadowRadius = 10f;

    private float radiuscircle => shadowRadius * shadowRadius;

    private GameObject shadowPlane;
    private Mesh mesh;
    private Vector3[] vertices;
    private Color[] colors;

    public void Register(GameObject plane)
    {
        shadowPlane = plane;
        Initialize();
    }

    public void Unregister()
    {
        shadowPlane = null;
        mesh = null;
        vertices = null;
        colors = null;
    }

    void Update()
    {
        if (shadowPlane == null || player == null || mesh == null) return;

        Ray r = new Ray(transform.position, player.position - transform.position);
        if (!Physics.Raycast(r, out RaycastHit hit, 1000, shadowLayer, QueryTriggerInteraction.Collide)) return;

        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 v = shadowPlane.transform.TransformPoint(vertices[i]);
            float distance = Vector3.SqrMagnitude(v - hit.point);
            if (distance < radiuscircle)
            {
                float alpha = Mathf.Min(colors[i].a, distance / radiuscircle);
                colors[i].a = alpha;
            }
        }

        mesh.colors = colors;
    }

    private void Initialize()
    {
        mesh = shadowPlane.GetComponent<MeshFilter>().mesh;
        vertices = mesh.vertices;
        colors = new Color[vertices.Length];
        for (int i = 0; i < colors.Length; i++)
            colors[i] = Color.black;
        mesh.colors = colors;
    }
}
