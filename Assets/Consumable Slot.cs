using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ConsumableSlot : MonoBehaviour, IPointerClickHandler
{
    public ConsumableItem currentItem;
    public Image icon;
    public ConsumableUseTagAppear useTag;

    [Header("Sound")]
    [SerializeField] private AudioClip clickSound;

    public void SetItem(ConsumableItem item)
    {
        currentItem = item;

        if (icon == null)
        {
            Debug.LogError("ICON NOT ASSIGNED on " + gameObject.name);
            return;
        }

        if (item == null)
        {
            icon.enabled = false;
            return;
        }

        icon.sprite = item.icon;
        icon.enabled = true;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (currentItem == null) return;

        if (useTag == null)
        {
            Debug.LogError("USE TAG NOT ASSIGNED on " + gameObject.name);
            return;
        }

        if (SFXManager.Instance != null)
            SFXManager.Instance.PlaySound(clickSound);

        useTag.ToggleTag(currentItem, this); // no-ops internally if a cancel is currently pending
    }

    public void Clear()
    {
        currentItem = null;
        if (icon != null)
            icon.enabled = false;
    }
}