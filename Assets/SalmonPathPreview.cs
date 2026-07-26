using UnityEngine;

public class SalmonPathPreview : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private SpriteMover salmonMover; // drag the salmon's SpriteMover here

    [Header("Preview settings")]
    [SerializeField] private float previewLength = 10f;
    [SerializeField] private int resolution = 100;

    void Start()
    {
        DrawPath();
    }

    [ContextMenu("Redraw Path")]
    public void DrawPath()
    {
        if (lineRenderer == null || salmonMover == null) return;

        float startRadius = salmonMover.SpiralStartRadius;
        float growthRate = salmonMover.SpiralGrowthRate;

        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        Vector3 scaleCorrection = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f
        );

        Vector3 fixedForward = Vector3.up;
        Vector3 fixedRight = Vector3.right;
        Vector3 center = Vector3.zero;

        lineRenderer.positionCount = resolution;

        float theta = 0.01f;
        float accumulatedLength = 0f;
        float stepTheta = 0.01f; // fine step for the integration — smaller = more accurate curve

        for (int i = 0; i < resolution; i++)
        {
            float targetLength = (i / (float)(resolution - 1)) * previewLength;

            // Integrate forward (same rule as MoveInSpiral: ds = r * dtheta) until we reach this point's target distance
            while (accumulatedLength < targetLength)
            {
                float r = startRadius + growthRate * theta;
                accumulatedLength += r * stepTheta;
                theta += stepTheta;
            }

            float finalR = startRadius + growthRate * theta;
            Vector3 offsetDir = fixedRight * Mathf.Cos(theta) + fixedForward * Mathf.Sin(theta);
            Vector3 point = center + offsetDir * finalR;

            point = Vector3.Scale(point, scaleCorrection);
            lineRenderer.SetPosition(i, point);
        }
    }
}