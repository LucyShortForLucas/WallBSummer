using UnityEngine;

[RequireComponent(typeof(Collider))]
public abstract class InteractableComponent : MonoBehaviour, IInteractable
{
    private HighlightComponent highlighter;

    protected virtual void Awake()
    {
        highlighter = GetComponent<HighlightComponent>();
    }

    public void Interact(GameObject interactor)
    {
        ExecuteInteraction(interactor);
    }

    public void SetHighlight(bool active)
    {
        if (highlighter != null)
        {
            highlighter.ToggleHighlight(active);
        }
    }

    protected abstract void ExecuteInteraction(GameObject interactor);
}