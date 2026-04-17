using System.Collections;
using UnityEngine;

public class NPC : InteractionZone
{
    [Header("NPC")]
    [SerializeField] private float facePlayerSpeed = 5f;
    [SerializeField] private float returnSpeed = 1f;

    private Animator animator;
    private Transform player;
    private bool isTalking;
    private Vector3 startPosition;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;
        startPosition = transform.position;
    }

    void Update()
    {
        base.Update();
        if (isTalking && player != null)
            FacePlayer();
    }

    private void FacePlayer()
    {
        Vector3 dir = player.position - transform.position;
        dir.y = 0f;
        if (dir == Vector3.zero) return;
        Quaternion target = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, target, facePlayerSpeed * Time.deltaTime);
    }

    protected override void OnDialogueEnd()
    {
        isTalking = false;
        animator?.SetBool("talking", false);
        StartCoroutine(ReturnToStart());
    }

    protected override void OnDialogueCancelled()
    {
        isTalking = false;
        animator?.SetBool("talking", false);
        StartCoroutine(ReturnToStart());
    }

    private IEnumerator ReturnToStart()
    {
        animator?.SetTrigger("returning");

        while (Vector3.Distance(transform.position, startPosition) > 0.05f)
        {
            Vector3 dir = (startPosition - transform.position).normalized;
            dir.y = 0f;

            if (dir != Vector3.zero)
            {
                Quaternion target = Quaternion.LookRotation(dir);
                transform.rotation = Quaternion.Slerp(transform.rotation, target, facePlayerSpeed * Time.deltaTime);
            }

            transform.position = Vector3.MoveTowards(transform.position, startPosition, returnSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = startPosition;
    }

    protected override void OnDialogueStart()
    {
        isTalking = true;
        animator?.SetBool("talking", true);
    }
}
