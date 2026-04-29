using UnityEngine;
using UnityEngine.SceneManagement;

// Вешай на камеру. Переключает sky_background объекты при смене сцены.
public class SkyBackgroundSwitcher : MonoBehaviour
{
    [SerializeField] private GameObject overworldSky;
    [SerializeField] private GameObject dungeonSky;

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Apply(SceneManager.GetActiveScene().name);
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode) => Apply(scene.name);

    void Apply(string sceneName)
    {
        bool inDungeon = sceneName == "Dungeon";
        if (overworldSky != null) overworldSky.SetActive(!inDungeon);
        if (dungeonSky   != null) dungeonSky.SetActive(inDungeon);
    }
}
