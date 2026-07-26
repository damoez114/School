using UnityEngine;

[CreateAssetMenu(menuName = "Consumable/Jighead Hook")]
public class JigheadHookConsumable : ConsumableItem
{
    [SerializeField] private float damageBonus = 3f;

    public override void Activate()
    {
        RarityDamageBonusManager.AddBonus(Rarity.Rare, damageBonus);
        Debug.Log("Jighead Hook used - all Rare Roe fish permanently deal +" + damageBonus + " damage");
    }
}