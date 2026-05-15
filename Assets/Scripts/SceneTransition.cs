using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private float fadeDuration = 0.4f;

    private VisualElement fadeOverlay;
    private VisualElement cutsceneBalloon;
    private Label cutsceneLabel;
    private bool isTransitioning;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public bool suppressAutoFadeIn;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CreateOverlay();
        if (!suppressAutoFadeIn)
            StartCoroutine(FadeIn());
    }

    public void TriggerFadeIn()
    {
        StartCoroutine(FadeIn());
    }

    public void GoToScene(string sceneName)
    {
        if (isTransitioning) return;
        StartCoroutine(Transition(sceneName));
    }

    public void FadeAndDo(System.Action onBlack)
    {
        if (isTransitioning) return;
        StartCoroutine(FadeAndDoRoutine(onBlack));
    }

    private IEnumerator FadeAndDoRoutine(System.Action onBlack)
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeOut());
        onBlack?.Invoke();
        yield return new WaitForSeconds(0.3f);
        yield return StartCoroutine(FadeIn());
        isTransitioning = false;
    }

    private IEnumerator Transition(string sceneName)
    {
        isTransitioning = true;
        yield return StartCoroutine(FadeOut());
        yield return SceneManager.LoadSceneAsync(sceneName);
        isTransitioning = false;
    }

    private IEnumerator FadeOut()
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetOverlayAlpha(t / fadeDuration);
            yield return null;
        }
        SetOverlayAlpha(1f);
    }

    private IEnumerator FadeIn()
    {
        SetOverlayAlpha(1f);
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            SetOverlayAlpha(1f - t / fadeDuration);
            yield return null;
        }
        SetOverlayAlpha(0f);
    }

    // — Cutscene helpers —

    public Coroutine FadeOutManual(float duration = -1f)
    {
        return StartCoroutine(FadeOutTimed(duration > 0 ? duration : fadeDuration));
    }

    public Coroutine FadeInManual(float duration = -1f)
    {
        return StartCoroutine(FadeInTimed(duration > 0 ? duration : fadeDuration));
    }

    public void ShowCutsceneText(string text)
    {
        if (cutsceneBalloon == null) return;
        cutsceneLabel.text = text;
        cutsceneBalloon.style.display = DisplayStyle.Flex;
    }

    public void HideCutsceneText()
    {
        if (cutsceneBalloon != null)
            cutsceneBalloon.style.display = DisplayStyle.None;
    }

    private IEnumerator FadeOutTimed(float dur)
    {
        float t = 0f;
        while (t < dur) { t += Time.deltaTime; SetOverlayAlpha(t / dur); yield return null; }
        SetOverlayAlpha(1f);
    }

    private IEnumerator FadeInTimed(float dur)
    {
        SetOverlayAlpha(1f);
        float t = 0f;
        while (t < dur) { t += Time.deltaTime; SetOverlayAlpha(1f - t / dur); yield return null; }
        SetOverlayAlpha(0f);
    }

    // — Internal —

    private void CreateOverlay()
    {
        var uiDoc = GetComponent<UIDocument>();
        if (uiDoc == null) return;
        var root = uiDoc.rootVisualElement;
        root.Clear();

        fadeOverlay = new VisualElement();
        fadeOverlay.style.position = Position.Absolute;
        fadeOverlay.style.left = 0; fadeOverlay.style.top = 0;
        fadeOverlay.style.right = 0; fadeOverlay.style.bottom = 0;
        fadeOverlay.style.backgroundColor = new StyleColor(Color.black);
        fadeOverlay.style.opacity = 1f;
        fadeOverlay.pickingMode = PickingMode.Ignore;
        fadeOverlay.style.justifyContent = Justify.Center;
        fadeOverlay.style.alignItems = Align.Center;

        cutsceneBalloon = new VisualElement();
        cutsceneBalloon.style.display = DisplayStyle.None;
        cutsceneBalloon.style.backgroundColor = new StyleColor(Color.white);
        cutsceneBalloon.style.borderTopLeftRadius = 30;
        cutsceneBalloon.style.borderTopRightRadius = 30;
        cutsceneBalloon.style.borderBottomLeftRadius = 30;
        cutsceneBalloon.style.borderBottomRightRadius = 30;
        cutsceneBalloon.style.paddingTop = 12;
        cutsceneBalloon.style.paddingBottom = 12;
        cutsceneBalloon.style.paddingLeft = 24;
        cutsceneBalloon.style.paddingRight = 24;
        cutsceneBalloon.style.maxWidth = new StyleLength(new Length(60, LengthUnit.Percent));
        cutsceneBalloon.style.alignItems = Align.Center;

        cutsceneLabel = new Label();
        cutsceneLabel.style.color = new StyleColor(new Color(0.04f, 0.08f, 0.31f));
        cutsceneLabel.style.fontSize = 20;
        cutsceneLabel.style.whiteSpace = WhiteSpace.Normal;
        cutsceneLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
        cutsceneBalloon.Add(cutsceneLabel);
        fadeOverlay.Add(cutsceneBalloon);

        root.Add(fadeOverlay);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (fadeOverlay != null)
            fadeOverlay.style.opacity = alpha;
    }
}
