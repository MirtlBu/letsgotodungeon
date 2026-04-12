using UnityEngine;
using UnityEngine.UIElements;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance { get; private set; }

    private UIDocument balloonDoc;

    private VisualElement balloon;
    private Label balloonText;
    private Transform targetTransform;

    private VisualElement playerBalloon;
    private Label playerBalloonText;
    private Transform playerTargetTransform;

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

        playerBalloon = root.Q("player-balloon");
        playerBalloonText = root.Q<Label>("player-balloon-text");

        if (balloon == null) Debug.LogError("InteractionUI: balloon element not found in UXML!");
        else balloon.style.display = DisplayStyle.None;

        if (playerBalloon == null) Debug.LogError("InteractionUI: player-balloon element not found in UXML!");
        else playerBalloon.style.display = DisplayStyle.None;
    }

    void Update()
    {
        UpdateBalloonPosition(balloon, targetTransform);
        UpdateBalloonPosition(playerBalloon, playerTargetTransform);
    }

    private void UpdateBalloonPosition(VisualElement el, Transform anchor)
    {
        if (anchor == null || el == null) return;
        if (el.style.display == DisplayStyle.None) return;

        Vector3 worldPos = anchor.position + Vector3.up * worldOffsetY;
        Vector2 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        screenPos.y = Screen.height - screenPos.y;

        var panel = balloonDoc.rootVisualElement.panel;
        if (panel == null) return;
        Vector2 panelPos = RuntimePanelUtils.ScreenToPanel(panel, screenPos);

        el.style.left = panelPos.x - el.resolvedStyle.width * 0.5f;
        el.style.top = panelPos.y - el.resolvedStyle.height;
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

    public void ShowPlayer(string text, Transform anchor)
    {
        playerTargetTransform = anchor;
        playerBalloonText.text = text;
        playerBalloon.style.display = DisplayStyle.Flex;
    }

    public void HidePlayer()
    {
        playerTargetTransform = null;
        playerBalloon.style.display = DisplayStyle.None;
    }
}
