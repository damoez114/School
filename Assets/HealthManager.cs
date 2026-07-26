using UnityEngine;
using UnityEngine.UI;

public class HealthManager : MonoBehaviour
{
    public Image healthBar;
    public float maxHealth = 100f;
    public float healthAmount;

    private void Awake()
    {
        healthAmount = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(float damage)
    {
        healthAmount -= damage;
        healthAmount = Mathf.Clamp(healthAmount, 0f, maxHealth);

        UpdateHealthBar();
    }

    // Lets enemies like Kelp set their total health at spawn time based on segment count
    public void SetMaxHealth(float newMax)
    {
        maxHealth = newMax;
        healthAmount = newMax;
        UpdateHealthBar();
    }

    private void UpdateHealthBar()
    {
        healthBar.fillAmount = healthAmount / maxHealth;
    }
}