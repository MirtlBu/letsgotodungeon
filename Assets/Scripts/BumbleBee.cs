using UnityEngine;

// Вешай на GameObject с мешем шмеля (или просто capsule для теста).
// Шмель летает по случайным точкам внутри заданной зоны,
// иногда зависает на месте, наклоняется в сторону полёта.
public class BumbleBee : MonoBehaviour
{
    [Header("Зона полёта")]
    [SerializeField] private Vector3 zoneCenter;   // в мировых координатах; если (0,0,0) — берётся стартовая позиция
    [SerializeField] private Vector3 zoneSize = new Vector3(4f, 1.5f, 4f);

    [Header("Движение")]
    [SerializeField] private float moveSpeed    = 1.2f;
    [SerializeField] private float turnSpeed    = 3f;
    [SerializeField] private float waypointReachDist = 0.2f;

    [Header("Зависание")]
    [SerializeField] private float hoverChance  = 0.3f;   // вероятность зависнуть в следующей точке
    [SerializeField] private float hoverTimeMin = 0.5f;
    [SerializeField] private float hoverTimeMax = 2f;

    [Header("Покачивание")]
    [SerializeField] private float bobAmplitude = 0.04f;
    [SerializeField] private float bobFrequency = 8f;

    private Vector3 _target;
    private bool    _hovering;
    private float   _hoverTimer;
    private Vector3 _startPos;
    private float   _bobOffset;

    void Start()
    {
        _startPos  = transform.position;
        if (zoneCenter == Vector3.zero) zoneCenter = _startPos;
        _bobOffset = Random.Range(0f, Mathf.PI * 2f); // разные фазы у разных шмелей
        PickNextTarget();
    }

    void Update()
    {
        if (_hovering)
        {
            _hoverTimer -= Time.deltaTime;
            if (_hoverTimer <= 0f) PickNextTarget();
        }
        else
        {
            MoveToTarget();
            if (Vector3.Distance(transform.position, _target) < waypointReachDist)
                OnReachedTarget();
        }

        // покачивание вверх-вниз
        Vector3 p = transform.position;
        p.y += Mathf.Sin(Time.time * bobFrequency + _bobOffset) * bobAmplitude * Time.deltaTime * 60f;
        transform.position = p;
    }

    void MoveToTarget()
    {
        Vector3 dir = (_target - transform.position).normalized;

        // поворот к цели
        if (dir != Vector3.zero)
        {
            Quaternion look = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, turnSpeed * Time.deltaTime);
        }

        transform.position += dir * moveSpeed * Time.deltaTime;
    }

    void OnReachedTarget()
    {
        if (Random.value < hoverChance)
        {
            _hovering   = true;
            _hoverTimer = Random.Range(hoverTimeMin, hoverTimeMax);
        }
        else
        {
            PickNextTarget();
        }
    }

    void PickNextTarget()
    {
        _hovering = false;
        _target   = zoneCenter + new Vector3(
            Random.Range(-zoneSize.x * 0.5f, zoneSize.x * 0.5f),
            Random.Range(-zoneSize.y * 0.5f, zoneSize.y * 0.5f),
            Random.Range(-zoneSize.z * 0.5f, zoneSize.z * 0.5f)
        );
    }

    // Визуализация зоны в редакторе
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.3f);
        Vector3 c = (zoneCenter == Vector3.zero && !Application.isPlaying) ? transform.position : zoneCenter;
        Gizmos.DrawWireCube(c, zoneSize);
    }
}
