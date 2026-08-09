using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class ResourceUIUpdater : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ResourceStorage targetStorage;
    [SerializeField] private ResourceData targetResource;

    [Header("UI Elements")]
    [SerializeField] private TextMeshProUGUI amountText;
    [SerializeField] private Image resourceIcon;

    [Header("Animation Settings")]
    [SerializeField] private float animationDuration = 0.5f;

    private float currentDisplayAmount;
    private int targetAmount;
    private int currentMaxAmount;
    private Coroutine countingCoroutine;

    private void Start()
    {
        // Set icon if both references exist
        if (resourceIcon != null && targetResource != null)
        {
            resourceIcon.sprite = targetResource.icon;
        }

        targetAmount = targetStorage.GetAmount(targetResource);
        currentMaxAmount = targetStorage.GetMaxAmount(targetResource);
        currentDisplayAmount = targetAmount;

        UpdateText(Mathf.RoundToInt(currentDisplayAmount), currentMaxAmount);
    }

    private void OnEnable()
    {
        targetStorage.OnResourceChanged += HandleResourceChanged;
    }

    private void OnDisable()
    {
        targetStorage.OnResourceChanged -= HandleResourceChanged;
    }

    private void HandleResourceChanged(ResourceData resource, int newAmount, int maxAmount)
    {
        // Update targets and restart animation if matching resource
        if (resource == targetResource)
        {
            targetAmount = newAmount;
            currentMaxAmount = maxAmount;

            if (countingCoroutine != null)
            {
                StopCoroutine(countingCoroutine);
            }
            countingCoroutine = StartCoroutine(CountToTarget());
        }
    }

    private IEnumerator CountToTarget()
    {
        float startAmount = currentDisplayAmount;
        float elapsed = 0f;

        // Smoothly displayed amount over duration
        while (elapsed < animationDuration)
        {
            elapsed += Time.deltaTime;
            currentDisplayAmount = Mathf.Lerp(startAmount, targetAmount, elapsed / animationDuration);
            UpdateText(Mathf.RoundToInt(currentDisplayAmount), currentMaxAmount);

            yield return null;
        }

        // Snap to final value
        currentDisplayAmount = targetAmount;
        UpdateText(targetAmount, currentMaxAmount);
    }

    private void UpdateText(int current, int max)
    {
        if (amountText != null)
        {
            amountText.text = $"{current}/{max}";
        }
    }
}