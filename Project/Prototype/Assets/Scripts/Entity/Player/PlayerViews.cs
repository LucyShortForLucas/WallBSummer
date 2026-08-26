using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PlayerViews : MonoBehaviour
{
    [Header("Post Processing")]
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float transitionSpeed = 150f;

    public static event Action<bool> OnViewToggled;

    private ColorAdjustments colorAdjustments;
    private bool isWaterViewActive = false;
    private float targetSaturation = 0f;

    private void Start()
    {
        // Get the color adjustments
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out colorAdjustments);
        }
    }

    public void OnToggleView(InputValue value)
    {
        if (!value.isPressed) return;

        isWaterViewActive = !isWaterViewActive;
        targetSaturation = isWaterViewActive ? -50f : 0f;

        OnViewToggled?.Invoke(isWaterViewActive);
    }

    public void ForceWaterView(bool forceActive)
    {
        if (isWaterViewActive == forceActive) return;

        isWaterViewActive = forceActive;
        targetSaturation = isWaterViewActive ? -50f : 0f;

        OnViewToggled?.Invoke(isWaterViewActive);
    }

    private void Update()
    {
        // Smoothly animate 
        if (colorAdjustments != null)
        {
            float currentSat = colorAdjustments.saturation.value;

            if (!Mathf.Approximately(currentSat, targetSaturation))
            {
                colorAdjustments.saturation.value = Mathf.MoveTowards(currentSat, targetSaturation, transitionSpeed * Time.deltaTime);
            }
        }
    }
}