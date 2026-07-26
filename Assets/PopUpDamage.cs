using TMPro;
using UnityEngine;

public class PopUpDamage : MonoBehaviour
{
    [SerializeField] private float speed = 2.5f;
    [SerializeField] private float lifetime = 1.5f;
    [SerializeField] private float fadeTime = 0.7f;

    private TextMeshProUGUI fadeAwayText;
    private Vector2 direction;

    void Start()
    {
        fadeAwayText = GetComponentInChildren<TextMeshProUGUI>();

        direction = GetDirection();

        Destroy(gameObject, lifetime);
    }

    void Update()
    {
        // Move manually via transform instead of Rigidbody2D
        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        if (fadeTime > 0)
        {
            fadeTime -= Time.deltaTime;

            fadeAwayText.color = new Color(
                fadeAwayText.color.r,
                fadeAwayText.color.g,
                fadeAwayText.color.b,
                fadeTime
            );
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private Vector2 GetDirection()
    {
        return new Vector2(1f, 1f).normalized;
    }
}