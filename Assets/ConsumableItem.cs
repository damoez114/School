using UnityEngine;

public abstract class ConsumableItem : ShopItem
{
    [Header("Tooltip")]
    [TextArea(2, 5)]
    [SerializeField] private string bonusDescription = "";

    public override string GetBonusInfo() => bonusDescription;

    // Called by ShopManager.BuyItem when purchased — routes into the consumable hotbar instead of the roe hotbar
    public override void Purchase(HotbarManager hotbarManager)
    {
        ConsumableManager consumableManager = Object.FindFirstObjectByType<ConsumableManager>();

        if (consumableManager == null)
        {
            Debug.LogError("ConsumableManager not found in scene.");
            return;
        }

        consumableManager.AddConsumable(this);
    }
    public override bool CanPurchase(HotbarManager hotbarManager)
    {
        ConsumableManager consumableManager = Object.FindFirstObjectByType<ConsumableManager>();
        return consumableManager != null && consumableManager.GetFirstEmptySlot() != -1;
    }
    // Called when the player clicks the consumable in its hotbar slot
    public abstract void Activate();
}