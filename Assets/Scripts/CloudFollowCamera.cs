using UnityEngine;

// Вешай на пустой GameObject "CloudLayer" (все облака — дочерние).
// CloudLayer НЕ является дочерним камеры — только следит за её позицией XZ.
// parallaxFactor: 1.0 = двигается вместе с камерой, 0.5 = вдвое медленнее (параллакс)
public class CloudFollowCamera : MonoBehaviour
{
    [SerializeField] [Range(0f, 1f)] private float parallaxFactor = 0.85f;

    private Camera cam;
    private Vector3 prevCamPos;

    void Start()
    {
        cam = Camera.main;
        prevCamPos = cam.transform.position;
    }

    void LateUpdate()
    {
        Vector3 delta = cam.transform.position - prevCamPos;
        delta.y = 0f; // высоту облаков не трогаем
        transform.position += delta * parallaxFactor;
        prevCamPos = cam.transform.position;
    }
}
