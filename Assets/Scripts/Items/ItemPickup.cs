using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : MonoBehaviour 
{
	public Item item;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer.sprite = item.look;
        switch (item.itemType)
        {
			case ItemType.Weapon:
				Weapon newWeapon = ScriptableObject.CreateInstance<Weapon>();
				newWeapon.NewAsset((Weapon)item);
				item = newWeapon;
				break;
		}
    }

	public void PickUp()
	{
        Player.Instance.GetComponent<WeaponManager>().Pickup(gameObject);
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag.Contains("Player"))
        {
            PickUp();
        }
    }
}
