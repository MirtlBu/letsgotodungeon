using UnityEngine;

// Вешается на объект с InteractionZone/диалогом.
// После каждого диалога переключает на следующий трэк из массива (по кругу).
public class MusicChangerInteractable : InteractionZone
{
    [Header("Music")]
    [SerializeField] private AudioClip[] tracks;

    private int currentIndex = 0;

    protected override void OnInteract() => SwitchTrack();
    protected override void OnDialogueEnd() => SwitchTrack();

    private void SwitchTrack()
    {
        if (tracks == null || tracks.Length == 0) return;
        AudioManager.Instance?.SwitchMusic(tracks[currentIndex]);
        currentIndex = (currentIndex + 1) % tracks.Length;
    }
}
