using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public bool HasSword { get; private set; }

    void Awake()
    {
        Instance = this;
        HasSword = PlayerPrefs.GetInt("HasSword", 0) == 1;
    }

    public void Reset()
    {
        HasSword = false;
    }

    public void GiveSword()
    {
        if (HasSword) return;
        HasSword = true;
        PlayerPrefs.SetInt("HasSword", 1);
        PlayerPrefs.Save();

        // Destroy world sword prop in dungeon (if in same scene)
        var worldSword = GameObject.Find("game_sword");
        if (worldSword != null) Destroy(worldSword);

        // Show sword in player's hand immediately
        GameObject.FindWithTag("Player")?.GetComponent<WeaponVisibility>()?.SetAlwaysVisible();

        Debug.Log("[Inventory] Sword obtained!");
    }

#if UNITY_EDITOR
    [ContextMenu("Reset sword state")]
    private void ResetSwordState()
    {
        HasSword = false;
        PlayerPrefs.DeleteKey("HasSword");
        PlayerPrefs.DeleteKey("sword_world_collected");
        PlayerPrefs.Save();
        Debug.Log("[Inventory] Sword state reset");
    }
#endif

    // Multipiler applied to base stat from equipped items
    public float GetEquipmentMultiplier(StatType type)
    {
        float m = 1f;
        if (HasSword && type == StatType.Damage)
            m *= 1.05f;
        return m;
    }
}
