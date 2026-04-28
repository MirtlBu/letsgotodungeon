using UnityEngine;

// Включает/выключает список объектов в зависимости от времени суток.
// Работает с Point Light, Particle System, любым GameObject.
public class DayNightToggle : MonoBehaviour
{
    [SerializeField] private GameObject[] targets;

    [Range(0, 24)] public float hourOn  = 22f;
    [Range(0, 24)] public float hourOff = 6f;

    void Start() => Apply();

    void Update() => Apply();

    void Apply()
    {
        bool on = ShouldBeOn(GetCurrentHour());
        foreach (var t in targets)
            if (t != null && t.activeSelf != on)
                t.SetActive(on);
    }

    bool ShouldBeOn(float hour)
    {
        if (hourOn < hourOff)
            return hour >= hourOn && hour < hourOff;
        else
            return hour >= hourOn || hour < hourOff;
    }

    float GetCurrentHour() =>
        GameClock.Instance != null ? GameClock.Instance.TotalMinutes / 60f % 24f : 12f;
}
