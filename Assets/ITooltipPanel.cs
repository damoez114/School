using UnityEngine;

public interface ITooltipPanel
{
    void Show(ShopItem item, string extraInfo);
    void Hide();
    GameObject GameObject { get; }
}