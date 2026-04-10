using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private string respawnSceneName = "Overworld";
    [SerializeField] private string respawnTargetName = "campfire";
    [SerializeField] private float deathDelay = 2f;

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
        GetComponent<CharacterMovement>().enabled = false;
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
            transform.position = target.transform.position + new Vector3(2f, 0.5f, 0f);
            if (cc != null) cc.enabled = true;
        }
        else
        {
            Debug.LogWarning($"[PlayerDeath] GameObject '{respawnTargetName}' не найден в сцене '{respawnSceneName}'");
        }

        isDying = false;
        GetComponent<CharacterMovement>().enabled = true;
        GetComponent<PlayerAttack>().enabled = true;
        var anim = GetComponent<Animator>();
        if (anim)
        {
            anim.applyRootMotion = true;
            anim.ResetTrigger("dying");
            anim.ResetTrigger("attack");
            anim.ResetTrigger("impact");
            anim.Play("Idle-Walk-Run", 0, 0f);
        }
        GetComponent<CharacterMovement>()?.ForceUnground();
        FindObjectOfType<CameraController>()?.SnapToTarget();
    }
}
