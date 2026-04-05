using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public float smoothSpeed = 10f;

    private Vector3 offset;

    void Start()
    {
        if (target == null)
            target = GameObject.FindWithTag("Player")?.transform;

        if (target != null)
            offset = transform.position - target.position;
    }

    public void SnapToTarget()
    {
        if (target != null)
            transform.position = target.position + offset;
    }

    void LateUpdate()
    {
        if (target == null)
            return;

        Vector3 desiredPosition = target.position + offset;
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}
