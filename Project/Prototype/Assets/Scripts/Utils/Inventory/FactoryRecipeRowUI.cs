using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FactoryRecipeRowUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text amountText;

    public void Setup(GlobalResourceDatabase.ResourceDefinition resDef, int requiredAmount)
    {
        if (iconImage != null) iconImage.sprite = resDef.icon;
        if (nameText != null) nameText.text = resDef.resourceName;
        if (amountText != null) amountText.text = requiredAmount.ToString();
    }
}