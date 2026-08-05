using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class DangerComponent : MonoBehaviour
{
    public float baseDanger = 50f;

    private HealthComponent healthComponent;

    private void Awake()
    {
        healthComponent = GetComponent<HealthComponent>();
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