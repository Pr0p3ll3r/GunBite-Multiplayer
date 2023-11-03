using System;
using UnityEngine;

public class Acid : MonoBehaviour, IPooledObject
{
    [SerializeField] private float forcePower = 10f;

    public ObjectPooler Pool { get; set; }

    private void OnEnable()
    {
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.AddForce(transform.right * forcePower, ForceMode2D.Impulse);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.name.Equals("Hitbox"))
        {
            collision.gameObject.transform.root.GetComponent<Player>().TakeDamage(1);
        }
        Pool.ReturnToPool(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Pool.ReturnToPool(gameObject);
    }
}
