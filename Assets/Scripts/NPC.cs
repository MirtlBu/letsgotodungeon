using UnityEngine;

public class NPC : InteractionZone
{
    [Header("NPC")]
    [SerializeField] private string npcName = "NPC";
    [SerializeField] private float facePlayerSpeed = 5f;

    private Animator animator;
    private Transform player;
    private bool isTalking;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindWithTag("Player")?.transform;
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
    }

    protected override void OnDialogueStart()
    {
        isTalking = true;
        animator?.SetBool("talking", true);
    }
}
