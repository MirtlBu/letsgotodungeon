using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StatType { Speed, Damage, CritChance, CritMultiplier, Heal }

[System.Serializable]
public class ActiveBuff
{
    public BuffDefinition definition;
    public float timeRemaining;
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    [Header("Base Stats")]
    public float baseSpeed = 5f;
    public float baseDamage = 25f;
    public float baseCritChance = 0f;   // 0–1
    public float baseCritMultiplier = 2f;

    private readonly List<ActiveBuff> activeBuffs = new();

    public float Speed => ComputeStat(StatType.Speed, baseSpeed);
    public float Damage => ComputeStat(StatType.Damage, baseDamage);
    public float CritChance => ComputeStat(StatType.CritChance, baseCritChance);
    public float CritMultiplier => ComputeStat(StatType.CritMultiplier, baseCritMultiplier);

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].timeRemaining -= Time.deltaTime;
            if (activeBuffs[i].timeRemaining <= 0f)
                activeBuffs.RemoveAt(i);
        }
    }

    public void ApplyBuff(BuffDefinition buff)
    {
        if (buff.statType == StatType.Heal)
        {
            GetComponent<HealthSystem>()?.Heal(buff.flatBonus);
            return;
        }

        // Replace existing buff of same type, or add new
        var existing = activeBuffs.Find(b => b.definition.statType == buff.statType);
        if (existing != null)
            existing.timeRemaining = buff.duration;
        else
            activeBuffs.Add(new ActiveBuff { definition = buff, timeRemaining = buff.duration });
    }

    private float ComputeStat(StatType type, float baseValue)
    {
        float result = baseValue;
        foreach (var buff in activeBuffs)
        {
            if (buff.definition.statType != type) continue;
            result *= buff.definition.multiplier;
            result += buff.definition.flatBonus;
        }
        return result;
    }
}
