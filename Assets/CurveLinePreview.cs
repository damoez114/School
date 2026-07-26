using UnityEngine;

public class TroutPathPreview : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;

    [Header("Must match SpriteMover's trout settings")]
    [SerializeField] private float waveAmplitude = 0.5f;
    [SerializeField] private float waveFrequency = 2f;
    [SerializeField] private float speed = 5f;

    [Header("Preview length")]
    [SerializeField] private float previewDuration = 3f; // how many seconds of path to draw
    [SerializeField] private int resolution = 50;         // number of points, higher = smoother


    void Start()
    {
        DrawPath();
    }

    [ContextMenu("Redraw Path")] // lets you preview in editor via right-click too
    public void DrawPath()
    {
        Vector3 origin = Vector3.zero; // local space, since we're now a child of the trout
        Vector3 fixedForward = Vector3.up;   // local up
        Vector3 fixedRight = Vector3.right;  // local right

        lineRenderer.positionCount = resolution;

        for (int i = 0; i < resolution; i++)
        {
            float t = (i / (float)(resolution - 1)) * previewDuration;

            Vector3 forwardPoint = origin + fixedForward * speed * t;
            float offset = Mathf.Sin(t * waveFrequency) * waveAmplitude;
            Vector3 point = forwardPoint + fixedRight * offset;

            lineRenderer.SetPosition(i, point);
        }
    }
}
