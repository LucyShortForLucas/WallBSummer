using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class DangerComponent : MonoBehaviour
{
    [SerializeField] private float baseDanger = 50f; 

    private HealthComponent healthComponent;

    private void Awake()
    {
        healthComponent = GetComponent<HealthComponent>(); // Lucy: Everything that has danger has health?
    }

    public float CurrentDanger
    {
        get
        {
            if (healthComponent == null || healthComponent.MaxHealth <= 0) return 0f;

            float healthRatio = (float)healthComponent.CurrentHealth / healthComponent.MaxHealth;
            return baseDanger * healthRatio;
        }
    }
}