using UnityEngine;

[CreateAssetMenu(menuName = "Consumable/Simple Hook")]
public class SimpleHookConsumable : ConsumableItem
{
    [SerializeField] private float damageBonus = 3f;

    public override void Activate()
    {
        RarityDamageBonusManager.AddBonus(Rarity.Common, damageBonus);
        Debug.Log("Simple Hook used - all Common Roe fish permanently deal +" + damageBonus + " damage");
    }
}