using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Zombie : ZombieInfo, IDamageable
{
    private int currentHealth;

    private Transform player;
    [SerializeField] private ParticleSystem deathEffect;
    private Animator animator;
    private GameObject healthBar;
    private TextMeshProUGUI moneyReward;
    private SpriteRenderer sprite;
    private GameObject hitbox;
    private Rigidbody2D rb;

    [SerializeField] private Transform spitPoint;
    private ObjectPooler acidPooler;

    [SerializeField] private ParticleSystem explodeEffect;
    [SerializeField] private float radius;

    private float lastAttackTime = 0;
    private bool isDead;

    void Start()
    {
        acidPooler = GameManager.Instance.acidPooler;
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
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

        Vector2 direction = (player.position - transform.position).normalized;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        if (angle >= -90 && angle <= 90)
        {
            sprite.flipX = false;
        }
        else
        {
            sprite.flipX = true;
        }
    }

    private void FixedUpdate()
    {
        if (isDead || player == null)
        {
            return;
        }

        if (Vector3.Distance(transform.position, player.position) <= attackDistance)
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
        if(Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetBool("Attack", true);
            lastAttackTime = Time.time;

            if (type == Type.Spitter)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                GameObject acid = acidPooler.Get();
                acid.transform.position = spitPoint.position;
                acid.transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
                acid.SetActive(true);
            }
            else
            {
                player.GetComponent<Player>().TakeDamage(1);
            }
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
                Die();
            }
        }
    }

    public virtual void Die()
    {
        isDead = true;
        player.gameObject.GetComponent<Player>().Reward(exp, money);
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
        if (type == Type.Boomer) Explode();
        else
        {
            deathEffect.Play();
            StartCoroutine(Destroy(deathEffect.main.duration));
        }
        Drop();
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

    void Explode()
    {
        explodeEffect.Play();
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D col in objectsInRange)
        {
            if (col.gameObject.tag.Equals("Player"))
            {
                col.gameObject.transform.root.GetComponent<Player>().TakeDamage(1);
            }
        }
        StartCoroutine(Destroy(explodeEffect.main.duration));
    }

    void Drop()
    {
        List<Item> droppedItems = Loot.Drop(loot);
        foreach(Item item in droppedItems)
        {
            GameObject sceneObject = Instantiate(item.pickUp, transform.position, transform.rotation);
            sceneObject.GetComponent<ItemPickup>().item = item;
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawSphere(transform.position, radius);
    //}
}
