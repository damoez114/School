using UnityEngine;
using UnityEngine.EventSystems;

public class HotbarSlot : MonoBehaviour, IDropHandler
{
    public Roe currentRoe;
    public string itemID;
    public HotbarManager hotbarManager;

    public void SetRoe(Roe roe)
    {
        currentRoe = roe;
        itemID = roe != null ? roe.itemID : "";

        hotbarManager.SaveHotbar();
    }

    public void Clear()
    {
        currentRoe = null;
        itemID = "";
    }

    // IMPORTANT: DO NOTHING HERE
    public void OnDrop(PointerEventData eventData)
    {
        // Intentionally empty
    }
}