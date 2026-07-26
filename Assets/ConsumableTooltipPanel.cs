using UnityEngine;
using TMPro;

public class ConsumableTooltipPanel : MonoBehaviour, ITooltipPanel
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;

    public GameObject GameObject => gameObject;

    public void Show(ShopItem item, string costDisplay)
    {
        nameText.text = item.GetDisplayName();
        descriptionText.text = item.GetBonusInfo(); // uses the same bonusDescription field pattern as RoeItem
        costText.text = "Cost: " + costDisplay;

        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}