using UnityEngine;

[CreateAssetMenu(menuName = "Roe/Crank Baits/Lower Rounds")]
public class LowerRoundCrankbait : CrankBaitItem
{
    protected override void ApplyUpgrade()
    {
        RoundManager roundManager = Object.FindFirstObjectByType<RoundManager>();

        if (roundManager == null)
        {
            Debug.LogError("RoundManager not found in scene — cannot apply Lower Rounds upgrade.");
            return;
        }

        roundManager.LowerRounds(2);
        Debug.Log("Difficulty lowered by 2 (extra -1 if it landed on difficulty 6)");
    }
}