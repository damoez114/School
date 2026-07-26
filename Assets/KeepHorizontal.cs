using UnityEngine;

public class KeepHorizontal : MonoBehaviour
{
    [SerializeField] private Transform target;       // the strider (assign in Inspector, or auto-grab parent)
    [SerializeField] private Vector3 offset = new Vector3(0f, -0.5f, 0f); // fixed offset, e.g. "underneath"

    void Awake()
    {
        if (target == null)
            target = transform.parent;
    }

    void LateUpdate()
    {
        transform.position = target.position + offset;
        transform.rotation = Quaternion.identity;
    }
}