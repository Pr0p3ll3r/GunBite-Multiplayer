using System.Collections;
using UnityEngine;
using TMPro;
using FishNet.Object;

public class Zombie : EnemyInfo, IDamageable
{
    [SerializeField] private ParticleSystem deathEffect;
    [SerializeField] private Transform spitPoint;
    [SerializeField] private GameObject acidPrefab;
    [SerializeField] private ParticleSystem explodeEffect;
    [SerializeField] private float radius;

    private int currentHealth;
    private float lastAttackTime = 0;
    private bool isDead;

    private Transform player;
    private Animator animator;
    private GameObject healthBar;
    private TextMeshProUGUI moneyReward;
    private SpriteRenderer sprite;
    private GameObject hitbox;
    private Rigidbody2D rb;
    private ObjectPooler acidPooler;

    void Start()
    {
        //acidPooler = GameManager.Instance.acidPooler;
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        sprite = GetComponent<SpriteRenderer>();
        healthBar = transform.GetChild(0).transform.Find("HealthBar").gameObject;
        moneyReward = transform.GetChild(0).transform.Find("Money").GetComponent<TextMeshProUGUI>();
        hitbox = transform.GetChild(1).gameObject;
        rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!IsServer) return;

        if (GetClosestPlayer() != null)
            player = GetClosestPlayer().transform;

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

    GameObject GetClosestPlayer()
    {
        Vector3 position = transform.position;
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        GameObject closestPlayer = null;
        float minimumDistance = 1000000f;

        foreach (GameObject player in players)
        {
            float distanceToPlayer = Vector2.Distance(position, player.transform.position);

            if (distanceToPlayer < minimumDistance)
            {
                closestPlayer = player;
                minimumDistance = distanceToPlayer;
            }
        }

        return closestPlayer;
    }

    void Attack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            animator.SetBool("Attack", true);
            lastAttackTime = Time.time;

            if (type == Type.Spitter)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                GameObject acid = Instantiate(acidPrefab, spitPoint.transform.position, Quaternion.Euler(new Vector3(0, 0, angle)));
                Spawn(acid);
            }
            else
            {
                player.GetComponent<Player>().TakeDamage(1);
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void TakeDamage(int damage)
    {
        if (isDead) return;
        currentHealth -= damage;
        SoundManager.Instance.Play("ZombieHurt");
        SetHealthBar();
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    [ObserversRpc]
    public virtual void Die()
    {
        isDead = true;
        player.gameObject.GetComponent<Player>().Reward(exp, money);
        moneyReward.text = $"+{money}$";
        moneyReward.GetComponent<Animator>().Play("FadeOut");
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;
        hitbox.GetComponent<Collider2D>().enabled = false;
        if (GameManager.Instance != null) GameManager.Instance.EnemyKilled();
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
        foreach (LootItem lootItem in loot)
        {
            if (Loot.Drop(lootItem, lootItem.chanceToDrop))
            {
                GameObject sceneObject = Instantiate(lootItem.item.pickUp, transform.position, transform.rotation);
                sceneObject.GetComponent<ItemPickup>().item = lootItem.item;
                break;
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.DrawSphere(transform.position, radius);
    //}
}
