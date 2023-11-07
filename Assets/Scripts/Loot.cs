using UnityEngine;

public static class Loot
{
    public static Item Drop(LootItem lootBasic, float chance)
    {
        if (!ChanceToDrop(chance)) return null;

        if (lootBasic.item.GetType() == typeof(Weapon))
        {
            Weapon weapon = (Weapon)lootBasic.item.GetCopy();
            return weapon;
        }
        else
        {
            Item item = lootBasic.item.GetCopy();
            return item;
        }
    }

    static bool ChanceToDrop(float chance)
    {
        int randomNumber = Random.Range(1, 101);
        //Debug.Log(randomNumber);
        if (randomNumber <= chance * 100)
        {
            return true;
        }
        else return false;
    }
}
