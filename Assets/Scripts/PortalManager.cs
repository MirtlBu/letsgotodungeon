using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    public string DestinationPortalId { get; private set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public void Travel(string targetScene, string destinationPortalId)
    {
        DestinationPortalId = destinationPortalId;
        SceneTransition.Instance.GoToScene(targetScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (string.IsNullOrEmpty(DestinationPortalId)) return;
        StartCoroutine(TeleportNextFrame());
    }

    private System.Collections.IEnumerator TeleportNextFrame()
    {
        yield return null; // wait for Start() to run on all scene objects

        Portal destination = FindDestinationPortal(DestinationPortalId);
        DestinationPortalId = null;

        if (destination == null) yield break;

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) yield break;

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        Vector3 spawnPoint = destination.SpawnPoint + Vector3.up * 0.2f;
        player.transform.position = spawnPoint;
        if (cc != null) cc.enabled = true;

        var movement = player.GetComponent<CharacterMovement>();
        movement?.ForceUnground();

        FindObjectOfType<CameraController>()?.SnapToTarget();
    }

    private Portal FindDestinationPortal(string portalId)
    {
        foreach (var portal in FindObjectsByType<Portal>(FindObjectsSortMode.None))
        {
            if (portal.portalId == portalId)
                return portal;
        }
        return null;
    }
}
