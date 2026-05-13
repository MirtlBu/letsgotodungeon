using UnityEngine;

public class CritStarsEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem starsParticle;

    void Start()
    {
        if (starsParticle == null)
            starsParticle = GetComponentInChildren<ParticleSystem>();

        var health = GetComponentInParent<HealthSystem>();
        if (health == null)
            Debug.LogWarning($"[CritStarsEffect] HealthSystem not found on {gameObject.name} or its parents");
        else
            health.OnCritDamaged.AddListener(PlayStars);
    }

    private void PlayStars()
    {
        if (starsParticle == null) return;
        starsParticle.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        starsParticle.Play();
    }
}
