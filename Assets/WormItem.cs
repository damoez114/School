using UnityEngine;

[CreateAssetMenu(menuName = "Consumable/Worm")]
public class WormConsumable : ConsumableItem
{
    [SerializeField] private float growthPercent = 0.10f;

    public override void Activate()
    {
        FishGrowthTargeting.BeginTargeting(growthPercent);
        Debug.Log("Worm used - select a Roe to grow");
    }
}