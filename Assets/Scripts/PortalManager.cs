using UnityEngine;
using UnityEngine.SceneManagement;

public class PortalManager : MonoBehaviour
{
    public static PortalManager Instance { get; private set; }

    public string DestinationPortalId { get; private set; }
    public bool SpawnAtDefault { get; set; }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SceneManager.sceneLoaded += OnSceneLoaded;

        if (PlayerPrefs.GetInt("SpawnAtDefault", 0) == 1)
        {
            SpawnAtDefault = true;
            PlayerPrefs.DeleteKey("SpawnAtDefault");
            PlayerPrefs.Save();
        }
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
        StartCoroutine(TeleportNextFrame());
    }

    private System.Collections.IEnumerator TeleportNextFrame()
    {
        yield return null; // wait for Start() to run on all scene objects

        GameObject player = GameObject.FindWithTag("Player");
        if (player == null) yield break;

        Vector3 spawnPoint;

        if (!string.IsNullOrEmpty(DestinationPortalId))
        {
            Portal destination = FindDestinationPortal(DestinationPortalId);
            DestinationPortalId = null;
            if (destination == null) yield break;
            spawnPoint = destination.SpawnPoint + Vector3.up * 1.1f;
        }
        else if (SpawnAtDefault)
        {
            SpawnAtDefault = false;
            GameObject spawnObj = GameObject.FindWithTag("PlayerSpawn");
            if (spawnObj == null) yield break;
            spawnPoint = spawnObj.transform.position;
        }
        else
        {
            // Continue — игрок остаётся там где был, только камеру снапаем
            FindObjectOfType<CameraController>()?.SnapToTarget();
            yield break;
        }

        var cc = player.GetComponent<CharacterController>();
        if (cc != null) cc.enabled = false;
        player.transform.position = spawnPoint;
        if (cc != null) cc.enabled = true;

        var movement = player.GetComponent<CharacterMovement>();
        if (movement != null)
        {
            movement.ForceUnground();
            movement.IsLocked = false;
            movement.enabled = true;
        }

        DialogueManager.Instance?.CancelDialogue();

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
