using UnityEngine;

public class FishOffscreenDisable : MonoBehaviour
{
    private Camera cam;

    [SerializeField] private float padding = 0.1f;

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) return;

        Vector3 viewPos = cam.WorldToViewportPoint(transform.position);

        bool offscreen =
            viewPos.x < -padding || viewPos.x > 1 + padding ||
            viewPos.y < -padding || viewPos.y > 1 + padding;

        if (offscreen)
        {
            Destroy(gameObject);
            Roe.fishCount--;
            Debug.Log("Fish Remaining "+Roe.fishCount);
        }
    }
}
