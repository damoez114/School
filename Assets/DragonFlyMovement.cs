using System.Collections;
using UnityEngine;

public class DragonflyEnemy : MonoBehaviour
{
    [Header("Zip settings")]
    [SerializeField] private float zipSpeed = 12f;
    [SerializeField] private float minZipDistance = 1.5f;
    [SerializeField] private float maxZipDistance = 4f;
    [SerializeField] private float pauseDuration = 0.4f;

    private Bounds bounds;

    public void Init(Bounds spawnBounds)
    {
        bounds = spawnBounds;
        StartCoroutine(ZipSequence());
    }

    private IEnumerator ZipSequence()
    {
        while (true)
        {
            Vector3 destination = PickZipTarget();

            yield return ZipTo(destination);

            yield return new WaitForSeconds(pauseDuration);
        }
    }

    private Vector3 PickZipTarget()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        float distance = Random.Range(minZipDistance, maxZipDistance);

        Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * distance;
        Vector3 target = transform.position + offset;

        target.x = Mathf.Clamp(target.x, bounds.min.x, bounds.max.x);
        target.y = Mathf.Clamp(target.y, bounds.min.y, bounds.max.y);

        return target;
    }

    private IEnumerator ZipTo(Vector3 destination)
    {
        Vector3 dir = (destination - transform.position).normalized;
        if (dir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        while (Vector3.Distance(transform.position, destination) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destination, zipSpeed * Time.deltaTime);
            yield return null;
        }

        transform.position = destination;
    }
}