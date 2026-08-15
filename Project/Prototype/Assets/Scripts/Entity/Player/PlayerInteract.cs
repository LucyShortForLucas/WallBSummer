using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();

    public void OnInteract(InputValue value)
    {
        if (value.isPressed && nearbyInteractables.Count > 0)
        {
            // Interact recently item
            IInteractable target = nearbyInteractables[nearbyInteractables.Count - 1];
            target.Interact(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && !nearbyInteractables.Contains(interactable))
        {
            nearbyInteractables.Add(interactable);
            UpdateHighlight();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        IInteractable interactable = other.GetComponent<IInteractable>();
        if (interactable != null && nearbyInteractables.Contains(interactable))
        {
            interactable.SetHighlight(false);
            nearbyInteractables.Remove(interactable);
            UpdateHighlight();
        }
    }

    private void Update()
    {
        bool listChanged = false;

        // Clean up destroyed objects
        for (int i = nearbyInteractables.Count - 1; i >= 0; i--)
        {
            if (nearbyInteractables[i] as MonoBehaviour == null)
            {
                nearbyInteractables.RemoveAt(i);
                listChanged = true;
            }
        }

        if (listChanged)
        {
            UpdateHighlight();
        }
    }

    private void UpdateHighlight()
    {
        // Turn off highlights
        foreach (var interactable in nearbyInteractables)
        {
            if (interactable as MonoBehaviour != null)
            {
                interactable.SetHighlight(false);
            }
        }

        // Turn on highlight for recent object
        if (nearbyInteractables.Count > 0)
        {
            IInteractable target = nearbyInteractables[nearbyInteractables.Count - 1];
            target.SetHighlight(true);
        }
    }
}