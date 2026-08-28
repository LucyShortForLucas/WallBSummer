using UnityEngine;

[RequireComponent(typeof(HealthComponent))]
public class DangerComponent : MonoBehaviour // CONSIDER: Every monobehaviour is a Component. Putting Component in the name is unnecessary. 
{                                            // If 'Danger' alone is too vague of a component name, perhaps name it something more specific
                                             // like 'DangerValue' to denote the purpose of this component is simply to give a game object a 
                                             // danger value. Ditto for health. -Lucy
    [SerializeField] private float baseDanger = 50f; // NAME VIOLATION: Should start with Underscore. -Lucy

    private HealthComponent healthComponent;

    private void Awake()
    {
        healthComponent = GetComponent<HealthComponent>(); // CONSIDER: Everything that has danger has health? -Lucy
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