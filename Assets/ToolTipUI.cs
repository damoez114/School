using UnityEngine;

public class TooltipUI : MonoBehaviour
{
    public static TooltipUI Instance;

    [SerializeField] private RectTransform roePanelRect;
    [SerializeField] private RoeTooltipPanel roePanel;

    [SerializeField] private RectTransform crankBaitPanelRect;
    [SerializeField] private CrankBaitTooltipPanel crankBaitPanel;

    [SerializeField] private RectTransform consumablePanelRect;
    [SerializeField] private ConsumableTooltipPanel consumablePanel;

    [Header("Offset from cursor")]
    [SerializeField] private float horizontalGap = 20f;
    [SerializeField] private float verticalOffset = 0f;

    private ITooltipPanel activePanel;
    private RectTransform activeRect;
    private bool appearOnRight;

    private void Awake()
    {
        Instance = this;
        roePanel.Hide();
        crankBaitPanel.Hide();
        consumablePanel.Hide();
    }

    private void Update()
    {
        if (activePanel == null || !activePanel.GameObject.activeSelf)
            return;

        FollowCursor();
    }

    private void FollowCursor()
    {
        Vector3 screenPos = Input.mousePosition;
        float xOffset = appearOnRight ? horizontalGap : -horizontalGap;

        activeRect.pivot = appearOnRight ? new Vector2(0f, 0.5f) : new Vector2(1f, 0.5f);
        activeRect.position = screenPos + new Vector3(xOffset, verticalOffset, 0f);
    }

    public void Show(ShopItem item, string extraInfo, bool onRight = false, float damageBonus = 0f)
    {
        HideAll();

        appearOnRight = onRight;

        if (item is RoeItem)
        {
            activePanel = roePanel;
            activeRect = roePanelRect;
            roePanel.Show(item, extraInfo, damageBonus); // use the extended overload directly
        }
        else if (item is CrankBaitItem)
        {
            activePanel = crankBaitPanel;
            activeRect = crankBaitPanelRect;
            crankBaitPanel.Show(item, extraInfo);
        }
        else if (item is ConsumableItem)
        {
            activePanel = consumablePanel;
            activeRect = consumablePanelRect;
            consumablePanel.Show(item, extraInfo);
        }
        else
        {
            return;
        }

        FollowCursor();
    }

    public void Hide()
    {
        HideAll();
    }

    private void HideAll()
    {
        roePanel.Hide();
        crankBaitPanel.Hide();
        consumablePanel.Hide();
        activePanel = null;
    }
}