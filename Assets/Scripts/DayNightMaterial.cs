using UnityEngine;

// Вешай на объект с MeshRenderer.
// Создаёт дочерний объект с тем же мешем и emission материалом.
// Включает/выключает его в зависимости от времени суток.
public class DayNightMaterial : MonoBehaviour
{
    [SerializeField] private Material emissionMaterial;

    [Range(0, 24)] public float hourOn  = 22f;
    [Range(0, 24)] public float hourOff = 6f;

    private GameObject _emissionChild;

    void Start()
    {
        var mf = GetComponent<MeshFilter>();
        if (mf == null || emissionMaterial == null)
        {
            Debug.LogWarning($"[DayNightMaterial] {name}: нужен MeshFilter и emissionMaterial", this);
            enabled = false;
            return;
        }

        _emissionChild = new GameObject("EmissionLayer");
        _emissionChild.transform.SetParent(transform, false);

        var childMf = _emissionChild.AddComponent<MeshFilter>();
        childMf.sharedMesh = mf.sharedMesh;

        var childMr = _emissionChild.AddComponent<MeshRenderer>();
        childMr.sharedMaterial = emissionMaterial;

        _emissionChild.SetActive(ShouldBeOn(GetCurrentHour()));
    }

    void Update()
    {
        bool on = ShouldBeOn(GetCurrentHour());
        if (_emissionChild.activeSelf != on)
            _emissionChild.SetActive(on);
    }

    bool ShouldBeOn(float hour)
    {
        if (hourOn < hourOff)
            return hour >= hourOn && hour < hourOff;
        else // переход через полночь (например 22 → 6)
            return hour >= hourOn || hour < hourOff;
    }

    float GetCurrentHour() =>
        GameClock.Instance != null ? GameClock.Instance.TotalMinutes / 60f % 24f : 12f;
}
