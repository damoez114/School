using UnityEngine;

[CreateAssetMenu(menuName = "Roe/Crank Baits/Shells Discount")]
public class ShellsOffCrankBait : CrankBaitItem
{
    protected override void ApplyUpgrade()
    {
        ShopManager shopManager = Object.FindFirstObjectByType<ShopManager>();

        if (shopManager == null)
        {
            Debug.LogError("ShopManager not found in scene — cannot apply Shells Off upgrade.");
            return;
        }

        shopManager.ApplyShellDiscount(0.2f); // 20% off
        Debug.Log("Shells Off purchased — 20% discount applied to all shop items.");
    }
}