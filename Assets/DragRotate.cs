using UnityEngine;

public class FishRotateHandle : MonoBehaviour
{
    public Transform objectToRotate; // the fish this arrow belongs to

    public float GetMouseAngle(Vector2 mousePos)
    {
        Vector2 direction = mousePos - (Vector2)objectToRotate.position;
        return Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

    public void Rotate(float deltaAngle)
    {
        objectToRotate.Rotate(0, 0, deltaAngle);
    }
}