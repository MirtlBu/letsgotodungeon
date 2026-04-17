using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private string respawnSceneName = "Overworld";
    [SerializeField] private string respawnTargetName = "campfire";
    [SerializeField] private float deathDelay = 2f;
    [SerializeField] private string respawnBalloonText = "Ugh... not again...";

    private HealthSystem health;
    private bool isDying;

    void Start()
    {
        health = GetComponent<HealthSystem>();
        health?.OnDeath.AddListener(OnDeath);
    }

    private void OnDeath()
    {
        if (isDying) return;
        isDying = true;
        GetComponent<CharacterMovement>().IsLocked = true;
        GetComponent<PlayerAttack>().enabled = false;
        StartCoroutine(DyingRoutine());
    }

    private IEnumerator DyingRoutine()
    {
        var anim = GetComponent<Animator>();
        var combat = GetComponent<PlayerCombat>();
        var cc = GetComponent<CharacterController>();

        if (anim) anim.applyRootMotion = false;

        // Сначала knockback
        if (combat != null && combat.LastAttackerPosition != Vector3.zero)
        {
            Vector3 dir = transform.position - combat.LastAttackerPosition;
            dir.y = 0f;
            dir.Normalize();

            float elapsed = 0f;
            float knockDuration = combat.LongKnockbackDuration;
            float knockForce = combat.LongKnockbackForce;
            while (elapsed < knockDuration)
            {
                float t = 1f - elapsed / knockDuration;
                cc?.Move(dir * (knockForce * t * Time.deltaTime));
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        // Потом анимация смерти
        anim?.SetTrigger("dying");

        // Ждём чтобы анимация успела проиграться, потом fade + переход
        yield return new WaitForSeconds(deathDelay);

        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneTransition.Instance.suppressAutoFadeIn = true;
        SceneTransition.Instance.GoToScene(respawnSceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != respawnSceneName) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        StartCoroutine(RespawnNextFrame());
    }

    private IEnumerator RespawnNextFrame()
    {
        yield return null; // ждём Start() всех объектов сцены

        health?.Heal(float.MaxValue);

        GameObject target = GameObject.Find(respawnTargetName);
        if (target != null)
        {
            var cc = GetComponent<CharacterController>();
            if (cc != null) cc.enabled = false;
            transform.position = target.transform.position + new Vector3(2.5f, 0f, -0.5f);
            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning($"[PlayerDeath] GameObject '{respawnTargetName}' не найден в сцене '{respawnSceneName}'");
        }

        var anim = GetComponent<Animator>();
        if (anim)
        {
            anim.applyRootMotion = false;
            anim.ResetTrigger("dying");
            anim.ResetTrigger("attack");
            anim.ResetTrigger("impact");
            anim.SetFloat("speed", 0f);
            anim.Play("player_gettingup", 0, 0f);
        }

        GetComponent<CharacterMovement>()?.ForceUnground();
        FindObjectOfType<CameraController>()?.SnapToTarget();

        // Все готово — теперь можно убрать черный экран
        SceneTransition.Instance.suppressAutoFadeIn = false;
        SceneTransition.Instance.TriggerFadeIn();

        // Ждём пока проиграется анимация вставания
        yield return null; // один кадр чтобы аниматор переключился
        float gettingUpLength = 1.5f;
        if (anim != null)
        {
            var info = anim.GetCurrentAnimatorStateInfo(0);
            if (info.IsName("player_gettingup")) gettingUpLength = info.length;
        }

        InteractionUI.Instance?.Show(respawnBalloonText, transform);
        yield return new WaitForSeconds(gettingUpLength);
        InteractionUI.Instance?.Hide();

        isDying = false;
        GetComponent<CharacterMovement>().IsLocked = false;
        GetComponent<PlayerAttack>().enabled = true;
        anim?.Play("Idle-Walk-Run", 0, 0f);
    }
}
