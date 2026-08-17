using System;
using UnityEngine;
using UnityEngine.UI;

public class LaunchPanelUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button launchButton;

    public event Action OnLaunchPressed;

    private void Awake()
    {
        if (launchButton != null)
        {
            launchButton.onClick.AddListener(() => OnLaunchPressed?.Invoke());
        }
    }

    public void SetLaunchInteractable(bool isInteractable)
    {
        if (launchButton != null)
        {
            launchButton.interactable = isInteractable;
        }
    }
}