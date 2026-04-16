using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance { get; private set; }

    public bool HasSword { get; private set; }

    [Header("Debug")]
    [SerializeField] private bool debugResetSword = false;

    void Awake()
    {
        Instance = this;
        if (debugResetSword)
        {
            PlayerPrefs.DeleteKey("HasSword");
            PlayerPrefs.Save();
        }
        HasSword = PlayerPrefs.GetInt("HasSword", 0) == 1;
    }

    public void GiveSword()
    {
        if (HasSword) return;
        HasSword = true;
        PlayerPrefs.SetInt("HasSword", 1);
        PlayerPrefs.Save();
        Debug.Log("[Inventory] Sword obtained!");
    }

    // Multipiler applied to base stat from equipped items
    public float GetEquipmentMultiplier(StatType type)
    {
        float m = 1f;
        if (HasSword && type == StatType.Damage)
            m *= 1.05f;
        return m;
    }
}
