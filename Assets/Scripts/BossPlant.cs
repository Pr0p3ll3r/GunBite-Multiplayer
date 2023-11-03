using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossPlant : ZombieInfo, IDamageable
{
    private Transform player;
    private Animator animator;
    private GameObject healthBar;
    private TextMeshProUGUI moneyReward;
    private GameObject hitbox;

    [SerializeField] private ParticleSystem deathEffect;

    [SerializeField] private Transform spitPoint;
    [SerializeField] private float radius;

    private int currentHealth;
    private float lastAttackTime = 0;
    private bool isDead;
    private bool appear;

    private ObjectPooler acidPooler;
   
    void Start()
    {
        acidPooler = GameManager.Instance.plantAcidPooler;
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        healthBar = transform.GetChild(0).transform.Find("HealthBar").gameObject;
        moneyReward = transform.GetChild(0).transform.Find("Money").GetComponent<TextMeshProUGUI>();
        hitbox = transform.GetChild(1).gameObject;
    }

    private void Update()
    {
        if (!appear || isDead || player == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
        {
            Attack();
        }
        Spit();
    }

    void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetTrigger("Attack");
            lastAttackTime = Time.time;
        }
    }

    public void GiveDamage()
    {
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D col in objectsInRange)
        {
            if (col.gameObject.tag.Equals("Player"))
            {
                col.gameObject.transform.root.GetComponent<Player>().TakeDamage(1);
            }
        }
    }

    public void Appear()
    {
        appear = true;
    }

    void Spit()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;

            Vector2 direction = (player.position - transform.position).normalized;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            GameObject acid = acidPooler.Get();
            acid.transform.position = spitPoint.position;
            acid.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
            acid.SetActive(true);
        }
    }

    public void TakeDamage(int damage)
    {
        if (appear && isDead == false)
        {
            currentHealth -= damage;
            SoundManager.Instance.Play("ZombieHurt");
            SetHealthBar();
            if (currentHealth <= 0)
            {
                Die();
            }
        }
    }

    public void Die()
    {
        isDead = true;
        player.gameObject.GetComponent<Player>().Reward(exp, money);
        deathEffect.Play();
        moneyReward.text = $"+{money}$";
        moneyReward.GetComponent<Animator>().Play("FadeOut");
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        hitbox.GetComponent<Collider2D>().enabled = false;
        transform.GetChild(2).gameObject.SetActive(false);
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ZombieKilled();
            GameManager.Instance.waveManager.ZombieKilled(-1);
        }
        StartCoroutine(Destroy(deathEffect.main.duration));
    }

    IEnumerator Destroy(float time)
    {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }

    void SetHealthBar()
    {
        healthBar.SetActive(true);
        healthBar.GetComponentInChildren<SlicedFilledImage>().fillAmount = (float)currentHealth / maxHealth;
        if (healthBar.GetComponentInChildren<SlicedFilledImage>().fillAmount <= 0)
            healthBar.gameObject.SetActive(false);
    }
}
