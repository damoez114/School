using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class BuyTagAppear : MonoBehaviour
{
    public GameObject buyTag;

    private CanvasGroup canvasGroup;
    private bool isActive;

    private ShopManager shopManager;
    private ShopItem currentItem;

    public bool IsShowing => isActive;

    private void Awake()
    {
        canvasGroup = buyTag != null ? buyTag.GetComponent<CanvasGroup>() : GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            Debug.LogError("BuyTagAppear needs a CanvasGroup on the buyTag object.");

        HideTag();
    }

    private void Start()
    {
        shopManager = FindFirstObjectByType<ShopManager>();
    }

    public void SetItem(ShopItem item)
    {
        currentItem = item;
    }

    public void ToggleTag()
    {
        isActive = !isActive;
        ApplyVisibility();
    }

    public void ShowTag(ShopItem item)
    {
        currentItem = item;
        isActive = true;
        ApplyVisibility();
    }

    public void HideTag()
    {
        isActive = false;
        ApplyVisibility();
    }

    private void ApplyVisibility()
    {
        if (canvasGroup == null) return;

        canvasGroup.alpha = isActive ? 1f : 0f;
        canvasGroup.interactable = isActive;
        canvasGroup.blocksRaycasts = isActive;
    }
}