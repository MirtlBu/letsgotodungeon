using UnityEngine;

// Вешай на GameObject облака.
// Облако движется по direction, при выходе за wrapDistance — телепортируется на другую сторону.
// Освещение (день/ночь) подхватывается автоматически через URP Lit + RenderSettings ambient.
public class CloudMover : MonoBehaviour
{
    [SerializeField] private Vector3 direction = new Vector3(1f, 0f, 0f);
    [SerializeField] private float speed = 1f;
    [SerializeField] private float wrapDistance = 40f;

    private Vector3 startLocalPos;

    void Start()
    {
        startLocalPos = transform.localPosition;
    }

    void Update()
    {
        transform.localPosition += direction.normalized * speed * Time.deltaTime;

        // Когда облако улетело на wrapDistance вперёд — телепортируем назад
        if (Vector3.Dot(transform.localPosition - startLocalPos, direction) > wrapDistance)
            transform.localPosition -= direction.normalized * wrapDistance * 2f;
    }
}
