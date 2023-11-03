using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BossHead : ZombieInfo, IDamageable
{
    [SerializeField] private ParticleSystem deathEffect;

    private Transform player;    
    private Animator animator;
    private GameObject healthBar;
    private TextMeshProUGUI moneyReward;
    private GameObject hitbox;
    private Rigidbody2D rb;

    private int currentHealth;
    private float lastAttackTime = 0;
    private bool isDead;
    private bool attack;

    void Start()
    {
        if (size == 3f) currentHealth = maxHealth;
        else
        {
            transform.localScale = new Vector3(size, size, size);
        }
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        healthBar = transform.GetChild(0).transform.Find("HealthBar").gameObject;
        moneyReward = transform.GetChild(0).transform.Find("Money").GetComponent<TextMeshProUGUI>();
        hitbox = transform.GetChild(1).gameObject;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (isDead || player == null)
        {
            return;
        }

        if (attack)
        {
            Attack();

            rb.isKinematic = true;
        }
        else
        {
            rb.isKinematic = false;

            animator.SetBool("Attack", false);

            Vector3 movePosition = Vector3.MoveTowards(transform.position, player.transform.position, speed * Time.fixedDeltaTime);

            rb.MovePosition(movePosition);
        }
    }

    void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetBool("Attack", true);
            lastAttackTime = Time.time;

            player.GetComponent<Player>().TakeDamage(1);
        }
    }

    public void TakeDamage(int damage)
    {
        if (isDead == false)
        {
            currentHealth -= damage;
            SoundManager.Instance.Play("ZombieHurt");
            SetHealthBar();
            if (currentHealth <= 0)
            {
                isDead = true;
                player.gameObject.GetComponent<Player>().Reward(exp, money);
                deathEffect.Play();
                moneyReward.text = $"+{money}$";
                moneyReward.GetComponent<Animator>().Play("FadeOut");
                GetComponent<SpriteRenderer>().enabled = false;
                GetComponent<Collider2D>().enabled = false;
                hitbox.GetComponent<Collider2D>().enabled = false;
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.ZombieKilled();
                    GameManager.Instance.waveManager.ZombieKilled(-1);
                }
                Multiply();
                StartCoroutine(Destroy(deathEffect.main.duration));
            }
        }
    }

    public void SetSize(float s, int h, float sp)
    {
        size = s;
        maxHealth = h;
        currentHealth = h;
        speed = sp;
    }

    void Multiply()
    {
        if(size > 0.5f)
        {
            for(int i=0;i<2;i++)
            {
                Vector3 position;
                if (i == 1)
                    position = transform.position + new Vector3(0.1f, 0, 0);
                else
                    position = transform.position;

                GameObject clone = Instantiate(GameManager.Instance.waveManager.bigHeadPrefab, position, transform.rotation);
                clone.GetComponent<BossHead>().SetSize(size - 0.5f, maxHealth/2, speed + 0.5f);
            }

            GameManager.Instance.waveManager.ZombieKilled(2);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            attack = true;
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Player"))
        {
            attack = false;
        }
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
