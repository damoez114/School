using UnityEngine;

[CreateAssetMenu(menuName = "Consumable/Weighted Hook")]
public class WeightedHookConsumable : ConsumableItem
{
    [SerializeField] private float damageBonus = 3f;

    public override void Activate()
    {
        RarityDamageBonusManager.AddBonus(Rarity.Uncommon, damageBonus);
        Debug.Log("Weighted Hook used - all Uncommon Roe fish permanently deal +" + damageBonus + " damage");
    }
}