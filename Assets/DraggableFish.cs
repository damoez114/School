using UnityEngine;

public class DraggableFish : MonoBehaviour
{
    private Vector3 offset;
    private Rigidbody2D rb;

    public GameObject[] arrowObjects;
    public GameObject[] extraToggleObjects; // e.g. pufferfish radius line — toggles alongside arrows
    private bool arrowsOn = false;
    private bool isDragging;
    public bool IsDragging => isDragging;

    // Set true once the play button starts fish movement — blocks any
    // further dragging until reset (e.g. at the start of the next round).
    public static bool MovementStarted { get; private set; }

    public static void SetMovementStarted(bool started)
    {
        MovementStarted = started;
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetArrows(false);
    }

    public void StartDrag(Vector2 mousePos)
    {
        if (MovementStarted)
            return;

        isDragging = true;
        offset = transform.position - (Vector3)mousePos;
        offset = transform.position - (Vector3)mousePos;

        if (!arrowsOn)
            SetArrows(true);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }
    }

    public void Drag(Vector2 mousePos)
    {
        if (MovementStarted)
            return;

        transform.position = (Vector3)mousePos + offset;
    }

    public void EndDrag()
    {
        isDragging = false;
        if (rb != null)
            rb.gravityScale = 1;
    }

    public void ToggleArrows()
    {
        if (MovementStarted)
            return;

        SetArrows(!arrowsOn);
    }

    private void SetArrows(bool state)
    {
        arrowsOn = state;

        if (arrowObjects != null)
        {
            foreach (var a in arrowObjects)
                if (a != null) a.SetActive(state);
        }

        if (extraToggleObjects != null)
        {
            foreach (var e in extraToggleObjects)
                if (e != null) e.SetActive(state);
        }
    }
}