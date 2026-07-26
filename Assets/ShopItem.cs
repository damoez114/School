using UnityEngine;

public abstract class ShopItem : ScriptableObject
{
    public Sprite icon;
    public int cost;
    public string itemName;

    public virtual bool CanPurchase(HotbarManager hotbarManager)
    {
        return true;
    }

    public abstract void Purchase(HotbarManager hotbarManager);

    // Override in subclasses to supply tooltip-specific info.
    // Returning null/empty for rarity or bonusInfo just hides those lines.
    public virtual string GetDamageDisplay() => "—";
    public virtual string GetRarityDisplay() => "";
    public virtual string GetBonusInfo() => "";
    // in ShopItem.cs
    public virtual string GetDisplayName()
    {
        return string.IsNullOrEmpty(itemName) ? this.name : itemName;
    }
}