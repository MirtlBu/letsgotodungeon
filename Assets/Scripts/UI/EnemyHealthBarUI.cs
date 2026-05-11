using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class EnemyHealthBarUI : MonoBehaviour
{
    public static EnemyHealthBarUI Instance { get; private set; }

    [SerializeField] private float worldOffsetY = 2.2f;
    // ширина берётся из CSS (.enemy-bar-bg → width: 60px)
    private const float BarWidthPx = 30f;

    private UIDocument uiDoc;

    private class BarEntry
    {
        public VisualElement bg;
        public VisualElement fill;
        public Transform anchor;
        public HealthSystem health;
    }

    private readonly List<BarEntry> entries = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        uiDoc = GetComponent<UIDocument>();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        foreach (var e in entries)
            uiDoc.rootVisualElement.Remove(e.bg);
        entries.Clear();
    }

    public void Register(HealthSystem health, Transform anchor)
    {
        if (health == null) return;

        var bg = new VisualElement();
        bg.AddToClassList("enemy-bar-bg");

        var fill = new VisualElement();
        fill.AddToClassList("enemy-bar-fill");

        bg.Add(fill);
        bg.style.display = DisplayStyle.None; // скрыт до первого позиционирования
        uiDoc.rootVisualElement.Add(bg);

        entries.Add(new BarEntry { bg = bg, fill = fill, anchor = anchor, health = health });
    }

    public void Unregister(HealthSystem health)
    {
        var entry = entries.Find(e => e.health == health);
        if (entry == null) return;
        uiDoc.rootVisualElement.Remove(entry.bg);
        entries.Remove(entry);
    }

    void Update()
    {
        var panel = uiDoc.rootVisualElement.panel;
        if (panel == null || Camera.main == null) return;

        foreach (var e in entries)
        {
            if (e.anchor == null || !e.anchor.gameObject.activeInHierarchy)
            {
                e.bg.style.display = DisplayStyle.None;
                continue;
            }

            e.fill.style.width = Length.Percent(e.health.HealthPercent * 100f);

            Vector3 worldPos = e.anchor.position + Vector3.up * worldOffsetY;
            Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            screenPos.y = Screen.height - screenPos.y;
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPos);

            e.bg.style.left = panelPos.x - BarWidthPx * 0.5f;
            e.bg.style.top = panelPos.y;
            e.bg.style.display = DisplayStyle.Flex;
        }
    }
}
