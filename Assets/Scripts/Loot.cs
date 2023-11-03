using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Loot 
{
    public static List<Item> Drop(LootItem[] loot)
    {
        List<Item> droppedItems = new List<Item>();

        foreach(LootItem lootItem in loot)
        {
            int roll = Random.Range(0, 100);
            //Debug.Log(roll);
            if (roll <= lootItem.dropChance)
            {
                droppedItems.Add(lootItem.item);
            }
        }
        return droppedItems;
    }
}
