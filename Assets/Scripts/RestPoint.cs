using UnityEngine;

public class RestPoint : InteractionZone
{
    protected override void OnInteract()
    {
        var player = GameObject.FindWithTag("Player");
        if (player == null) return;
        var health = player.GetComponent<HealthSystem>();
        if (health == null) return;

        SceneTransition.Instance.FadeAndDo(() => health.Heal(health.MaxHealth));
    }
}
