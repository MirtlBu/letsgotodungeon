using System.Collections.Generic;
using UnityEngine;

public enum StatType { Speed, Damage, CritChance, CritMultiplier, Heal }

[System.Serializable]
public class ActiveBuff
{
    public BuffDefinition definition;
    public float timeRemaining;
    public GameObject vfxInstance; // spawned vfx object, destroyed when buff expires
}

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance { get; private set; }

    private Stats stats;
    private readonly List<ActiveBuff> activeBuffs = new();
    public IReadOnlyList<ActiveBuff> ActiveBuffs => activeBuffs;

    public float Speed          => ComputeStat(StatType.Speed,          stats.speed);
    public float Damage         => ComputeStat(StatType.Damage,         stats.damage);
    public float CritChance     => ComputeStat(StatType.CritChance,     stats.critChance);
    public float CritMultiplier => ComputeStat(StatType.CritMultiplier, stats.critMultiplier);

    void Awake()
    {
        Instance = this;
        stats = GetComponent<Stats>();
    }

    void Update()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            activeBuffs[i].timeRemaining -= Time.deltaTime;
            if (activeBuffs[i].timeRemaining <= 0f)
            {
                if (activeBuffs[i].vfxInstance != null)
                    Destroy(activeBuffs[i].vfxInstance);
                activeBuffs.RemoveAt(i);
            }
            else if (activeBuffs[i].vfxInstance != null)
            {
                activeBuffs[i].vfxInstance.transform.position = transform.position;
            }
        }
    }

    public void ApplyBuff(BuffDefinition buff)
    {
        if (buff.statType == StatType.Heal)
        {
            GetComponent<HealthSystem>()?.Heal(buff.flatBonus);
            return;
        }

        var existing = activeBuffs.Find(b => b.definition.statType == buff.statType);
        if (existing != null)
        {
            existing.timeRemaining = buff.duration;
        }
        else
        {
            GameObject vfxInst = null;
            if (buff.vfxPrefab != null)
                vfxInst = Instantiate(buff.vfxPrefab, transform.position, Quaternion.identity);
            activeBuffs.Add(new ActiveBuff { definition = buff, timeRemaining = buff.duration, vfxInstance = vfxInst });
        }
    }

    private float ComputeStat(StatType type, float baseValue)
    {
        float result = baseValue;

        if (PlayerInventory.Instance != null)
            result *= PlayerInventory.Instance.GetEquipmentMultiplier(type);

        foreach (var buff in activeBuffs)
        {
            if (buff.definition.statType != type) continue;
            result *= buff.definition.multiplier;
            result += buff.definition.flatBonus;
        }
        return result;
    }
}
