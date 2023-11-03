using UnityEngine;
using UnityEngine.UI;

public class ItemPickup : MonoBehaviour 
{
	public Item item;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private void Start()
    {       
        spriteRenderer.sprite = item.look;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag.Contains("Player"))
        {
            switch (item.itemType)
            {
                case ItemType.Ammo:
                    Player.Instance.GetComponent<WeaponManager>().Pickup(gameObject);
                    break;
                case ItemType.Health:
                    Player.Instance.Pickup(gameObject);
                    break;
            }      
        }
    }
}
