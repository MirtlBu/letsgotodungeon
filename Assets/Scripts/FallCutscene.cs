using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

// Вешай на пустой GameObject с BoxCollider (Is Trigger) в точке падения.
public class FallCutscene : MonoBehaviour
{
    [Header("Restart")]
    [SerializeField] private bool restartGame = true;
    [SerializeField] private Transform spawnPoint;       // только если restartGame = false
    [SerializeField] private string overworldScene = "Overworld";

    [Header("Text")]
    [SerializeField] [TextArea] private string text1 = "...";
    [SerializeField] private float text1Duration = 2.5f;
    [SerializeField] private float textGapDuration = 1.0f;
    [SerializeField] [TextArea] private string text2 = "...";
    [SerializeField] private float text2Duration = 2.5f;
    [Tooltip("Пауза после последнего balloon перед появлением на спавне (сек)")]
    [SerializeField] private float delayAfterText = 1.5f;

    [Header("Animation")]
    [SerializeField] private string animTrigger = "fall_land";

    [Header("Timing")]
    [Tooltip("Время плавного замедления гравитации (сек)")]
    [SerializeField] private float slowFallDuration = 1.5f;
    [Tooltip("Пауза после замедления перед началом затемнения (сек)")]
    [SerializeField] private float delayBeforeFade = 0.5f;
    [SerializeField] private float fadeOutDuration = 1.0f;
    [SerializeField] private float fadeInDuration = 0.8f;

    [Header("Particles")]
    [SerializeField] private ParticleSystem fallParticles;
    [Tooltip("Через сколько секунд после старта катсцены включить партиклс")]
    [SerializeField] private float particleStartDelay = 0.3f;

    private bool triggered;

    void OnTriggerEnter(Collider other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;
        triggered = true;
        StartCoroutine(Play(other.gameObject));
    }

    private IEnumerator Play(GameObject player)
    {
        var movement = player.GetComponent<CharacterMovement>();
        var animator = player.GetComponent<Animator>();

        // 1. Блокировать WASD, сбросить накопленную скорость падения
        if (movement != null)
        {
            movement.LockInput = true;
            movement.ResetVerticalVelocity();
        }

        // 2. Запустить анимацию
        if (!string.IsNullOrEmpty(animTrigger))
            animator?.SetTrigger(animTrigger);

        // 3. Запустить партиклс с задержкой
        if (fallParticles != null)
            StartCoroutine(StartParticlesDelayed());

        // 4. Плавно убираем гравитацию (замедление падения)
        float elapsed = 0f;
        float startGravity = movement != null ? movement.GravityMultiplier : 1f;
        while (elapsed < slowFallDuration)
        {
            elapsed += Time.deltaTime;
            if (movement != null)
                movement.GravityMultiplier = Mathf.Lerp(startGravity, 0f, elapsed / slowFallDuration);
            yield return null;
        }
        if (movement != null) movement.GravityMultiplier = 0f;

        // 5. Пауза перед затемнением
        yield return new WaitForSeconds(delayBeforeFade);

        // 6. Затемнение
        yield return SceneTransition.Instance.FadeOutManual(fadeOutDuration);

        // 7. Первый balloon
        SceneTransition.Instance.ShowCutsceneText(text1);
        yield return new WaitForSeconds(text1Duration);
        SceneTransition.Instance.HideCutsceneText();

        yield return new WaitForSeconds(textGapDuration);

        // Второй balloon
        SceneTransition.Instance.ShowCutsceneText(text2);
        yield return new WaitForSeconds(text2Duration);
        SceneTransition.Instance.HideCutsceneText();

        yield return new WaitForSeconds(delayAfterText);

        // 8. Сбросить состояние движения (на случай если restartGame = false)
        if (movement != null)
        {
            movement.GravityMultiplier = 1f;
            movement.LockInput = false;
        }

        // 9. Рестарт или телепорт
        if (restartGame)
            RestartGame();
        else
            TeleportAndFadeIn(player, movement);
    }

    private IEnumerator StartParticlesDelayed()
    {
        yield return new WaitForSeconds(particleStartDelay);
        fallParticles.Play();
    }

    private void RestartGame()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.SetInt("SpawnAtDefault", 1);
        PlayerPrefs.Save();

        var temp = new GameObject("__ddol_probe__");
        DontDestroyOnLoad(temp);
        var ddolScene = temp.scene;
        Destroy(temp);

        foreach (var go in ddolScene.GetRootGameObjects())
        {
            if (go.GetComponent<SceneTransition>() == null)
                Destroy(go);
        }

        SceneManager.LoadScene(overworldScene);
    }

    private void TeleportAndFadeIn(GameObject player, CharacterMovement movement)
    {
        if (spawnPoint != null)
        {
            var cc = player.GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            player.transform.position = spawnPoint.position;
            if (cc != null) cc.enabled = true;
        }

        StartCoroutine(FadeInAndUnlock(movement));
    }

    private IEnumerator FadeInAndUnlock(CharacterMovement movement)
    {
        yield return SceneTransition.Instance.FadeInManual(fadeInDuration);
        if (movement != null) movement.IsLocked = false;
        triggered = false;
    }
}
