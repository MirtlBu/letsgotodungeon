using UnityEngine;
using UnityEngine.UIElements;

public class HUDController : MonoBehaviour
{
    [SerializeField] private HealthSystem healthSystem;

    private VisualElement healthBarFill;
    private Label coinLabel;

    void Start()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        healthBarFill = root.Q<VisualElement>("health-bar-fill");
        coinLabel = root.Q<Label>("coin-label");
    }

    void Update()
    {
        if (healthSystem != null)
            healthBarFill.style.width = Length.Percent(healthSystem.HealthPercent * 100f);

        if (CoinCounter.Instance != null)
            coinLabel.text = CoinCounter.Instance.GetCount().ToString();
    }
}
