using System;
using UnityEngine;

public class Bullet : MonoBehaviour, IPooledObject
{
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float radius = 10f;

    private int damage;
    private Vector3 mousePos;
    private bool isGrenade;

    public ObjectPooler Pool { get; set; }

    public void SetDamage(int amount, bool grenade)
    {
        damage = amount;
        if (grenade)
        {
            isGrenade = true;
            mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        } 
        else isGrenade = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag.Equals("Enemy") && !isGrenade)
        {
            collision.gameObject.transform.root.GetComponent<IDamageable>()?.TakeDamage(damage);
        }
        else if (collision.gameObject.tag.Equals("Enemy") && isGrenade)
        {
            Detonate();
        }
        Pool.ReturnToPool(gameObject);
    }

    private void Update()
    {
        if (isGrenade)
        {
            transform.position = Vector2.MoveTowards(transform.position, mousePos, Time.deltaTime * WeaponLibrary.FindGun("Grenade Launcher").bulletForce);

            if (transform.position == mousePos)
            {
                Detonate();
                Pool.ReturnToPool(gameObject);
            }
        }
    }

    private void Detonate()
    {
        //Debug.Log("Explosion");
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        SoundManager.Instance.PlayOneShot("Explosion");
        Collider2D[] objectsInRange = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D col in objectsInRange)
        {
            if (col.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox"))
            {
                Vector2 closestPoint = col.ClosestPoint(transform.position);
                float distance = Vector3.Distance(closestPoint, transform.position);

                float damagePercent = Mathf.InverseLerp(radius, 0, distance);
                int damageToApply = damage;

                if (damagePercent < 0.95) damageToApply = (int)(damageToApply * damagePercent);
                //Debug.Log("Damage: " + damageToApply);
                col.gameObject.transform.root.GetComponent<IDamageable>()?.TakeDamage(damageToApply);
            }
        }
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawSphere(transform.position, radius);
    //}
}
