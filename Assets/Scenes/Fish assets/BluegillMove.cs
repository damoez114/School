using UnityEngine;
using UnityEngine.EventSystems;

public class FishDragManager : MonoBehaviour
{
    public static FishDragManager Instance;

    private DraggableFish selectedFish;
    private FishRotateHandle selectedHandle;
    private Camera cam;

    private Vector2 mouseDownPos;
    private bool isDragging;
    private bool isRotating;
    private float previousAngle;

    [SerializeField] private float dragThreshold = 0.15f;

    private void Awake()
    {
        Instance = this;
        cam = Camera.main;
    }

    private void Update()
    {
        if (PauseManager.isPaused) return;
        if (cam == null) return;

        Vector2 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);

        // MOUSE DOWN
        if (Input.GetMouseButtonDown(0))
        {
            selectedFish = null;
            selectedHandle = null;
            isDragging = false;
            isRotating = false;

            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            mouseDownPos = mousePos;

            // check rotate handles FIRST, since they're only active/relevant when arrows are on
            selectedHandle = GetHandleUnderMouse(mousePos);

            if (selectedHandle != null)
            {
                isRotating = true;
                previousAngle = selectedHandle.GetMouseAngle(mousePos);
            }
            else
            {
                selectedFish = GetTopFishUnderMouse(mousePos);
            }
        }

        // MOUSE HOLD
        if (Input.GetMouseButton(0))
        {
            if (isRotating && selectedHandle != null)
            {
                float currentAngle = selectedHandle.GetMouseAngle(mousePos);
                float deltaAngle = Mathf.DeltaAngle(previousAngle, currentAngle);
                selectedHandle.Rotate(deltaAngle);
                previousAngle = currentAngle;
                return;
            }

            if (selectedFish == null) return;

            if (!isDragging && Vector2.Distance(mouseDownPos, mousePos) > dragThreshold)
            {
                isDragging = true;
                selectedFish.StartDrag(mousePos);
            }

            if (isDragging)
            {
                selectedFish.Drag(mousePos);
            }
        }

        // MOUSE UP
        if (Input.GetMouseButtonUp(0))
        {
            if (isRotating)
            {
                isRotating = false;
                selectedHandle = null;
                return;
            }

            if (selectedFish == null) return;

            if (!isDragging)
            {
                selectedFish.ToggleArrows();
            }
            else
            {
                selectedFish.EndDrag();
            }

            selectedFish = null;
            isDragging = false;
        }
    }

    private DraggableFish GetTopFishUnderMouse(Vector2 mousePos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        DraggableFish bestFish = null;
        float bestZ = float.MaxValue;

        foreach (var hit in hits)
        {
            DraggableFish fish = hit.collider.GetComponent<DraggableFish>();

            if (fish != null)
            {
                float z = fish.transform.position.z;

                if (z < bestZ)
                {
                    bestZ = z;
                    bestFish = fish;
                }
            }
        }

        return bestFish;
    }

    private FishRotateHandle GetHandleUnderMouse(Vector2 mousePos)
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(mousePos, Vector2.zero);

        foreach (var hit in hits)
        {
            FishRotateHandle handle = hit.collider.GetComponent<FishRotateHandle>();
            if (handle != null)
                return handle;
        }

        return null;
    }
}