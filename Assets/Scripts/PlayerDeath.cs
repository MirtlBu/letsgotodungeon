using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerDeath : MonoBehaviour
{
    [SerializeField] private string respawnSceneName = "Overworld";
    [SerializeField] private string respawnTargetName = "campfire";

    private HealthSystem health;

    void Start()
    {
        health = GetComponent<HealthSystem>();
        health?.OnDeath.AddListener(OnDeath);
    }

    private void OnDeath()
    {
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

        GetComponent<CharacterMovement>()?.ForceUnground();
        FindObjectOfType<CameraController>()?.SnapToTarget();
    }
}
