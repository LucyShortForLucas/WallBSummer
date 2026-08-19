using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipePanelUI : MonoBehaviour
{
    [Header("UI Controls")]
    [SerializeField] private Button leftArrowBtn;
    [SerializeField] private Button rightArrowBtn;

    [Header("Recipe Display")]
    [SerializeField] private Image outputIconImage;
    [SerializeField] private TMP_Text recipeNameText;

    [Header("Requirements")]
    [SerializeField] private Transform rowContainer;
    [SerializeField] private GameObject rowPrefab;

    public event Action<GlobalRecipeDatabase.RecipeDefinition> OnRecipeSelected;

    private List<GameObject> activeRows = new List<GameObject>();
    private List<GlobalRecipeDatabase.RecipeDefinition> availableRecipes = new List<GlobalRecipeDatabase.RecipeDefinition>();

    private int currentIndex = 0;
    private bool isInitialized = false;

    private InventoryUIManager uiManager;
    private GlobalRecipeDatabase recipeDatabase;

    public void Refresh(InventoryUIManager manager, GlobalRecipeDatabase database)
    {
        uiManager = manager;
        recipeDatabase = database;

        if (!isInitialized)
        {
            if (leftArrowBtn != null) leftArrowBtn.onClick.AddListener(PrevRecipe);
            if (rightArrowBtn != null) rightArrowBtn.onClick.AddListener(NextRecipe);
            isInitialized = true;
        }

        LoadRecipesFromFactory();
    }

    private void LoadRecipesFromFactory()
    {
        if (uiManager == null || uiManager.CurrentFactory == null || recipeDatabase == null) return;

        availableRecipes.Clear();

        // Load allowed recipes
        foreach (int id in uiManager.CurrentFactory.AllowedRecipeIds)
        {
            var recipe = recipeDatabase.GetRecipe(id);
            if (recipe.id != 0) availableRecipes.Add(recipe);
        }

        currentIndex = 0;
        DisplayCurrentRecipe();
    }

    private void NextRecipe()
    {
        if (availableRecipes.Count == 0) return;

        currentIndex++;
        if (currentIndex >= availableRecipes.Count) currentIndex = 0; 

        DisplayCurrentRecipe();
    }

    private void PrevRecipe()
    {
        if (availableRecipes.Count == 0) return;

        currentIndex--;
        if (currentIndex < 0) currentIndex = availableRecipes.Count - 1; 

        DisplayCurrentRecipe();
    }

    private void DisplayCurrentRecipe()
    {
        // Clear old rows
        foreach (var row in activeRows) Destroy(row);
        activeRows.Clear();

        if (availableRecipes.Count == 0) return;

        // Get current recipe and set Name/Icon
        var recipe = availableRecipes[currentIndex];

        if (recipeNameText != null) recipeNameText.text = recipe.recipeName;

        if (recipe.outputs.Count > 0 && uiManager.ResourceDatabase != null)
        {
            var outRes = uiManager.ResourceDatabase.GetResource(recipe.outputs[0].resourceId);
            if (outputIconImage != null) outputIconImage.sprite = outRes.icon;
        }

        // Spawn rows for recipe
        foreach (var input in recipe.inputs)
        {
            var inRes = uiManager.ResourceDatabase.GetResource(input.resourceId);

            GameObject newRow = Instantiate(rowPrefab, rowContainer);
            newRow.SetActive(true);
            activeRows.Add(newRow);

            var rowScript = newRow.GetComponent<FactoryRecipeRowUI>();
            if (rowScript != null)
            {
                rowScript.Setup(inRes, input.amount);
            }
        }

        // Tell Craft panel recipe changed
        OnRecipeSelected?.Invoke(recipe);
    }

    public void SetArrowsInteractable(bool interactable)
    {
        if (leftArrowBtn != null) leftArrowBtn.interactable = interactable;
        if (rightArrowBtn != null) rightArrowBtn.interactable = interactable;
    }
}