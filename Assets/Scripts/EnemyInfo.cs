using FishNet.Object;
using System.Collections.Generic;

public enum Type
{
    Normal,
    Spitter,
    Boomer,
    BossPlant,
    BossHead
}

public class EnemyInfo : NetworkBehaviour
{
    public Type type;

    public int maxHealth = 100;
    public int damage = 90;
    public float attackDistance = 5f;
    public float speed = 5f;
    public float attackCooldown = 2;
    public float size = 3f;

    public int exp;
    public int money;
    public List<LootItem> loot;
}
