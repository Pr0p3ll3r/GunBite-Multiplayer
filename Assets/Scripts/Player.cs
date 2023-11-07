using UnityEngine;
using System.Collections;
using FishNet.Object;
using TMPro;
using FishNet.Object.Synchronizing;

public class Player : NetworkBehaviour, IDamageable
{
    public static Player Instance { get; private set; }

    [SyncVar]
    public int maxHealth = 4;
    public int currentHealth;

    private bool isDead;
    public bool IsDead => isDead;
    public bool autoAim;

    [SerializeField] private AudioSource hurtSource;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    private WeaponManager weaponManager;
    private PlayerController playerController;
    public TextMeshPro playerNickname;

    public bool invincible;
    [SerializeField] private GameObject deathEffect;
    private PlayerHUD hud;
    private LevelSystem ls;
    private MoneySystem ms;
    private Animator animator;
    private Pause pause;

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        hud = GetComponent<PlayerHUD>();
        weaponManager = GetComponent<WeaponManager>();
        ls = GetComponent<LevelSystem>();
        ms = GetComponent<MoneySystem>();
        animator = GetComponent<Animator>();
        pause = GameObject.Find("HUD/PauseMenu").GetComponent<Pause>();
        currentHealth = maxHealth;
        hud.RefreshBars(currentHealth, maxHealth);
        autoAim = PlayerPrefs.GetInt("AutoAim", 1) == 1;
    }

    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Escape))
            pause.TooglePause();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.T))
        {
            TakeDamage(1);
        }
#endif
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        if (!IsOwner) return;

        Instance = this;
        Camera.main.GetComponent<CameraFollow>().SetPlayer(transform);
    }

    public void TakeDamage(int damage)
    {
        if (isDead || invincible) return;
        hurtSource.PlayOneShot(hurtSound);
        if (IsOwner)
        {
            invincible = true;
            SoundManager.Instance.Play("PlayerHurt");
            animator.SetTrigger("GetHit");
            hud.ShowVignette();

            currentHealth -= 1;

            hud.RefreshBars(currentHealth, maxHealth);

            if (currentHealth <= 0)
            {
                StartCoroutine(Destroy());
                Die();
            }
        }
    }

    [ObserversRpc]
    void Die()
    {
        SoundManager.Instance.Play("PlayerDeath");
        isDead = true;
        Instantiate(deathEffect, transform.position, Quaternion.identity);
        hud.ShowDeadText();
        hurtSource.PlayOneShot(deathSound);
        Destroy(gameObject);
    }

    IEnumerator Destroy()
    {
        yield return new WaitForSeconds(5f);
        Despawn();
    }

    public void Reward(int exp, int money)
    {
        ls.GetExp(exp);
        ms.GetMoney(money);
    }

    public void RefillHealth()
    {
        currentHealth = maxHealth;
        hud.RefreshBars(currentHealth, maxHealth);
    }

    public void Control(bool status)
    {
        playerController.enabled = status;
        weaponManager.enabled = status;
    }

    public void DisableInvincible()
    {
        invincible = false;
    }
}
