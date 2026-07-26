using UnityEngine;

[CreateAssetMenu(menuName = "Consumable/Bobber")]
public class BobberConsumable : ConsumableItem
{
    public override void Activate()
    {
        int emptySlot = HotbarManager.Instance.GetFirstEmptySlot();

        if (emptySlot == -1)
        {
            Debug.Log("Bobber Used - No space in hotbar");
            return;
        }

        Rarity rolled = RollRarityExcludingLegendary();

        HotbarManager.HotbarItemEntry roeEntry = HotbarManager.Instance.GetRandomRoeEntryByRarity(rolled);

        // Fallback: if nothing of the rolled rarity is registered, just grab any roe
        // rather than the Bobber doing nothing
        if (roeEntry == null)
        {
            roeEntry = HotbarManager.Instance.GetRandomRoeEntry();
        }

        if (roeEntry == null)
        {
            Debug.LogWarning("Bobber Used - No Roe entries configured in HotbarManager.allItems");
            return;
        }

        HotbarManager.Instance.SpawnFromPrefab(emptySlot, roeEntry.prefab, roeEntry.sourceItem);

        Debug.Log("Bobber Used - Spawned " + roeEntry.itemID + " (" + rolled + ")");
    }

    private Rarity RollRarityExcludingLegendary()
    {
        Rarity[] order = ShopManager.Instance.RarityOrder;
        float[] baseWeights = ShopManager.Instance.RarityWeights;

        float total = 0f;
        for (int i = 0; i < order.Length; i++)
        {
            if (order[i] == Rarity.Legendary) continue;
            total += baseWeights[i];
        }

        float legendaryWeight = 0f;
        for (int i = 0; i < order.Length; i++)
        {
            if (order[i] == Rarity.Legendary)
                legendaryWeight = baseWeights[i];
        }

        float roll = Random.Range(0f, total);
        float cumulative = 0f;

        for (int i = 0; i < order.Length; i++)
        {
            if (order[i] == Rarity.Legendary) continue;

            float weight = baseWeights[i];
            if (order[i] == Rarity.Rare)
                weight += legendaryWeight; // fold Legendary's slice into Rare

            cumulative += weight;
            if (roll <= cumulative)
                return order[i];
        }

        return Rarity.Common; // fallback, shouldn't normally hit this
    }
}