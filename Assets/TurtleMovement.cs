using UnityEngine;

public class TurtleMovement : MonoBehaviour
{
    [Header("Ellipse settings")]
    [SerializeField] private float radiusX = 3f;
    [SerializeField] private float radiusY = 1.5f;
    [SerializeField] private float speed = 1f; // radians per second

    [Header("Rotation")]
    [SerializeField] private bool rotateToFaceDirection = true;
    [SerializeField] private float rotationOffset = 0f; // adjust if the sprite's "forward" isn't pointing right by default

    private Vector2 center;
    private float angle;
    private int direction = 1;

    public void Init(Vector2 orbitCenter)
    {
        center = orbitCenter;
        angle = Random.Range(0f, Mathf.PI * 2f);
        direction = Random.value < 0.5f ? 1 : -1;

        UpdatePosition();
    }

    private void Update()
    {
        angle += speed * direction * Time.deltaTime;
        UpdatePosition();
    }

    private void UpdatePosition()
    {
        float x = center.x + Mathf.Cos(angle) * radiusX;
        float y = center.y + Mathf.Sin(angle) * radiusY;
        transform.position = new Vector2(x, y);

        if (rotateToFaceDirection)
        {
            // Tangent to the ellipse at this angle = derivative of the position
            // w.r.t. angle, scaled by direction (so it flips correctly depending
            // on clockwise vs counter-clockwise travel)
            float tangentX = -Mathf.Sin(angle) * radiusX * direction;
            float tangentY = Mathf.Cos(angle) * radiusY * direction;

            float facingAngle = Mathf.Atan2(tangentY, tangentX) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, facingAngle + rotationOffset);
        }
    }
}