using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Globalization;
using FishNet.Object;

public class WeaponManager : NetworkBehaviour
{
    [SerializeField] private Weapon[] loadout;
    [SerializeField] private ObjectPooler bulletPooler;
    [SerializeField] private GameObject currentWeapon;
    [SerializeField] private Transform weaponHolder;
    [SerializeField] private int selectedWeapon = 0;
    [SerializeField] private Weapon currentWeaponData;
    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioSource weaponSound;
    public bool isReloading = false;
    public bool isEquipping = false;
    [SerializeField] private GameObject bulletPrefab;

    private float currentCooldown;
    private PlayerHUD hud;
    private Player player;
    private Vector3 weaponPosition;
    private PlayerController controller;
    private Coroutine muzzle;
    private Coroutine equip;
    private Coroutine reloadHud;
    private Coroutine reload;
    private bool burst;
    private GameObject closestEnemy;
    public GameObject ClosestEnemy => closestEnemy;

    private void Start()
    {
        player = GetComponent<Player>();
        hud = GetComponent<PlayerHUD>();
        controller = GetComponent<PlayerController>();
        equip = StartCoroutine(Equip(0));
    }

    void Update()
    {
        if (!IsOwner) return;

        closestEnemy = GetClosestEnemy();

        if (Pause.paused || GetComponent<Player>().IsDead) return;

        if (currentWeapon != null)
        {
            if (currentCooldown > 0) currentCooldown -= Time.deltaTime;

            if (currentWeapon.transform.localPosition != weaponPosition)
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, weaponPosition, Time.deltaTime * 4f);

            if (!isEquipping && currentCooldown <= 0 && !isReloading)
            {
                if (currentWeaponData.OutOfAmmo())
                    reload = StartCoroutine(Reload());
                else if (player.autoAim)
                {
                    if (closestEnemy)
                    {
                        Shoot();
                    }
                }
                else
                {

                    if (currentWeaponData.firingMode == 1)
                    {
                        if (Input.GetMouseButton(0))
                        {
                            Shoot();
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButtonDown(0))
                        {
                            Shoot();
                        }
                    }
                }

                if (Input.GetKeyDown(KeyCode.R))
                    if (!currentWeaponData.FullAmmo() && !isReloading) reload = StartCoroutine(Reload());
            }
        }
    }

    IEnumerator Equip(int index)
    {
        if (burst) yield break;

        isEquipping = true;

        selectedWeapon = index;
        currentWeaponData = (Weapon)loadout[selectedWeapon].GetCopy();

        SoundManager.Instance.Play("Equip");

        if (currentWeapon != null)
        {
            if (isReloading)
            {
                StopCoroutine(reload);
                StopCoroutine(reloadHud);
                hud.reloading.SetActive(false);
            }
            if (muzzle != null) StopCoroutine(muzzle);
            if (equip != null) StopCoroutine(equip);
            weaponSound.Stop();
            isReloading = false;
            Destroy(currentWeapon);
        }

        GameObject newWeapon = Instantiate(loadout[index].prefab, weaponHolder);
        currentWeapon = newWeapon;
        newWeapon.transform.localPosition = loadout[index].prefab.transform.localPosition;
        weaponPosition = newWeapon.transform.localPosition;

        if (currentWeaponData.equipSound != null)
        {
            weaponSound.clip = currentWeaponData.equipSound;
            weaponSound.Play();
        }

        hud.RefreshAmmo(currentWeaponData.GetAmmo());

        yield return new WaitForSeconds(1f);
        isEquipping = false;
    }

    private void Shoot()
    {
        if (currentWeaponData.FireBullet())
        {
            ServerShoot();
            currentCooldown = currentWeaponData.fireRate;
            hud.RefreshAmmo(currentWeaponData.GetAmmo());
        }
    }

    [ServerRpc]
    void ServerShoot()
    {
        //firepoint
        Transform firePoint = currentWeapon.transform.Find("FirePoint").transform;

        for (int i = 0; i < Mathf.Max(1, currentWeaponData.pellets); i++)
        {
            GameObject bullet = Instantiate(bulletPrefab);
            if (currentWeaponData.itemName.Contains("Grenade"))
                bullet.GetComponent<Bullet>().SetDamage(currentWeaponData.GetDamage(), true);
            else
                bullet.GetComponent<Bullet>().SetDamage(currentWeaponData.GetDamage(), false);
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            bullet.SetActive(true);
            Spawn(bullet);

            if (currentWeaponData.pellets == 0)
            {
                rb.AddForce(firePoint.right * currentWeaponData.bulletForce, ForceMode2D.Impulse);
            }
            else
            {
                float maxSpread = currentWeaponData.pelletsSpread;

                Vector3 direction = firePoint.right + new Vector3(Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread), Random.Range(-maxSpread, maxSpread));
                rb.AddForce(direction * currentWeaponData.bulletForce, ForceMode2D.Impulse);
            }
        }

        RpcShoot();
    }

    [ObserversRpc]
    void RpcShoot()
    {
        //animation
        if (currentWeapon.GetComponent<Animator>() != null)
            if (currentWeapon.GetComponent<Animator>().HasState(0, Animator.StringToHash("Shoot")))
                currentWeapon.GetComponent<Animator>().Play("Shoot", 0, 0);

        //sfx
        sfx.clip = currentWeaponData.gunshotSound;
        sfx.pitch = 1 - currentWeaponData.pitchRandom + Random.Range(-currentWeaponData.pitchRandom, currentWeaponData.pitchRandom);
        sfx.volume = currentWeaponData.shotVolume;
        sfx.PlayOneShot(sfx.clip);

        //muzzle
        SpriteRenderer muzzleFlash = currentWeapon.transform.Find("MuzzleFlash").GetComponent<SpriteRenderer>();
        muzzle = StartCoroutine(MuzzleFlash(muzzleFlash));

        //gun fx
        currentWeapon.transform.position -= currentWeapon.transform.right * currentWeaponData.kickback;
    }

    private GameObject GetClosestEnemy()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, currentWeaponData.range, LayerMask.GetMask("Enemy"));
        GameObject closestEnemy = null;
        float minimumDistance = 1000000f;

        foreach (Collider2D enemy in enemies)
        {
            float distanceToEnemy = Vector3.Distance(transform.position, enemy.transform.position);

            if (distanceToEnemy < minimumDistance)
            {
                closestEnemy = enemy.gameObject;
                minimumDistance = distanceToEnemy;
            }
        }
        
        return closestEnemy;
    }

    IEnumerator Reload()
    {
        weaponSound.Stop();

        isReloading = true;

        if (currentWeapon.GetComponent<Animator>() != null)
        {
            if (currentWeapon.GetComponent<Animator>().HasState(0, Animator.StringToHash("Reload")))
                currentWeapon.GetComponent<Animator>().Play("Reload", 0, 0);
        }

        if (reloadHud != null) StopCoroutine(reloadHud);

        if (currentWeaponData.insert)
        {
            do
            {
                if (reloadHud != null) StopCoroutine(reloadHud);
                if (!currentWeaponData.OutOfAmmo())
                {
                    isReloading = false;
                    StopCoroutine(reload);
                }
                reloadHud = StartCoroutine(ReloadingCircle(currentWeaponData.insertTime));
                weaponSound.PlayOneShot(currentWeaponData.reloadSound);
                yield return new WaitForSeconds(currentWeaponData.insertTime);
                currentWeaponData.Reload();
                hud.RefreshAmmo(currentWeaponData.GetAmmo());
            }
            while (currentWeaponData.GetAmmo() != currentWeaponData.ammo);
        }
        else
        {

            reloadHud = StartCoroutine(ReloadingCircle(currentWeaponData.reloadTime));
            weaponSound.clip = currentWeaponData.reloadSound;
            weaponSound.Play();
            yield return new WaitForSeconds(currentWeaponData.reloadTime);
            currentWeaponData.Reload();
            hud.RefreshAmmo(currentWeaponData.GetAmmo()); 
            yield return new WaitForSeconds(0.2f);
        }

        isReloading = false;
    }

    IEnumerator MuzzleFlash(SpriteRenderer muzzle)
    {
        muzzle.enabled = true;

        for (int i = 0; i < 1; i++)
        {
            yield return new WaitForSeconds(0.1f);
        }

        muzzle.enabled = false;
    }

    IEnumerator Burst()
    {
        burst = true;

        for (int i = 0; i < 3; i++)
        {
            ServerShoot();

            yield return new WaitForSeconds(0.11f);
        }

        burst = false;
    }

    IEnumerator ReloadingCircle(float time)
    {
        GameObject reloading = hud.reloading;
        float reloadTime = time;
        Image reloadingCircle = reloading.GetComponentInChildren<Image>();
        TextMeshProUGUI reloadTimeText = reloading.GetComponentInChildren<TextMeshProUGUI>();

        reloading.SetActive(true);

        while (time > 0)
        {
            float percent = time / reloadTime;
            reloadingCircle.fillAmount = percent;

            reloadTimeText.text = time.ToString("0.0", CultureInfo.InvariantCulture);

            time -= 0.1f;
            yield return new WaitForSeconds(0.1f);
        }

        reloading.SetActive(false);
    }

    public void Pickup(GameObject sceneObject)
    {
        bool pickedUp = false;
        Item newItem = sceneObject.GetComponent<ItemPickup>().item;
        switch (newItem.itemType)
        {
            case ItemType.Ammo:
                if (currentWeaponData.type != 2)
                {
                    if (!currentWeaponData.FullAmmo())
                    {
                        pickedUp = true;
                        hud.RefreshAmmo(currentWeaponData.GetAmmo());
                    }
                }
                break;
        }

        if (pickedUp)
        {
            Debug.Log("Picked up " + newItem.itemName);
            Destroy(sceneObject);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, currentWeaponData.range);
    }
}