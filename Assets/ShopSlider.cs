using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class ShopPanelSlider : MonoBehaviour
{
    [SerializeField] private float slideDistance = 800f;
    [SerializeField] private float speed = 8f;

    private RectTransform rect;
    private CanvasGroup canvasGroup;
    private Vector2 onScreenPos;
    private Vector2 offScreenPos;
    private Vector2 targetPos;

    private bool initialized = false;
    private bool isShown = false;

    private void Init()
    {
        if (initialized) return;

        rect = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        onScreenPos = rect.anchoredPosition;
        offScreenPos = onScreenPos + Vector2.up * slideDistance;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized) return;

        rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, targetPos, Time.deltaTime * speed);
    }

    public void Show()
    {
        gameObject.SetActive(true);
        Init();

        rect.anchoredPosition = offScreenPos;
        targetPos = onScreenPos;
        isShown = true;

        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }

    public void Hide()
    {
        Init();

        isShown = false;
        targetPos = offScreenPos;

        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;
    }
}