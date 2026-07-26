using UnityEngine;

// Put this on the child object that actually holds the ability's CircleCollider2D
public class TriggerRadiusForwarder : MonoBehaviour
{
    private IRadiusTriggerListener listener;

    void Awake()
    {
        listener = GetComponentInParent<IRadiusTriggerListener>();
    }

    private void OnTriggerEnter2D(Collider2D collision) => listener?.OnRadiusTriggerEnter2D(collision);
    private void OnTriggerStay2D(Collider2D collision) => listener?.OnRadiusTriggerStay2D(collision);
    private void OnTriggerExit2D(Collider2D collision) => listener?.OnRadiusTriggerExit2D(collision);
}