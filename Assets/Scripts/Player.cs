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
    public int currentArmor;
    public bool isDead;
    [SerializeField] private AudioSource hurtSource;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    private GameManager gameManager;
    private WeaponManager wm;
    private PlayerController playerController;
    public TextMeshPro playerNickname;

    public bool invincible;
    [SerializeField] private GameObject deathEffect;
    private PlayerHUD hud;
    private LevelSystem ls;
    private MoneySystem ms;
    private Animator animator;

    private void Awake()
    {
        hud = GetComponent<PlayerHUD>();
        ls = GetComponent<LevelSystem>();
        ms = GetComponent<MoneySystem>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        playerController = GetComponent<PlayerController>();
        hud = GetComponent<PlayerHUD>();
        wm = GetComponent<WeaponManager>();
        ls = GetComponent<LevelSystem>();
        ms = GetComponent<MoneySystem>();
        currentHealth = maxHealth;
        hud.RefreshBars(currentHealth, maxHealth, currentArmor);
    }

    void Update()
    {
        if (!IsOwner) return;

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

            if (currentArmor > 0)
            {
                currentArmor -= 1;
            }
            else
            {
                currentHealth -= 1;
            }

            hud.RefreshBars(currentHealth, maxHealth, currentArmor);

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
        if (PlayerPrefs.GetInt("Extra") == 1)
            SoundManager.Instance.Play("PlayerDeathExtra");
        else
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
        hud.RefreshBars(currentHealth, maxHealth, currentArmor);
    }

    public void Control(bool status)
    {
        GetComponent<PlayerController>().enabled = status;
        GetComponent<WeaponManager>().enabled = status;
    }

    public void DisableInvincible()
    {
        invincible = false;
    }
}
