using FishNet.Object;
using System;
using UnityEngine;

public class Acid : NetworkBehaviour, IPooledObject
{
    public ObjectPooler Pool { get; set; }
    //public float lifeTime;
    public float forcePower = 10f;

    private void OnEnable()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(transform.right * forcePower, ForceMode2D.Impulse);
    }

    private void Start()
    {
        //Destroy(gameObject, lifeTime);       
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("Hitbox"))
        {
            collision.gameObject.transform.root.GetComponent<Player>().TakeDamage(1);
        }
        // Pool.ReturnToPool(gameObject);
        Despawn();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Pool.ReturnToPool(gameObject);
        Despawn();
    }
}
