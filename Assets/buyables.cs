using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public ShopItem currentItem;

    public Image icon;
    public TMP_Text priceText;

    public BuyTagAppear buyTag;
    public ShopManager shopManager;

    [Header("Sound")]
    [SerializeField] private AudioClip clickSound;

    public void SetItem(ShopItem item)
    {
        currentItem = item;

        if (buyTag != null)
            buyTag.HideTag();

        if (item == null)
            return;

        if (icon == null)
        {
            Debug.LogError("ICON NOT ASSIGNED on " + gameObject.name);
            return;
        }

        if (priceText == null)
        {
            Debug.LogError("PRICE TEXT NOT ASSIGNED on " + gameObject.name);
            return;
        }

        icon.sprite = item.icon;
        icon.enabled = true;

        RefreshPriceDisplay();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null)
            return;

        if (buyTag == null)
        {
            Debug.LogError("BUY TAG NOT ASSIGNED on " + gameObject.name);
            return;
        }

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(clickSound);

        // Clicking an already-open tag closes it; otherwise open it.
        if (buyTag.IsShowing)
            buyTag.HideTag();
        else
            buyTag.ShowTag(currentItem);
    }

    public void Buy()
    {
        if (currentItem == null)
        {
            Debug.Log("No item in slot");
            return;
        }

        if (buyTag != null)
            buyTag.HideTag(); // hide before purchase logic runs

        shopManager.BuyItem(currentItem, this);
    }

    public void Clear()
    {
        currentItem = null;

        if (icon != null)
            icon.enabled = false;

        if (priceText != null)
            priceText.text = "";

        buyTag.HideTag();

        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();
    }

    public void RefreshPriceDisplay()
    {
        if (currentItem == null || priceText == null) return;

        int displayCost = shopManager != null ? shopManager.GetDiscountedCost(currentItem) : currentItem.cost;
        priceText.text = displayCost.ToString();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem == null || TooltipUI.Instance == null)
            return;

        int displayCost = shopManager != null ? shopManager.GetDiscountedCost(currentItem) : currentItem.cost;

        TooltipUI.Instance.Show(currentItem, displayCost.ToString(), true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance != null)
            TooltipUI.Instance.Hide();
    }
}