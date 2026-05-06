using UnityEngine;

// Вешай на CloudLayer. Внутри CloudLayer сделай два дочерних объекта:
// GroupA и GroupB — в каждом 3-4 облака.
// Группы движутся по ветру, когда одна уходит за камеру — телепортируется за другую.
public class CloudGroupCycler : MonoBehaviour
{
    [Header("Groups")]
    [SerializeField] private Transform groupA;
    [SerializeField] private Transform groupB;

    [Header("Wind")]
    [SerializeField] private Vector3 windDirection = Vector3.right;
    [SerializeField] private float speed = 2f;

    [Header("Cycle")]
    [Tooltip("Расстояние в направлении ветра за которое группа считается вышедшей из камеры")]
    [SerializeField] private float exitDistance = 40f;
    [Tooltip("Расстояние между группами — обычно равно exitDistance")]
    [SerializeField] private float groupSpacing = 40f;

    private Camera cam;
    private Vector3 windDir;

    void Start()
    {
        cam = Camera.main;
        windDir = windDirection.normalized;
    }

    void Update()
    {
        Vector3 move = windDir * speed * Time.deltaTime;
        groupA.position += move;
        groupB.position += move;

        CheckAndWrap(groupA, groupB);
        CheckAndWrap(groupB, groupA);
    }

    // Проверяем насколько группа ушла вперёд камеры в направлении ветра
    private void CheckAndWrap(Transform group, Transform other)
    {
        float groupAlong = Vector3.Dot(group.position, windDir);
        float camAlong   = Vector3.Dot(cam.transform.position, windDir);

        // Если группа ушла вперёд дальше exitDistance от камеры
        if (groupAlong - camAlong > exitDistance)
        {
            // Телепортируем её за другую группу (назад на groupSpacing * 2)
            group.position -= windDir * groupSpacing * 2f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.5f, 0.8f, 1f, 0.4f);
        if (groupA) Gizmos.DrawWireSphere(groupA.position, 2f);
        if (groupB) Gizmos.DrawWireSphere(groupB.position, 2f);
    }
}
