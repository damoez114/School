using UnityEngine;

[CreateAssetMenu(menuName = "Roe/Crank Baits/More Damage")]
public class MoreDamageCrankbait : CrankBaitItem
{
    protected override void ApplyUpgrade()
    {

        FishStats.damageMult = 1.25f;
        Debug.Log("Difficulty lowered by 1");
    }
}