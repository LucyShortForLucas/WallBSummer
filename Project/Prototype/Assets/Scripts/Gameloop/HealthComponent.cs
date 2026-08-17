using UnityEngine;

public class HealthComponent : MonoBehaviour, IDamageable
{
    [SerializeField] private int maxHealth = 100; 
    [SerializeField] private int currentHealth;   

    public bool IsDead => currentHealth <= 0; // VIOLATES Cs.S.1 - Avoid declaring public fields in a class
                                              // + VIOLATES Cs.S.2 - Avoid auto-implemented properties.

    // Getters and Setters
    public int MaxHealth
    {
        get => maxHealth;
        set => maxHealth = value;
    }
    public int CurrentHealth
    {
        get => currentHealth;
        set => currentHealth = value;
    }

    public void Initialize(int startHealth)
    {
        maxHealth = startHealth;
        currentHealth = startHealth;
    }

    public void TakeDamage(int amount)
    {
        if (IsDead) return; // Lucy: Skipping if healt < 0, but below we destroy the gameobject the moment health falls below 0, so this can never cause an early return

        currentHealth -= amount;

        if (currentHealth <= 0)
        {
            currentHealth = 0;
            Destroy(gameObject); // Lucy: Should the entire GameObject ALWAYS be destroyed the moment we hit 0? 
        }
    }
}