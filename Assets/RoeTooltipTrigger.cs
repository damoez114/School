using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Roe))]
public class RoeTooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Roe roe;

    private void Awake()
    {
        roe = GetComponent<Roe>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {

        if (TooltipUI.Instance == null || roe.SourceItem == null)
            return;

        int cost = HotbarManager.Instance.GetCostByID(roe.itemID);
        int sellValue = Mathf.RoundToInt(cost / 3f);

        TooltipUI.Instance.Show(roe.SourceItem, sellValue.ToString(), false, roe.DamageBonus);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipUI.Instance == null)
            return;

        TooltipUI.Instance.Hide();
    }
}