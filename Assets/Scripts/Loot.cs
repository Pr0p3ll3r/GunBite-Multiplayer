using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Loot
{
    public static Item Drop(LootItem lootBasic, float chance)
    {
        if (!ChanceToDrop(chance)) return null;

        if (lootBasic.item.GetType() == typeof(Weapon))
        {
            Weapon weapon = ScriptableObject.CreateInstance<Weapon>();
            weapon.NewAsset(lootBasic.item);
            return weapon;
        }
        else
        {
            Item item = ScriptableObject.CreateInstance<Item>();
            item.NewAsset(lootBasic.item);
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
