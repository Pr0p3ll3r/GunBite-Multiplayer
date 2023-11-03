using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type
{
    Normal,
    Spitter,
    Boomer,
    BossPlant,
    BossHead
}

public class ZombieInfo : MonoBehaviour
{
    public Type type;

    public int maxHealth = 100;
    public float attackDistance = 5f;
    public float speed = 5f;
    public float attackCooldown = 2;
    public float size = 3f;

    public int exp;
    public int money;
    public LootItem[] loot;
}
