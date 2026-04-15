using UnityEngine;

[CreateAssetMenu(fileName = "ShopItem", menuName = "Game/Shop Item")]
public class ShopItemSO : ScriptableObject
{
    public string itemName;
    public int price;
    public BuffDefinition buff;
}
