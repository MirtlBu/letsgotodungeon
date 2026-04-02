using UnityEngine;
using UnityEngine.UIElements;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    private UIDocument balloonDoc;
    private VisualElement balloon;
    private Label balloonText;

    private Transform targetTransform;
    [SerializeField] private float worldOffsetY = 2.5f;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        balloonDoc = GetComponent<UIDocument>();
        var root = balloonDoc.rootVisualElement;
        balloon = root.Q("balloon");
        balloonText = root.Q<Label>("balloon-text");

        if (balloon == null) Debug.LogError("InteractionUI: balloon element not found in UXML!");
        else balloon.style.display = DisplayStyle.None;
    }

    void Update()
    {
        if (targetTransform == null) return;
        if (balloon.style.display == DisplayStyle.None) return;

        Vector3 worldPos = targetTransform.position + Vector3.up * worldOffsetY;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);

        // UI Toolkit: origin top-left, Screen: origin bottom-left
        screenPos.y = Screen.height - screenPos.y;

        var panel = balloonDoc.rootVisualElement.panel;
        if (panel == null) return;
        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPos);

        balloon.style.left = panelPos.x - balloon.resolvedStyle.width * 0.5f;
        balloon.style.top = panelPos.y - balloon.resolvedStyle.height;
    }

    public void Show(string text, Transform anchor)
    {
        targetTransform = anchor;
        balloonText.text = text;
        balloon.style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        targetTransform = null;
        balloon.style.display = DisplayStyle.None;
    }
}
