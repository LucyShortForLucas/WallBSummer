using UnityEngine;

public class HighlightComponent : MonoBehaviour
{
    [SerializeField] private Renderer targetRenderer;
    [SerializeField] private Color highlightColor = Color.yellow;

    private Color originalColor;

    private void Start()
    {
        if (targetRenderer != null)
        {
            originalColor = targetRenderer.material.color;
        }
    }

    public void ToggleHighlight(bool active)
    {
        if (targetRenderer != null)
        {
            targetRenderer.material.color = active ? highlightColor : originalColor;
        }
    }
}