using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class SceneTransition : MonoBehaviour
{
    public static SceneTransition Instance { get; private set; }

    [SerializeField] private float fadeDuration = 0.4f;

    private VisualElement fadeOverlay;
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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        CreateOverlay();
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
        fadeOverlay.style.opacity = 0f;
        fadeOverlay.pickingMode = PickingMode.Ignore;
        root.Add(fadeOverlay);
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (fadeOverlay != null)
            fadeOverlay.style.opacity = alpha;
    }
}
