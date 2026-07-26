using UnityEngine;

[CreateAssetMenu(menuName = "Consumable/Water Bottle")]
public class WaterBottleConsumable : ConsumableItem
{
    public override void Activate()
    {
        RoundManager roundManager = Object.FindFirstObjectByType<RoundManager>();

        if (roundManager == null)
        {
            Debug.LogError("RoundManager not found in scene — cannot apply Water Bottle effect.");
            return;
        }

        roundManager.GrantTemporaryTry(1);
        Debug.Log("Water Bottle used — +1 try this round.");
    }
}