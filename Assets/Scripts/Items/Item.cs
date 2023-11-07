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

    public virtual void Initialize()
    {
        
    }

    public virtual Item GetCopy()
    {
        return this;
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
