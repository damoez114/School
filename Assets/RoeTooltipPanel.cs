using UnityEngine;
using TMPro;

public class RoeTooltipPanel : MonoBehaviour, ITooltipPanel
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text damageText;
    [SerializeField] private TMP_Text sellValueText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text bonusText;

    public GameObject GameObject => gameObject;

    // Interface version — used by generic callers, no extra damage bonus
    public void Show(ShopItem item, string sellValueDisplay)
    {
        Show(item, sellValueDisplay, 0f);
    }

    // Extended version — used by RoeTooltipTrigger, includes Triple Hook bonus
    public void Show(ShopItem item, string sellValueDisplay, float damageBonus)
    {
        nameText.text = item.GetDisplayName();

        float baseDamage = 0f;
        string baseDisplay = item.GetDamageDisplay();
        float.TryParse(baseDisplay, out baseDamage);

        float totalDamage = baseDamage + damageBonus;
        damageText.text = "Damage: " + totalDamage;

        sellValueText.text = "Sell Value: " + sellValueDisplay;

        string rarity = item.GetRarityDisplay();
        if (string.IsNullOrEmpty(rarity))
            rarityText.gameObject.SetActive(false);
        else
        {
            rarityText.gameObject.SetActive(true);
            rarityText.text = "Rarity: " + rarity;
        }

        string bonus = item.GetBonusInfo();
        if (string.IsNullOrEmpty(bonus))
            bonusText.gameObject.SetActive(false);
        else
        {
            bonusText.gameObject.SetActive(true);
            bonusText.text = bonus;
        }

        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}