using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Knockback")]
    [SerializeField] private float knockbackForce = 4f;
    [SerializeField] private float knockbackDuration = 0.15f;
    [SerializeField] private float knockbackDelay = 0.05f;

    [Header("Long Knockback")]
    [SerializeField] private float longKnockbackForce = 8f;
    [SerializeField] private float longKnockbackDuration = 0.3f;

    public float LongKnockbackForce => longKnockbackForce;
    public float LongKnockbackDuration => longKnockbackDuration;

    private Animator animator;
    private CharacterMovement movement;
    private CharacterController controller;

    // Запоминаем последнего атакующего для knockback при смерти
    public Vector3 LastAttackerPosition { get; private set; }

    void Start()
    {
        animator = GetComponent<Animator>();
        movement = GetComponent<CharacterMovement>();
        controller = GetComponent<CharacterController>();
    }

    public void RecordAttacker(Transform source)
    {
        LastAttackerPosition = source.position;
    }

    public void ApplyKnockback(Transform source)
    {
        LastAttackerPosition = source.position;
        StartCoroutine(KnockbackRoutine(source.position, knockbackForce, knockbackDuration));
    }

    public void ApplyLongKnockback(Transform source)
    {
        LastAttackerPosition = source.position;
        StartCoroutine(KnockbackRoutine(source.position, longKnockbackForce, longKnockbackDuration));
    }

    private IEnumerator KnockbackRoutine(Vector3 sourcePosition, float force, float duration)
    {
        animator?.SetTrigger("impact");

        Vector3 dir = transform.position - sourcePosition;
        dir.y = 0f;
        dir.Normalize();

        yield return new WaitForSeconds(knockbackDelay);

        movement.enabled = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = 1f - elapsed / duration;
            controller.Move(dir * (force * t * Time.deltaTime));
            elapsed += Time.deltaTime;
            yield return null;
        }

        movement.enabled = true;
    }
}
