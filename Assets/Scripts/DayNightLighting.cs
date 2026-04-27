using UnityEngine;

// Вешай на любой GameObject в Overworld.
// В Inspector:
//   1. Заполни phases (минимум 2), порядок по startHour
//   2. В каждую фазу положи свой skybox-материал (любой шейдер)
//   3. Укажи ссылку на Directional Light (sun)
[ExecuteAlways]
public class DayNightLighting : MonoBehaviour
{
    [System.Serializable]
    public class LightingPhase
    {
        public string   name;
        [Range(0, 24)]  public float    startHour;
        public Material skybox;
        public Color    skyColor        = new Color(0.5f, 0.7f, 1f);
        public Color    equatorColor    = new Color(0.8f, 0.8f, 0.8f);
        public Color    groundColor     = new Color(0.3f, 0.25f, 0.2f);
        public Color    sunColor        = Color.white;
        [Range(0f, 2f)] public float    sunIntensity    = 1f;
    }

    [SerializeField] private LightingPhase[] phases;
    [SerializeField] private Light sun;
    [SerializeField] private Renderer skyPlane;

    [Tooltip("Горизонтальный угол солнца (Y). 0 = север, 90 = восток")]
    [SerializeField] [Range(0f, 360f)] private float sunYaw = 45f;

    private float giTimer;
    private const float GIInterval = 1f;

    void OnEnable()
    {
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex != 0)
        {
            enabled = false;
            return;
        }
        if (phases != null)
            System.Array.Sort(phases, (a, b) => a.startHour.CompareTo(b.startHour));
        Apply();
    }

    void Update()
    {
        Apply();
        giTimer += Time.deltaTime;
        if (giTimer >= GIInterval)
        {
            giTimer = 0f;
            DynamicGI.UpdateEnvironment();
            float h = GetCurrentHour();
            int   p = GetPhaseIndex(h);
        }
    }

    void Apply()
    {
        if (phases == null || phases.Length < 1) return;

        float hour    = GetCurrentHour();
        int   fromIdx = GetPhaseIndex(hour);
        var   phase   = phases[fromIdx];

        // Skybox — просто свап при смене фазы
        if (phase.skybox != null)
        {
            if (RenderSettings.skybox != phase.skybox)
            RenderSettings.skybox = phase.skybox;

            // Лерп цветов между текущей и следующей фазой
            int   si  = (fromIdx + 1) % phases.Length;
            float st  = GetBlend(phase.startHour, phases[si].startHour, hour);
            phase.skybox.SetColor("_SkyTint",     Color.Lerp(phase.skyColor,     phases[si].skyColor,     st));
            phase.skybox.SetColor("_EquatorColor", Color.Lerp(phase.equatorColor, phases[si].equatorColor, st));
            phase.skybox.SetColor("_GroundColor",  Color.Lerp(phase.groundColor,  phases[si].groundColor,  st));

            DynamicGI.UpdateEnvironment();
#if UNITY_EDITOR
            UnityEditor.SceneView.RepaintAll();
#endif
        }

        if (sun != null)
        {
            int   toIdx = (fromIdx + 1) % phases.Length;
            float t     = GetBlend(phase.startHour, phases[toIdx].startHour, hour);
            sun.color     = Color.Lerp(phase.sunColor,     phases[toIdx].sunColor,     t);
            sun.intensity = Mathf.Lerp(phase.sunIntensity, phases[toIdx].sunIntensity, t);

            RenderSettings.ambientMode        = UnityEngine.Rendering.AmbientMode.Trilight;
            RenderSettings.ambientSkyColor     = Color.Lerp(phase.skyColor,     phases[toIdx].skyColor,     t);
            RenderSettings.ambientEquatorColor = Color.Lerp(phase.equatorColor, phases[toIdx].equatorColor, t);
            RenderSettings.ambientGroundColor  = Color.Lerp(phase.groundColor,  phases[toIdx].groundColor,  t);

            float sunAngle = (hour / 24f) * 360f - 90f;
            sun.transform.rotation = Quaternion.Euler(sunAngle, sunYaw, 0f);
        }

        if (skyPlane != null)
        {
            int   ni = (fromIdx + 1) % phases.Length;
            float nt = GetBlend(phase.startHour, phases[ni].startHour, hour);
            skyPlane.material.SetColor("_TopColor",    Color.Lerp(phase.skyColor,     phases[ni].skyColor,     nt));
            skyPlane.material.SetColor("_BottomColor", Color.Lerp(phase.equatorColor, phases[ni].equatorColor, nt));
        }
    }

    int GetPhaseIndex(float hour)
    {
        int result = 0;
        for (int i = 0; i < phases.Length; i++)
            if (phases[i].startHour <= hour) result = i;
        return result;
    }

    float GetBlend(float fromHour, float toHour, float currentHour)
    {
        if (toHour <= fromHour) toHour += 24f;
        float cur = currentHour < fromHour ? currentHour + 24f : currentHour;
        return Mathf.Clamp01(Mathf.InverseLerp(fromHour, toHour, cur));
    }

    float GetCurrentHour()
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) return previewHour;
#endif
        return GameClock.Instance != null ? GameClock.Instance.TotalMinutes / 60f % 24f : 12f;
    }

#if UNITY_EDITOR
    [Header("Editor Preview")]
    [Range(0, 24)] public float previewHour = 12f;
#endif
}
