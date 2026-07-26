using UnityEngine;

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Legendary
}

[CreateAssetMenu(menuName = "Roe/Roe Item")]
public class RoeItem : ShopItem
{
    public GameObject fishPrefab;
    public GameObject roePrefab;

    [Header("Shop Rarity")]
    [SerializeField] private Rarity rarity;
    public Rarity ItemRarity => rarity;

    public override bool CanPurchase(HotbarManager hotbarManager)
    {
        return hotbarManager.GetFirstEmptySlot() != -1;
    }

    public override void Purchase(HotbarManager hotbarManager)
    {
        int emptySlot = hotbarManager.GetFirstEmptySlot();
        hotbarManager.SpawnFromPrefab(emptySlot, roePrefab, this); // <-- add "this"
        hotbarManager.SaveHotbar();
    }

    public override string GetDamageDisplay()
    {
        if (fishPrefab == null) return "—";

        FishStats stats = fishPrefab.GetComponent<FishStats>();
        return stats != null ? stats.damage.ToString() : "—";
    }

    public override string GetRarityDisplay() => rarity.ToString();
    // in RoeItem.cs
    [Header("Tooltip")]
    [SerializeField] private string bonusDescription = "";

    public override string GetBonusInfo() => bonusDescription;
}