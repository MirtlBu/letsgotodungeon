using UnityEngine;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }

    public Transform target;
    public float smoothSpeed = 10f;

    [SerializeField] private Vector3 offset;

    [ContextMenu("Capture Offset From Scene")]
    void CaptureOffset()
    {
        var player = GameObject.FindWithTag("Player");
        if (player != null)
            offset = transform.position - player.transform.position;
    }

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        if (target == null)
            target = GameObject.FindWithTag("Player")?.transform;
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
