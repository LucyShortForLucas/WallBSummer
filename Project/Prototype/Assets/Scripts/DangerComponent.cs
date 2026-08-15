using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class DangerComponent : MonoBehaviour
{
    public float baseDanger = 50f; // VIOLATES Cs.S.1 - Avoid declaring public fields in a class

    private HealthComponent healthComponent;

    private void Awake()
    {
        healthComponent = GetComponent<HealthComponent>(); // Lucy: Everything that has danger has health?
    }

    public float CurrentDanger
    {
        get
        {
            if (healthComponent == null || healthComponent.maxHealth <= 0) return 0f;

            float healthRatio = (float)healthComponent.currentHealth / healthComponent.maxHealth;
            return baseDanger * healthRatio;
        }
    }
}