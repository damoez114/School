using UnityEngine;

[CreateAssetMenu(menuName = "Roe/Crank Baits/Extra Try")]
public class ExtraTryCrankBait : CrankBaitItem
{
    protected override void ApplyUpgrade()
    {
        RoundManager roundManager = Object.FindFirstObjectByType<RoundManager>();

        if (roundManager == null)
        {
            Debug.LogError("RoundManager not found in scene — cannot apply Extra Try upgrade.");
            return;
        }

        roundManager.GrantExtraTryNow(1);
        Debug.Log("Extra Try purchased — tries increased immediately and permanently.");
    }
}