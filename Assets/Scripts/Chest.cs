using UnityEngine;

public class Chest : InteractionZone
{
    private Animator animator;
    private bool opened;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    protected override void OnInteract()
    {
        if (opened) return;
        opened = true;

        animator?.SetTrigger("open");

        var player = GameObject.FindWithTag("Player");
        if (player == null) return;
        player.GetComponent<HealthSystem>()?.Heal(float.MaxValue);
    }
}
