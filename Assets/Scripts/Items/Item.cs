using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "GunBite/Item")]
public class Item : ScriptableObject
{
    public string itemName = "Item";
    public ItemType itemType = ItemType.Weapon;
    public GameObject pickUp;
    public Sprite look;

    public virtual void Use()
    {
        Debug.Log("Used " + itemName);
    }

    public void NewAsset(Item basic)
    {
        itemName = basic.itemName;
        name = itemName;
        itemType = basic.itemType;
        pickUp = basic.pickUp;
    }
}

public enum ItemType
{
    Weapon,
    Ammo,
    Health
}

[System.Serializable]
public class LootItem
{
    public Item item;
    [Range(0f, 1f)] public float chanceToDrop;
}
