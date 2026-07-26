using UnityEngine;

public class CatfishPathPreview : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private SpriteMover catfishMover; // drag the catfish's SpriteMover here

    [Header("Preview settings")]
    [SerializeField] private float previewLength = 10f; // total distance along the path to draw
    [SerializeField] private int resolution = 100;

    void Start()
    {
        DrawPath();
    }

    [ContextMenu("Redraw Path")]
    public void DrawPath()
    {
        if (lineRenderer == null || catfishMover == null) return;

        float uLegLength = catfishMover.ULegLength;
        float uRadius = catfishMover.URadius;

        // counteract parent scale so points aren't stretched
        Vector3 parentScale = transform.parent != null ? transform.parent.lossyScale : Vector3.one;
        Vector3 scaleCorrection = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f
        );

        Vector3 fixedForward = Vector3.up;
        Vector3 fixedRight = Vector3.right;
        Vector3 startPos = Vector3.zero;

        float arcLength = Mathf.PI * uRadius;
        float totalPathLength = uLegLength + arcLength + uLegLength;
        float lengthToShow = Mathf.Min(previewLength, totalPathLength);

        lineRenderer.positionCount = resolution;

        for (int i = 0; i < resolution; i++)
        {
            float s = (i / (float)(resolution - 1)) * lengthToShow;
            Vector3 point;

            if (s < uLegLength)
            {
                point = startPos + fixedForward * s;
            }
            else if (s < uLegLength + arcLength)
            {
                float arcProgress = (s - uLegLength) / arcLength;
                float theta = arcProgress * Mathf.PI;

                Vector3 arcCenter = startPos + fixedForward * uLegLength + fixedRight * uRadius;
                Vector3 offsetDir = -fixedRight * Mathf.Cos(theta) + fixedForward * Mathf.Sin(theta);
                point = arcCenter + offsetDir * uRadius;
            }
            else
            {
                float legProgress = s - uLegLength - arcLength;
                Vector3 secondLegStart = startPos + fixedForward * uLegLength + fixedRight * (2f * uRadius);
                point = secondLegStart - fixedForward * legProgress;
            }

            // scale-correct each point before assigning
            point = Vector3.Scale(point, scaleCorrection);

            lineRenderer.SetPosition(i, point);
        }
    }
}