using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyFootstepAudio : MonoBehaviour
{
    [SerializeField] private AudioClip[] clips;
    [SerializeField] [Range(0f, 1f)] private float volume = 0.4f;
    [SerializeField] private float stepDistance = 1.8f; // метров между шагами

    private NavMeshAgent agent;
    private Animator anim;
    private AudioSource audioSource;
    private float distanceAccum;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        anim = GetComponent<Animator>();
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.playOnAwake = false;
    }

    void Update()
    {
        bool isMoving = agent.enabled
                        && (anim == null || anim.GetFloat("speed") >= 0.1f);

        if (isMoving)
        {
            distanceAccum += agent.velocity.magnitude * Time.deltaTime;
            if (distanceAccum >= stepDistance)
            {
                PlayStep();
                distanceAccum = 0f;
            }
        }
        else
        {
            distanceAccum = 0f;
        }
    }

    private void PlayStep()
    {
        if (clips == null || clips.Length == 0) return;
        audioSource.PlayOneShot(clips[Random.Range(0, clips.Length)], volume);
    }
}
