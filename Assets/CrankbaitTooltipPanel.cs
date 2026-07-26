using UnityEngine;
using TMPro;

public class CrankBaitTooltipPanel : MonoBehaviour, ITooltipPanel
{
    [SerializeField] private GameObject panel;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text costText;

    public GameObject GameObject => gameObject;

    public void Show(ShopItem item, string costDisplay)
    {
        CrankBaitItem crankBait = item as CrankBaitItem;

        nameText.text = item.GetDisplayName();
        descriptionText.text = crankBait != null ? crankBait.description : "";
        costText.text = "Cost: " + costDisplay;

        panel.SetActive(true);
    }

    public void Hide()
    {
        panel.SetActive(false);
    }
}