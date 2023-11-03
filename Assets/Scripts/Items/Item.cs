using UnityEngine;

[CreateAssetMenu(fileName = "Item", menuName = "GunBite/Item")]
public class Item : ScriptableObject 
{
	public string itemName = "Item";
	public ItemType itemType = ItemType.Weapon;
	public GameObject pickUp;
	public Sprite look;
}

public enum ItemType
{
	Weapon,
	Health,
	Ammo
}

[System.Serializable]
public class LootItem
{
	public Item item;
	[Range(1, 100)] public int dropChance;
}
