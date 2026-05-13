using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [Header("Music")]
    [SerializeField] private AudioClip overworldMusic;
    [SerializeField] private AudioClip dungeonMusic;
    [SerializeField] private AudioClip mainMenuMusic;
    [SerializeField] [Range(0f, 1f)] private float musicVolume = 0.5f;

    private AudioSource musicSource;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        musicSource = gameObject.AddComponent<AudioSource>();
        musicSource.loop = true;
        musicSource.volume = musicVolume;
        musicSource.playOnAwake = false;
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AudioClip clip = scene.name switch
        {
            "MainMenu" => mainMenuMusic,
            "Overworld" => overworldMusic,
            "Dungeon"   => dungeonMusic,
            _           => null
        };

        PlayMusic(clip);
    }

    private void PlayMusic(AudioClip clip)
    {
        if (clip == null) { musicSource.Stop(); return; }
        if (musicSource.clip == clip) return;

        musicSource.clip = clip;
        musicSource.Play();
    }

    public void SwitchMusic(AudioClip clip) => PlayMusic(clip);

    public void SetMusicVolume(float volume)
    {
        musicVolume = volume;
        musicSource.volume = volume;
    }
}
