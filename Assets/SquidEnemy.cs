using System.Collections;
using UnityEngine;

public class SquidEnemy : MonoBehaviour
{
    [SerializeField] private SpriteRenderer[] renderers; // leave empty to auto-grab in Awake
    [SerializeField] private GameObject healthBarObject;  // the child GameObject holding the health bar
    [SerializeField] private float fadeDuration = 0.6f;

    private bool isInvisible = false;
    private CanvasGroup healthBarCanvasGroup;
    private SpriteRenderer healthBarSpriteRenderer;

    private void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<SpriteRenderer>();

        if (healthBarObject != null)
        {
            healthBarCanvasGroup = healthBarObject.GetComponent<CanvasGroup>();
            healthBarSpriteRenderer = healthBarObject.GetComponent<SpriteRenderer>();
        }
    }

    public void GoInvisible()
    {
        if (isInvisible) return;
        isInvisible = true;

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float elapsed = 0f;

        // cache starting colors so we don't stomp any tinting already on the sprites
        Color[] startColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] != null)
                startColors[i] = renderers[i].color;
        }

        float healthBarStartAlpha = 1f;
        if (healthBarCanvasGroup != null)
            healthBarStartAlpha = healthBarCanvasGroup.alpha;
        else if (healthBarSpriteRenderer != null)
            healthBarStartAlpha = healthBarSpriteRenderer.color.a;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeDuration;

            for (int i = 0; i < renderers.Length; i++)
            {
                if (renderers[i] == null) continue;
                Color c = startColors[i];
                c.a = Mathf.Lerp(startColors[i].a, 0f, t);
                renderers[i].color = c;
            }

            if (healthBarCanvasGroup != null)
            {
                healthBarCanvasGroup.alpha = Mathf.Lerp(healthBarStartAlpha, 0f, t);
            }
            else if (healthBarSpriteRenderer != null)
            {
                Color c = healthBarSpriteRenderer.color;
                c.a = Mathf.Lerp(healthBarStartAlpha, 0f, t);
                healthBarSpriteRenderer.color = c;
            }

            yield return null;
        }

        // snap fully off at the end, and disable so it stops taking draw calls / raycasts
        foreach (var r in renderers)
        {
            if (r != null) r.enabled = false;
        }

        if (healthBarObject != null)
            healthBarObject.SetActive(false);
    }
}