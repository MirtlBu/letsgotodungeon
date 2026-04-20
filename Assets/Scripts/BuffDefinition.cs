using UnityEngine;

[CreateAssetMenu(fileName = "Buff", menuName = "Game/Buff Definition")]
public class BuffDefinition : ScriptableObject
{
    public string buffName;
    public Sprite icon;
    public GameObject vfxPrefab; // visual effects object spawned on player while buff is active
    public StatType statType;

    [Header("Effect")]
    public float multiplier = 1f;   // e.g. 1.5 = +50% speed
    public float flatBonus = 0f;    // e.g. +30 hp for Heal

    [Header("Duration")]
    public float duration = 10f;    // seconds, ignored for Heal
}
