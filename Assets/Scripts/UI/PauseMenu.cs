using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class PauseMenu : MonoBehaviour
{
    public static PauseMenu Instance { get; private set; }

    [SerializeField] private string mainMenuScene = "MainMenu";

    private VisualElement root;
    private bool isPaused;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;
        root.style.display = DisplayStyle.None;

        root.Q<Button>("resume-btn")?.RegisterCallback<ClickEvent>(_ => Resume());
        root.Q<Button>("menu-btn")?.RegisterCallback<ClickEvent>(_ => GoToMainMenu());
        root.Q<Button>("credits-btn")?.RegisterCallback<ClickEvent>(_ => ShowCredits());
        root.Q<Button>("credits-close-btn")?.RegisterCallback<ClickEvent>(_ => HideCredits());

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused) Resume();
            else Pause();
        }
    }

    public void Pause()
    {
        isPaused = true;
        Time.timeScale = 0f;
        root.style.display = DisplayStyle.Flex;
    }

    public void Resume()
    {
        isPaused = false;
        Time.timeScale = 1f;
        root.style.display = DisplayStyle.None;
        HideCredits();
    }

    private void GoToMainMenu()
    {
        Resume();
        SceneTransition.Instance?.GoToScene(mainMenuScene);
    }

    private void ShowCredits()
    {
        root.Q<VisualElement>("credits-panel")?.RemoveFromClassList("hidden");
    }

    private void HideCredits()
    {
        root.Q<VisualElement>("credits-panel")?.AddToClassList("hidden");
    }

    // Скрываем паузу при смене сцены
    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        if (isPaused) Resume();
    }
}
