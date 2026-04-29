using UnityEngine;
using UnityEngine.Rendering;

// Вешай на любой объект в сцене (например Main Camera или Directional Light).
// Форсирует настройки освещения каждый кадр,
// перекрывая DayNightLighting если он активен в редакторе.
[ExecuteAlways]
public class SceneLighting : MonoBehaviour
{
    [SerializeField] private Material skybox;
    [SerializeField] private Light    sun;
    [SerializeField] private Renderer skyPlane;
    [SerializeField] private Color    skyPlaneTop    = new Color(0.05f, 0.05f, 0.1f);
    [SerializeField] private Color    skyPlaneBottom = new Color(0.02f, 0.02f, 0.05f);

    [SerializeField] private Color skyColor      = new Color(0.1f, 0.1f, 0.15f);
    [SerializeField] private Color equatorColor  = new Color(0.05f, 0.05f, 0.1f);
    [SerializeField] private Color groundColor   = new Color(0.02f, 0.02f, 0.05f);
    [SerializeField] private Color sunColor      = Color.white;
    [SerializeField] private float sunIntensity  = 0.5f;

    void Awake() => Apply();
    void Update() => Apply();

    void Apply()
    {
        if (skybox != null && RenderSettings.skybox != skybox)
            RenderSettings.skybox = skybox;

        RenderSettings.ambientMode         = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor     = skyColor;
        RenderSettings.ambientEquatorColor = equatorColor;
        RenderSettings.ambientGroundColor  = groundColor;

        var activeSun = sun != null ? sun : RenderSettings.sun;
        if (activeSun != null)
        {
            activeSun.color     = sunColor;
            activeSun.intensity = sunIntensity;
        }

        if (skyPlane != null)
        {
            skyPlane.sharedMaterial.SetColor("_TopColor",    skyPlaneTop);
            skyPlane.sharedMaterial.SetColor("_BottomColor", skyPlaneBottom);
        }

        DynamicGI.UpdateEnvironment();
    }
}
