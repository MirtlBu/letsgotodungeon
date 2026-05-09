using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(CharacterMovement))]
public class FootstepAudio : MonoBehaviour
{
    [Header("Overworld")]
    [SerializeField] private AudioClip[] overworldClips;

    [Header("Dungeon")]
    [SerializeField] private AudioClip[] dungeonClips;

    [SerializeField] [Range(0f, 1f)] private float volume = 0.4f;
    [SerializeField] private float stepInterval = 0.42f;

    private CharacterMovement movement;
    private Animator anim;
    private AudioSource audioSource;
    private float stepTimer;
    private AudioClip[] currentClips;

    void Awake()
    {
        movement = GetComponent<CharacterMovement>();
        anim = GetComponent<Animator>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 0f;
        audioSource.playOnAwake = false;
    }

    void OnEnable()  => SceneManager.sceneLoaded += OnSceneLoaded;
    void OnDisable() => SceneManager.sceneLoaded -= OnSceneLoaded;

    void Start() => UpdateClipsForScene(SceneManager.GetActiveScene().name);

    private void OnSceneLoaded(Scene scene, LoadSceneMode _) => UpdateClipsForScene(scene.name);

    private void UpdateClipsForScene(string sceneName)
    {
        currentClips = sceneName == "Dungeon" ? dungeonClips : overworldClips;
    }

    void Update()
    {
        bool isMoving = movement.enabled
                        && movement.IsGrounded
                        && !movement.IsLocked
                        && (anim == null || anim.GetFloat("speed") >= 0.1f);

        if (isMoving)
        {
            stepTimer -= Time.deltaTime;
            if (stepTimer <= 0f)
            {
                PlayStep();
                stepTimer = stepInterval;
            }
        }
        else
        {
            stepTimer = 0f;
        }
    }

    private void PlayStep()
    {
        if (currentClips == null || currentClips.Length == 0) return;
        audioSource.PlayOneShot(currentClips[Random.Range(0, currentClips.Length)], volume);
    }
}
