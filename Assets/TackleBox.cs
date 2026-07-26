using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Consumable/Tackle Box")]
public class TackleBox : ConsumableItem
{
    public override void Activate()
    {
        ConsumableManager consumableManager = Object.FindFirstObjectByType<ConsumableManager>();

        if (consumableManager == null)
        {
            Debug.LogError("ConsumableManager not found in scene.");
            return;
        }

        int emptySlots = consumableManager.GetEmptySlotCount();

        if (emptySlots == 0)
        {
            Debug.Log("Tackle Box Used - No space in consumable bar");
            return;
        }

        // Only ever hands out 2, but caps to whatever room is actually left
        // (e.g. only 1 empty slot -> only 1 item granted)
        int spawnCount = Mathf.Min(2, emptySlots);

        ConsumableItem[] allConsumables = ShopManager.Instance != null ? ShopManager.Instance.allConsumableItems : null;

        if (allConsumables == null)
        {
            Debug.LogWarning("Tackle Box Used - ShopManager not found, can't source consumable pool");
            return;
        }

        List<ConsumableItem> pool = new List<ConsumableItem>();
        foreach (var entry in allConsumables)
        {
            if (entry == null) continue;
            if (entry == this) continue; // exclude the Tackle Box itself

            pool.Add(entry);
        }

        if (pool.Count == 0)
        {
            Debug.LogWarning("Tackle Box Used - No other consumables configured in ShopManager.allConsumableItems");
            return;
        }

        for (int i = 0; i < spawnCount; i++)
        {
            if (pool.Count == 0) break; // ran out of unique items to hand out

            int index = Random.Range(0, pool.Count);
            ConsumableItem chosen = pool[index];
            pool.RemoveAt(index); // don't grant the same item twice in one use

            consumableManager.AddConsumable(chosen);

            Debug.Log("Tackle Box Used - Granted " + chosen.name);
        }
    }
}