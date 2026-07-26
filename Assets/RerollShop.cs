using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Reroll : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private float pressOffset = 10f;   // how far it moves down (in pixels)
    [SerializeField] private float speed = 15f;          // how fast it moves/returns

    private RectTransform rect;
    private Vector2 originalPos;
    private Vector2 targetPos;

    public ShopManager shopManager;
    public TMP_Text priceText;

    public int baseRerollCost = 5;
    private int currentRerollCost;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        originalPos = rect.anchoredPosition;
        targetPos = originalPos;
    }

    private void OnEnable()
    {
        // Still resets if this object ever gets disabled/re-enabled elsewhere
        ResetRerollCost();
    }

    // Called explicitly by ShopManager whenever the shop actually opens —
    // OnEnable no longer fires reliably now that the panel stays active and
    // is just slid in/out instead of being deactivated.
    public void ResetRerollCost()
    {
        currentRerollCost = baseRerollCost;
        RefreshPriceDisplay();
    }

    private void Update()
    {
        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * speed);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        targetPos = originalPos + Vector2.down * pressOffset;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        targetPos = originalPos;
    }

    public void RefreshPriceDisplay()
    {
        if (priceText != null)
            priceText.text = shopManager.GetDiscountedCost(currentRerollCost).ToString();
    }

    public void reroll()
    {
        int finalCost = shopManager.GetDiscountedCost(currentRerollCost);

        if (!ShellManager.Instance.SpendShells(finalCost))
        {
            Debug.Log("FAILED: not enough shells");
            return;
        }

        shopManager.RefreshShop();

        currentRerollCost++;
        RefreshPriceDisplay();
    }

}