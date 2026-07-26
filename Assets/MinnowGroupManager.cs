using UnityEngine;

public class MinnowGroupManager : MonoBehaviour
{
    void Update()
    {
        // if no children remain, destroy this group object (and its collider)
        if (transform.childCount == 2)
        {
            Destroy(gameObject);
        }
    }
}
