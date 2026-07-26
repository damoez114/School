using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopManager : MonoBehaviour
{
    public GameObject winScreen;

    public RoeItem[] allItems;
    public ShopSlot[] shopSlots;
    public ConsumableItem[] consumableItems;

    [Header("Crank Bait Section")]
    public CrankBaitItem[] crankBaitItems;
    public ShopSlot crankBaitSlot;          // dedicated single slot
    public int crankBaitRoundInterval = 4;
    [Header("Consumable Section")]
    public ConsumableItem[] allConsumableItems;
    public ShopSlot[] consumableShopSlots;
    private int roundsSinceLastCrankBait = 0;
    private bool crankBaitSlotFilled = false;
    public Rarity[] RarityOrder => rarityOrder;
    public float[] RarityWeights => rarityWeights;

    public HotbarManager hotbarManager;
    public ShopPanelSlider panelSlider;
    [Header("Audio")]
    public AudioClip purchaseSound;
    public static bool IsShopOpen { get; private set; } // <-- add

    void Start()
    {
        winScreen.SetActive(false);
        RefreshShop();
        RefreshCrankBaitSlot();
        panelSlider.Show();
        IsShopOpen = true; // <-- add

        if (rerollButton != null)
            rerollButton.ResetRerollCost();
    }

    public void OpenShop()
    {
        RefreshShop();
        panelSlider.Show();
        IsShopOpen = true; // <-- add

        if (rerollButton != null)
            rerollButton.ResetRerollCost();
    }

    public void CloseShop()
    {
        panelSlider.Hide();
        IsShopOpen = false;

        HideAllBuyTags();

        // Fresh attempt starts once the shop is exited — un-grey every roe
        hotbarManager.ResetAllPlacedStates();
    }

    private void HideAllBuyTags()
    {
        foreach (var slot in shopSlots)
        {
            if (slot != null && slot.buyTag != null)
                slot.buyTag.HideTag();
        }

        foreach (var slot in consumableShopSlots)
        {
            if (slot != null && slot.buyTag != null)
                slot.buyTag.HideTag();
        }

        if (crankBaitSlot != null && crankBaitSlot.buyTag != null)
            crankBaitSlot.buyTag.HideTag();
    }

    // Call once per round from your round/wave system
    public void AdvanceRound()
    {
        roundsSinceLastCrankBait++;
        RefreshCrankBaitSlot();
    }

    [Header("Rarity Odds")]
    [SerializeField] private Rarity[] rarityOrder = new Rarity[] { Rarity.Common, Rarity.Uncommon, Rarity.Rare, Rarity.Legendary };
    [SerializeField] private float[] rarityWeights = new float[] { 60f, 28f, 10f, 2f }; // must match rarityOrder length

    public void RefreshShop()
    {
        Debug.Log("Refreshing shop...");

        List<RoeItem> pool = new List<RoeItem>(allItems);

        for (int i = 0; i < shopSlots.Length; i++)
        {
            if (pool.Count == 0)
            {
                shopSlots[i].SetItem(null); // <-- explicitly clear instead of leaving stale
                continue;
            }

            RoeItem chosen = PickWeightedRoe(pool);
            if (chosen == null)
            {
                shopSlots[i].SetItem(null); // <-- same here
                continue;
            }

            shopSlots[i].SetItem(chosen);
            pool.Remove(chosen);
        }

        List<ConsumableItem> consumablePool = new List<ConsumableItem>(allConsumableItems);

        for (int i = 0; i < consumableShopSlots.Length; i++)
        {
            if (consumablePool.Count == 0)
            {
                consumableShopSlots[i].SetItem(null); // <-- explicitly clear
                continue;
            }

            int index = Random.Range(0, consumablePool.Count);
            ConsumableItem chosen = consumablePool[index];

            consumableShopSlots[i].SetItem(chosen);
            consumablePool.RemoveAt(index);
        }
    }

    // Rolls a rarity per your configured odds, then picks a random roe of that
    // rarity from the remaining pool. Falls back to progressively lower rarities
    // (then any remaining roe at all) if the rolled rarity has none left —
    // otherwise a slot could come up empty just because e.g. all Legendaries
    // already got used this refresh.
    private RoeItem PickWeightedRoe(List<RoeItem> pool)
    {
        Rarity rolled = RollRarity();

        RoeItem match = TryPickFromRarity(pool, rolled);
        if (match != null) return match;

        // Fallback: rolled rarity had nothing left in the pool, so just grab
        // anything remaining rather than skipping the slot entirely
        if (pool.Count > 0)
            return pool[Random.Range(0, pool.Count)];

        return null;
    }

    private RoeItem TryPickFromRarity(List<RoeItem> pool, Rarity rarity)
    {
        List<RoeItem> matching = pool.FindAll(item => item.ItemRarity == rarity);
        if (matching.Count == 0) return null;

        return matching[Random.Range(0, matching.Count)];
    }

    private Rarity RollRarity()
    {
        float totalWeight = 0f;
        foreach (float w in rarityWeights)
            totalWeight += w;

        float roll = Random.Range(0f, totalWeight);
        float cumulative = 0f;

        for (int i = 0; i < rarityOrder.Length; i++)
        {
            cumulative += rarityWeights[i];
            if (roll <= cumulative)
                return rarityOrder[i];
        }

        return rarityOrder[rarityOrder.Length - 1]; // fallback, shouldn't normally hit this
    }

    private void RefreshCrankBaitSlot()
    {
        if (crankBaitSlot == null) return;
        if (crankBaitSlotFilled) return;

        // Only wait for the interval if this isn't the first-ever fill
        bool firstFill = crankBaitSlot.currentItem == null && roundsSinceLastCrankBait == 0;
        if (!firstFill && roundsSinceLastCrankBait < crankBaitRoundInterval) return;

        if (crankBaitItems == null || crankBaitItems.Length == 0)
        {
            Debug.LogWarning("No crank bait items assigned.");
            return;
        }

        CrankBaitItem chosen = crankBaitItems[Random.Range(0, crankBaitItems.Length)];
        crankBaitSlot.SetItem(chosen);

        crankBaitSlotFilled = true;
        roundsSinceLastCrankBait = 0;
    }

    public void BuyItem(ShopItem item, ShopSlot slot)
    {
        Debug.Log("BUY STARTED: " + item.name);

        if (!item.CanPurchase(hotbarManager))
        {
            Debug.Log("FAILED: cannot purchase right now (e.g. hotbar full)");
            return;
        }

        int finalCost = GetDiscountedCost(item);

        if (!ShellManager.Instance.SpendShells(finalCost))
        {
            Debug.Log("FAILED: not enough shells");
            return;
        }

        item.Purchase(hotbarManager);

        // Play purchase sound only once we know the buy actually went through
        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(purchaseSound);

        slot.Clear();

        if (slot == crankBaitSlot)
            crankBaitSlotFilled = false;
    }

    [Header("Crank Bait Upgrades")]
    public float shellDiscountMultiplier = 1f; // 1 = no discount, 0.8 = 20% off
    public Reroll rerollButton; // <-- add this, drag your Reroll object in
    public int GetDiscountedCost(ShopItem item)
    {
        return GetDiscountedCost(item.cost);
    }

    public int GetDiscountedCost(int baseCost)
    {
        return Mathf.RoundToInt(baseCost * shellDiscountMultiplier);
    }

    public void ApplyShellDiscount(float percentOff)
    {
        shellDiscountMultiplier *= (1f - percentOff);
        Debug.Log("New shell discount multiplier: " + shellDiscountMultiplier);

        RefreshDisplayedPrices();
    }

    private void RefreshDisplayedPrices()
    {
        foreach (var slot in shopSlots)
        {
            slot.RefreshPriceDisplay();
        }

        if (crankBaitSlot != null)
            crankBaitSlot.RefreshPriceDisplay();

        if (rerollButton != null)
            rerollButton.RefreshPriceDisplay();
    }
    public static ShopManager Instance;

    void Awake()
    {
        Instance = this;
    }
}