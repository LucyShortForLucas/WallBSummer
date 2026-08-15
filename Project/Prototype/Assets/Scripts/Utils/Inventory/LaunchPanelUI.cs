using System;
using UnityEngine;
using UnityEngine.UI;

public class LaunchPanelUI : MonoBehaviour
{
    public static LaunchPanelUI Instance { get; private set; }

    [Header("UI Elements")]
    public Button launchButton;

    public static event Action OnLaunchPressed;

    private void Awake()
    {
        Instance = this;

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