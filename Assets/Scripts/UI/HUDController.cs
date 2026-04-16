using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [SerializeField] private HealthSystem healthSystem;

    private VisualElement healthBarFill;
    private Label coinLabel;
    private VisualElement buffIconsContainer;

    // Отслеживаем иконки по statType чтобы не пересоздавать каждый кадр
    private readonly Dictionary<StatType, VisualElement> buffIconMap = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        healthBarFill = root.Q<VisualElement>("health-bar-fill");
        coinLabel = root.Q<Label>("coin-label");
        buffIconsContainer = root.Q<VisualElement>("buff-icons");

        if (healthSystem == null)
            healthSystem = GameObject.FindWithTag("Player")?.GetComponent<HealthSystem>();
    }

    void Update()
    {
        if (healthSystem != null)
            healthBarFill.style.width = Length.Percent(healthSystem.HealthPercent * 100f);

        if (CoinCounter.Instance != null)
            coinLabel.text = CoinCounter.Instance.GetCount().ToString();

        UpdateBuffIcons();
    }

    private void UpdateBuffIcons()
    {
        if (buffIconsContainer == null || PlayerStats.Instance == null) return;

        var activeBuffs = PlayerStats.Instance.ActiveBuffs;

        // Добавляем иконки для новых баффов
        foreach (var buff in activeBuffs)
        {
            if (buff.definition.icon == null) continue;
            if (buffIconMap.ContainsKey(buff.definition.statType)) continue;

            var icon = new VisualElement();
            icon.AddToClassList("buff-icon");
            icon.style.backgroundImage = new StyleBackground(buff.definition.icon);
            buffIconsContainer.Add(icon);
            buffIconMap[buff.definition.statType] = icon;
        }

        // Удаляем иконки истёкших баффов
        var toRemove = new List<StatType>();
        foreach (var kvp in buffIconMap)
        {
            bool stillActive = false;
            foreach (var buff in activeBuffs)
                if (buff.definition.statType == kvp.Key) { stillActive = true; break; }
            if (!stillActive) toRemove.Add(kvp.Key);
        }

        foreach (var key in toRemove)
        {
            buffIconsContainer.Remove(buffIconMap[key]);
            buffIconMap.Remove(key);
        }
    }
}
