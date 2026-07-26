using UnityEngine;

public static class RarityDamageBonusManager
{
    private static float commonBonus = 0f;
    private static float uncommonBonus = 0f;
    private static float rareBonus = 0f;

    public static void AddBonus(Rarity rarity, float amount)
    {
        switch (rarity)
        {
            case Rarity.Common:
                commonBonus += amount;
                break;
            case Rarity.Uncommon:
                uncommonBonus += amount;
                break;
            case Rarity.Rare:
                rareBonus += amount;
                break;
                // Legendary intentionally not supported by any hook item
        }
    }

    public static float GetBonus(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return commonBonus;
            case Rarity.Uncommon: return uncommonBonus;
            case Rarity.Rare: return rareBonus;
            default: return 0f;
        }
    }
    public static void ResetAll()
    {
        commonBonus = 0f;
        uncommonBonus = 0f;
        rareBonus = 0f;
    }
}
