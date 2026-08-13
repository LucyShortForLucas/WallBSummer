using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    public int maxHealth = 100;
    public int currentHealth;

    public bool IsDead => currentHealth <= 0;

    public void Initialize(int startHealth)
    {
        maxHealth = startHealth;
        currentHealth = startHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return;

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Destroy(gameObject);
        }
    }
}