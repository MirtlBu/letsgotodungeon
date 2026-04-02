using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private string targetScene;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        SceneTransition.Instance.GoToScene(targetScene);
    }
}
