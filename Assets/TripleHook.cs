using UnityEngine;

[CreateAssetMenu(menuName = "Consumable/Triple Hook")]
public class TripleHookConsumable : ConsumableItem
{
    [SerializeField] private float damageBonus = 2.0f;

    public override void Activate()
    {
        TripleHookTargeting.BeginTargeting(damageBonus);
        Debug.Log("Triple Hook used - select a Roe to boost damage");
    }
}