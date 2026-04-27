using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class CapsuleRing : MonoBehaviour
{
    [Header("Ring Settings")]
    [Min(1)] public int count = 8;
    public float radius = 3f;
    public float capsuleHeight = 1f;
    public float capsuleRadius = 0.2f;
    public CapsuleDirection capsuleDirection = CapsuleDirection.Vertical;

    [Header("")]
    public bool regenerateOnValidate = true;

    public enum CapsuleDirection { Vertical, Horizontal_X, Horizontal_Z }

    public void Generate()
    {
        // Удалить старые дочерние объекты
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else
#endif
                Destroy(transform.GetChild(i).gameObject);
        }

        float angleStep = 360f / count;

        for (int i = 0; i < count; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 pos = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);

            GameObject go = new GameObject($"Capsule_{i}");
            go.transform.SetParent(transform, false);
            go.transform.localPosition = pos;

            // Поворачиваем горизонтальные капсулы по касательной к кругу
            if (capsuleDirection != CapsuleDirection.Vertical)
            {
                float angleDeg = i * angleStep;
                if (capsuleDirection == CapsuleDirection.Horizontal_X)
                    go.transform.localRotation = Quaternion.Euler(0f, angleDeg, 90f);
                else // Horizontal_Z
                    go.transform.localRotation = Quaternion.Euler(90f, angleDeg, 0f);
            }

            CapsuleCollider col = go.AddComponent<CapsuleCollider>();
            col.height = capsuleHeight;
            col.radius = capsuleRadius;
            col.direction = capsuleDirection == CapsuleDirection.Vertical ? 1
                          : capsuleDirection == CapsuleDirection.Horizontal_X ? 0
                          : 2;
        }
    }

    public void Clear()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(transform.GetChild(i).gameObject);
            else
#endif
                Destroy(transform.GetChild(i).gameObject);
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (regenerateOnValidate)
            EditorApplication.delayCall += () =>
            {
                if (this != null) Generate();
            };
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(CapsuleRing))]
public class CapsuleRingEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        GUILayout.Space(8);
        CapsuleRing ring = (CapsuleRing)target;

        if (GUILayout.Button("Generate"))
            ring.Generate();

        if (GUILayout.Button("Clear"))
            ring.Clear();
    }
}
#endif
