using UnityEngine;

[CreateAssetMenu(fileName = "Weapon", menuName = "GunBite/Weapon")]
public class Weapon : Item
{
    [Tooltip("0 - semi-auto, 1 - auto, 2 - burst")]
    public int firingMode;
    [Tooltip("0 - secondary, 1 - primary, 2 - melee")]
    public int type;
    public GameObject prefab;
    public int damage;
    public int ammo;
    public int clipSize;
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
    public AudioClip slideSound;
    public AudioClip reloadSound;
    public AudioClip gunshotSound;
    public float pitchRandom;
    public float shotVolume;

    [Header("Inserting")]
    public bool insert;
    public float insertTime;

    [Header("SHOP")]
    public bool canBeSold;
    public bool canBeUpgraded;
    public int startPrice;
    public int[] upgradePrices;
    public float damageMultiplier;
    public Sprite icon;
    public int clipPrice;

    private int currentAmmo;
    private int clip;
    private int currentDamage;
    private int currentTier;
    private bool currentUpgraded;

    public void Initialize()
    {
        currentAmmo = ammo;
        clip = clipSize;
        currentDamage = damage;
        currentTier = 0;
        currentUpgraded = canBeUpgraded;
    }

    public bool FireBullet()
    {
        if (clip > 0)
        {
            clip -= 1;
            return true;
        }
        else return false;
    }

    public bool FireBurst()
    {
        if (clip >= 3)
        {
            clip -= 3;
            return true;
        }
        else return false;
    }

    public void Reload()
    {
        if(insert)
        {
            clip += 1;
            currentAmmo -= 1;
        }
        else
        {
            currentAmmo += clip;
            clip = Mathf.Min(clipSize, currentAmmo);
            currentAmmo -= clip;
        }
    }

    public bool OutOfAmmo()
    {
        if (clipSize != clip && currentAmmo != 0)
            return true;
        else
            return false;
    }

    public int GetClip() { return clip; }
    public int GetAmmo() { return currentAmmo; }
    public int GetTier() { return currentTier; }
    public int GetDamage() { return currentDamage; }
    public int GetSellPrice() 
    {
        int sellPrice;
        sellPrice = startPrice;
        for (int i = 0; i < currentTier; i++)
        {
            sellPrice += upgradePrices[i]; 
        }
        sellPrice /= 2;
        return sellPrice;
    }
    public bool CanStillBeUpgraded() { return currentUpgraded; }

    public void AddMag()
    {
        int remainFromClip = clipSize;
        clip = clipSize;
        currentAmmo += remainFromClip;
        currentAmmo = Mathf.Min(currentAmmo, ammo);
    }

    public void Refill()
    {
        currentAmmo = ammo;
        clip = clipSize;
    }

    public int GetRefillPrice()
    {
        float remainMags = (float)(ammo - currentAmmo) / (float)clipSize;
        remainMags = Mathf.Ceil(remainMags);
        return (int)remainMags * clipPrice;
    }

    public void Upgrade()
    {
        currentTier++;
        currentDamage = (int)(currentDamage * damageMultiplier);
        if (currentTier == upgradePrices.Length)
            currentUpgraded = false;
    }

    public bool FullAmmo()
    {
        if (currentAmmo == ammo) return true;
        else return false;
    }
}