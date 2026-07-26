using UnityEngine;
using System.Collections;

public class FloatOutSpawn : MonoBehaviour
{
    private bool isFloating = false;

    // disable normal enemy behavior scripts while floating, then re-enable
    [SerializeField] private MonoBehaviour[] scriptsToDisableWhileFloating;

    public void Init(Vector2 direction, float distance, float duration)
    {
        StartCoroutine(FloatRoutine(direction, distance, duration));
    }

    private IEnumerator FloatRoutine(Vector2 direction, float distance, float duration)
    {
        isFloating = true;

        foreach (var script in scriptsToDisableWhileFloating)
        {
            if (script != null) script.enabled = false;
        }

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos + (Vector3)(direction.normalized * distance);

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float eased = 1f - Mathf.Pow(1f - t, 2f); // ease-out, feels more "floaty"

            transform.position = Vector3.Lerp(startPos, endPos, eased);
            yield return null;
        }

        transform.position = endPos;

        foreach (var script in scriptsToDisableWhileFloating)
        {
            if (script != null) script.enabled = true;
        }

        isFloating = false;
    }
}