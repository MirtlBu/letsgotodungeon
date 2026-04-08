using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    [SerializeField] private HealthSystem healthSystem;

    private VisualElement healthBarFill;
    private Label coinLabel;

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

        if (healthSystem == null)
            healthSystem = GameObject.FindWithTag("Player")?.GetComponent<HealthSystem>();
    }

    void Update()
    {
        if (healthSystem != null)
            healthBarFill.style.width = Length.Percent(healthSystem.HealthPercent * 100f);

        if (CoinCounter.Instance != null)
            coinLabel.text = CoinCounter.Instance.GetCount().ToString();
    }
}
