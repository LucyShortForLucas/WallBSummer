using UnityEngine;
using UnityEngine.UI;

public class CraftPanelUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private Button craftButton;
    [SerializeField] private Image progressFillImage;

    [Header("Reference")]
    [SerializeField] private RecipePanelUI recipePanel;

    private CentralResourceHub resourceHub;
    private InventoryUIManager uiManager;

    private bool isCrafting = false;
    private float craftTimer = 0f;
    private bool isInitialized = false;
    private GlobalRecipeDatabase.RecipeDefinition currentRecipe;

    public void Refresh(InventoryUIManager manager, CentralResourceHub hub)
    {
        uiManager = manager;
        resourceHub = hub;

        if (!isInitialized)
        {
            if (craftButton != null) craftButton.onClick.AddListener(TryStartCrafting);
            if (recipePanel != null) recipePanel.OnRecipeSelected += UpdateCurrentRecipe;
            isInitialized = true;
        }

        isCrafting = false;
        craftTimer = 0f;
        if (progressFillImage != null) progressFillImage.fillAmount = 0f;
        if (craftButton != null) craftButton.interactable = true;
        if (recipePanel != null) recipePanel.SetArrowsInteractable(true);
    }

    private void UpdateCurrentRecipe(GlobalRecipeDatabase.RecipeDefinition recipe)
    {
        currentRecipe = recipe;
        if (!isCrafting && progressFillImage != null) progressFillImage.fillAmount = 0f;
    }

    private void TryStartCrafting()
    {
        if (isCrafting || currentRecipe.id == 0 || uiManager.CurrentFactory == null) return;

        int inputId = uiManager.CurrentFactory.InputStorage.StorageID;

        // Check if enough resources
        foreach (var input in currentRecipe.inputs)
        {
            if (!resourceHub.HasEnough(inputId, input.resourceId, input.amount))
            {
                Debug.Log($"Craft Failed: Missing {input.amount} of Resource ID {input.resourceId}");
                return;
            }
        }

        // Consume resources
        foreach (var input in currentRecipe.inputs)
        {
            resourceHub.ConsumeResource(inputId, input.resourceId, input.amount, true);
        }

        // Start crafting
        isCrafting = true;
        craftTimer = 0f;
        craftButton.interactable = false;

        if (recipePanel != null) recipePanel.SetArrowsInteractable(false);
    }

    private void Update()
    {
        if (!isCrafting) return;

        // Update crafting timer
        craftTimer += Time.deltaTime;

        // Update progress bar
        if (progressFillImage != null)
        {
            progressFillImage.fillAmount = craftTimer / currentRecipe.craftTime;
        }

        if (craftTimer >= currentRecipe.craftTime)
        {
            FinishCrafting();
        }
    }

    private void FinishCrafting()
    {
        // Reset
        isCrafting = false;
        craftTimer = 0f;

        if (progressFillImage != null) progressFillImage.fillAmount = 0f;
        craftButton.interactable = true;
        if (recipePanel != null) recipePanel.SetArrowsInteractable(true);

        // Add crafted resources to output storage
        int outputId = uiManager.CurrentFactory.OutputStorage.StorageID;

        foreach (var output in currentRecipe.outputs)
        {
            resourceHub.AddResource(outputId, output.resourceId, output.amount, true);
        }
    }
}