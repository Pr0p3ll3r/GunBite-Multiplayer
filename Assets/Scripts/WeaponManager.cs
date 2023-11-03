using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

public class WeaponManager : MonoBehaviour
{
    public Weapon[] loadout;
    private GameObject currentWeapon;
    [SerializeField] private Transform weaponHolder;
    public int selectedWeapon = 0;
    private Weapon currentWeaponData;

    [SerializeField] private AudioSource sfx;
    [SerializeField] private AudioSource weaponSound;
    public bool isReloading = false;
    private bool isEquipping = false;
    [SerializeField] private GameObject reloading;

    private float currentCooldown;
    private PlayerHUD hud;
    private Vector3 weaponPosition;

    private Coroutine muzzle;
    private Coroutine equip;
    private Coroutine reloadHud;
    private Coroutine reload;
    private bool burst;

    private void Start()
    {
        hud = GetComponent<PlayerHUD>();
        foreach (Weapon weapon in loadout)
        {
            if(weapon != null)
                weapon.Initialize();
        }
        reloading.SetActive(false);
        equip = StartCoroutine(Equip(1));
        hud.RefreshWeapon(loadout);
    }

    void Update()
    {
        if (Pause.paused || GetComponent<Player>().isDead) return;

        if (Input.GetKeyDown(KeyCode.Alpha1) && loadout[0] != null && selectedWeapon != 0)
            equip = StartCoroutine(Equip(0));

        if (Input.GetKeyDown(KeyCode.Alpha2) && loadout[1] != null && selectedWeapon != 1)
            equip = StartCoroutine(Equip(1));

        if (Input.GetKeyDown(KeyCode.Alpha3) && loadout[2] != null && selectedWeapon != 2 )
            equip = StartCoroutine(Equip(2));

        if (currentWeapon != null)
        {
            if (currentCooldown > 0) currentCooldown -= Time.deltaTime;

            if (currentWeapon.transform.localPosition != weaponPosition)
                currentWeapon.transform.localPosition = Vector3.Lerp(currentWeapon.transform.localPosition, weaponPosition, Time.deltaTime * 4f);

            if (!isEquipping)
            {
                if (loadout[selectedWeapon].type == 2)
                {
                    if (Input.GetMouseButtonDown(0) && currentCooldown <= 0)
                    {
                        Attack();
                    }
                }
                else
                {
                    if (loadout[selectedWeapon].firingMode == 0)
                    {
                        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0 && isReloading == false)
                        {
                            if (loadout[selectedWeapon].FireBullet()) Shoot();
                            else if (currentWeaponData.OutOfAmmo()) reload = StartCoroutine(Reload());
                        }
                    }
                    else if(loadout[selectedWeapon].firingMode == 2)
                    {
                        if (Input.GetMouseButtonDown(0) && currentCooldown <= 0 && isReloading == false)
                        {
                            if (loadout[selectedWeapon].FireBurst()) StartCoroutine(Burst());
                            else if (currentWeaponData.OutOfAmmo()) reload = StartCoroutine(Reload());
                        }
                    }
                    else
                    {
                        if (Input.GetMouseButton(0) && currentCooldown <= 0 && isReloading == false)
                        {
                            if (loadout[selectedWeapon].FireBullet()) Shoot();
                            else if (currentWeaponData.OutOfAmmo()) reload = StartCoroutine(Reload());
                        }
                    }

                    if (Input.GetKeyDown(KeyCode.R)) if (currentWeaponData.OutOfAmmo() && isReloading == false) reload = StartCoroutine(Reload());

                    if (Input.GetKeyDown(KeyCode.E)) StartCoroutine(QuickAttack());
                }              
            }
        }
    }

    IEnumerator Equip(int index)
    {
        if (burst) yield break;

        isEquipping = true;

        selectedWeapon = index;
        currentWeaponData = loadout[selectedWeapon];

        SoundManager.Instance.Play("Equip");

        if (currentWeapon != null)
        {
            if (isReloading)
            {
                StopCoroutine(reload);
                StopCoroutine(reloadHud);
                reloading.SetActive(false);
            }
            if (muzzle != null) StopCoroutine(muzzle);
            if (equip != null) StopCoroutine(equip);
            weaponSound.Stop();
            isReloading = false;
            Destroy(currentWeapon);
        }

        GameObject newWeapon = Instantiate(loadout[index].prefab, weaponHolder) as GameObject;
        currentWeapon = newWeapon;
        newWeapon.transform.localPosition = loadout[index].prefab.transform.localPosition;
        weaponPosition = newWeapon.transform.localPosition;

        if (currentWeaponData.equipSound != null)
        {
            weaponSound.clip = currentWeaponData.equipSound;
            weaponSound.Play();
        }

        hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());
        if (currentWeapon.GetComponent<Animator>() != null)
            if (currentWeapon.GetComponent<Animator>().HasState(0, Animator.StringToHash("Equip"))) 
                currentWeapon.GetComponent<Animator>().Play("Equip", 0, 0);

        hud.SelectWeapon(selectedWeapon);

        yield return new WaitForSeconds(1f);
        isEquipping = false;
    }

    void Shoot()
    {
        hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());

        //sfx
        sfx.clip = currentWeaponData.gunshotSound;
        sfx.pitch = 1 - currentWeaponData.pitchRandom + Random.Range(-currentWeaponData.pitchRandom, currentWeaponData.pitchRandom);
        sfx.volume = currentWeaponData.shotVolume;
        sfx.PlayOneShot(sfx.clip);

        //slide sound
        if (currentWeaponData.slideSound != null)
        {
            sfx.clip = currentWeaponData.slideSound;
            sfx.PlayOneShot(sfx.clip);
        }

        //muzzle
        SpriteRenderer muzzleFlash = currentWeapon.transform.Find("MuzzleFlash").GetComponent<SpriteRenderer>();
        muzzle = StartCoroutine(MuzzleFlash(muzzleFlash));

        //firepoint
        Transform firePoint = currentWeapon.transform.Find("FirePoint").transform;

        //animation
        if(currentWeapon.GetComponent<Animator>() != null)
            if (currentWeapon.GetComponent<Animator>().HasState(0, Animator.StringToHash("Shoot")))
                currentWeapon.GetComponent<Animator>().Play("Shoot", 0, 0);

        for (int i = 0; i < Mathf.Max(1, currentWeaponData.pellets); i++)
        {
            GameObject bullet = GameManager.Instance.bulletPooler.Get();
            if (currentWeaponData.itemName.Contains("Grenade"))
                bullet.GetComponent<Bullet>().SetDamage(currentWeaponData.GetDamage(), true);
            else
                bullet.GetComponent<Bullet>().SetDamage(currentWeaponData.GetDamage(), false);
            bullet.transform.position = firePoint.position;
            bullet.transform.rotation = firePoint.rotation;
            Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
            rb.velocity = Vector2.zero;
            bullet.SetActive(true);

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

        //gun fx
        currentWeapon.transform.position -= currentWeapon.transform.right * currentWeaponData.kickback;

        //cooldown
        currentCooldown = currentWeaponData.fireRate;
    }

    void Attack()
    {
        //sfx
        sfx.clip = currentWeaponData.gunshotSound;
        sfx.volume = currentWeaponData.shotVolume;
        sfx.PlayOneShot(sfx.clip);

        //animation
        currentWeapon.GetComponent<Animator>().Play("Attack", 0, 0);

        //attackpoint
        Transform attackPoint = currentWeapon.transform.Find("AttackPoint").transform;

        Collider2D[] enemies = Physics2D.OverlapCircleAll(attackPoint.position, currentWeaponData.range);
        foreach(Collider2D enemy in enemies)
        {
            if (enemy.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox"))
            {
                enemy.gameObject.transform.root.GetComponent<IDamageable>()?.TakeDamage(currentWeaponData.damage);
            }
        }

        //cooldown
        currentCooldown = currentWeaponData.fireRate;
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
                hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());
            }
            while (currentWeaponData.GetClip() != currentWeaponData.clipSize);
        }
        else
        {

            reloadHud = StartCoroutine(ReloadingCircle(currentWeaponData.reloadTime));
            weaponSound.clip = currentWeaponData.reloadSound;
            weaponSound.Play();
            yield return new WaitForSeconds(currentWeaponData.reloadTime);
            currentWeaponData.Reload();
            hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());
            yield return new WaitForSeconds(0.2f);
        }

        isReloading = false;
    }

    IEnumerator MuzzleFlash(SpriteRenderer muzzle)
    {
        muzzle.enabled = true;

        for(int i = 0; i < 1; i++)
        {
            yield return new WaitForSeconds(0.1f);
        }

        muzzle.enabled = false;
    }

    IEnumerator QuickAttack()
    {
        int prevWeapon = selectedWeapon;
        selectedWeapon = 2;
        currentWeaponData = loadout[selectedWeapon];

        if (currentWeapon != null)
        {
            if (isReloading) StopCoroutine(reload);
            if (muzzle != null) StopCoroutine(muzzle);
            if (equip != null) StopCoroutine(equip);
            isReloading = false;
            Destroy(currentWeapon);
        }

        GameObject newWeapon = Instantiate(loadout[2].prefab, weaponHolder) as GameObject;
        currentWeapon = newWeapon;
        newWeapon.transform.localPosition = loadout[2].prefab.transform.localPosition;
        weaponPosition = newWeapon.transform.localPosition;

        hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());

        hud.SelectWeapon(selectedWeapon);

        isEquipping = false;

        Attack();

        yield return new WaitForSeconds(currentCooldown);

        equip = StartCoroutine(Equip(prevWeapon));
    }

    IEnumerator Burst()
    {
        burst = true;

        for (int i = 0; i < 3; i++)
        {
            Shoot();

            yield return new WaitForSeconds(0.11f);
        }

        burst = false;
    }

    IEnumerator ReloadingCircle(float time)
    {
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
                        currentWeaponData.AddMag();
                        hud.RefreshAmmo(currentWeaponData.GetClip(), currentWeaponData.GetAmmo());
                    }                   
                }                             
                break;
        }

        if (pickedUp)
        {
            Debug.Log("Picked up " + newItem.itemName);
            SoundManager.Instance.PlayOneShot("Pickup");
            Destroy(sceneObject);
        }
    }
}