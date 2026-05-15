using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class DamageNumbersUI : MonoBehaviour
{
    public static DamageNumbersUI Instance { get; private set; }

    [SerializeField] private float floatSpeed = 1.5f;
    [SerializeField] private float duration = 0.8f;

    private UIDocument doc;
    private VisualElement container;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        doc = GetComponent<UIDocument>();
        container = doc.rootVisualElement.Q("damage-container");
    }

    public void Show(float damage, Vector3 worldPos, bool isCrit, bool isPlayerDamage = false)
    {
        string text = isCrit ? $"CRIT {Mathf.RoundToInt(damage)}!" : Mathf.RoundToInt(damage).ToString();
        var label = new Label(text);
        label.AddToClassList("damage-number");
        if (isCrit) label.AddToClassList("damage-number-crit");
        else if (isPlayerDamage) label.AddToClassList("damage-number-player");
        container.Add(label);
        StartCoroutine(Animate(label, worldPos));
    }

    private IEnumerator Animate(Label label, Vector3 worldPos)
    {
        float elapsed = 0f;
        var panel = doc.rootVisualElement.panel;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            if (Camera.main == null) { container.Remove(label); yield break; }
            Vector3 offsetPos = worldPos + Vector3.up * (floatSpeed * elapsed);
            Vector2 screenPos = Camera.main.WorldToScreenPoint(offsetPos);
            screenPos.y = Screen.height - screenPos.y;
            Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPos);

            label.style.left = panelPos.x;
            label.style.top = panelPos.y;
            label.style.opacity = 1f - t;

            yield return null;
        }

        container.Remove(label);
    }
}
