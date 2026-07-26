using UnityEngine;

public abstract class CrankBaitItem : ShopItem
{
    [TextArea]
    public string description;

    private void OnValidate()
    {
        cost = 10; // all crank baits cost the same, locked in the editor
    }

    public override void Purchase(HotbarManager hotbarManager)
    {
        ApplyUpgrade();
    }

    protected abstract void ApplyUpgrade();
}