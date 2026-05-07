using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class MainMenuController : MonoBehaviour
{
    [SerializeField] private string overworldScene = "Overworld";

    void Start()
    {
        Time.timeScale = 1f;

        var root = GetComponent<UIDocument>().rootVisualElement;

        root.Q<Button>("start-btn")?.RegisterCallback<ClickEvent>(_ => StartNewGame());
        root.Q<Button>("credits-btn")?.RegisterCallback<ClickEvent>(_ => ShowCredits());
        root.Q<Button>("credits-close-btn")?.RegisterCallback<ClickEvent>(_ => HideCredits(root));
    }

    private void StartNewGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("SpawnAtDefault", 1);
        PlayerPrefs.Save();

        DestroyAllDontDestroyOnLoadObjects();
        SceneManager.LoadScene(overworldScene);
    }

    private void DestroyAllDontDestroyOnLoadObjects()
    {
        var temp = new GameObject("__ddol_probe__");
        DontDestroyOnLoad(temp);
        var ddolScene = temp.scene;
        Destroy(temp);

        foreach (var go in ddolScene.GetRootGameObjects())
            Destroy(go);
    }

    private void ShowCredits()
    {
        var root = GetComponent<UIDocument>().rootVisualElement;
        root.Q<VisualElement>("credits-panel")?.RemoveFromClassList("hidden");
    }

    private void HideCredits(VisualElement root)
    {
        root.Q<VisualElement>("credits-panel")?.AddToClassList("hidden");
    }



}
