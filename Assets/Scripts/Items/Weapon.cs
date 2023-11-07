using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "GunBite/Weapon")]
public class Weapon : Item
{
    [Tooltip("0 - semi-auto, 1 - auto, 2 - burst")]
    public int firingMode;
    [Tooltip("0 - secondary, 1 - primary")]
    public int type;
    public GameObject prefab;
    public int damage;
    public int ammo;
    public int pellets;
    public float pelletsSpread;
    public float range;
    public float fireRate;
    public float bulletForce;
    public GameObject bulletPrefab;
    public float kickback;
    public float movementSpeed;
    public float reloadTime;

    [Header("Sounds")]
    public AudioClip equipSound;
    public AudioClip reloadSound;
    public AudioClip gunshotSound;
    public float pitchRandom;
    public float shotVolume;

    [Header("Inserting")]
    public bool insert;
    public float insertTime;

    [Header("SHOP")]
    public bool canBeUpgraded;
    public int startPrice;
    public int[] upgradePrices;
    public float damageMultiplier;
    public Sprite icon;

    private int currentAmmo;
    private int currentDamage;
    private int currentTier;
    private bool currentUpgraded;

    public int GetAmmo() { return currentAmmo; }
    public int GetTier() { return currentTier; }
    public int GetDamage() { return currentDamage; }
    public bool CanStillBeUpgraded() { return currentUpgraded; }

    public override void Initialize()
    {
        currentAmmo = ammo;
        currentDamage = damage;
        currentTier = 0;
        currentUpgraded = canBeUpgraded;
    }

    public override Item GetCopy()
    {
        Item weapon = Instantiate(this);
        weapon.Initialize();
        return weapon;
    }

    public bool FireBullet()
    {
        if (currentAmmo > 0)
        {
            currentAmmo -= 1;
            return true;
        }
        else return false;
    }

    public bool FireBurst()
    {
        if (currentAmmo >= 3)
        {
            currentAmmo -= 3;
            return true;
        }
        else return false;
    }

    public void Reload()
    {
        if (insert)
        {
            currentAmmo -= 1;
        }
        else
        {
            currentAmmo = ammo;
        }
    }

    public void Refill()
    {
        currentAmmo = ammo;
    }

    public void Upgrade()
    {
        currentTier++;
        currentDamage = (int)(currentDamage * damageMultiplier);
        if (currentTier == upgradePrices.Length)
            currentUpgraded = false;
    }

    public bool OutOfAmmo()
    {
        if (currentAmmo <= 0)
            return true;
        else
            return false;
    }

    public bool FullAmmo()
    {
        if (currentAmmo == ammo)
            return true;
        else
            return false;
    }
}