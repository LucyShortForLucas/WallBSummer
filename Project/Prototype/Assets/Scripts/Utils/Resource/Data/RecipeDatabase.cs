using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

[CreateAssetMenu(fileName = "New Recipe Database", menuName = "Database/Recipe Database")]
public class GlobalRecipeDatabase : ScriptableObject
{
    [Serializable]
    public struct RecipeIngredient
    {
        public int resourceId;
        public int amount;
    }

    [Serializable]
    public struct RecipeDefinition
    {
        public int id;
        public string recipeName;
        public float craftTime;
        public List<RecipeIngredient> inputs;
        public List<RecipeIngredient> outputs;
    }

    [Header("Recipes")]
    public List<RecipeDefinition> recipes = new List<RecipeDefinition>();

    // Dictionary for looking up recipes by ID 
    private Dictionary<int, RecipeDefinition> recipeDict = new Dictionary<int, RecipeDefinition>();

    public void Initialize()
    {
        recipeDict.Clear();

        // Put all recipes in other dictionary
        foreach (var recipe in recipes)
        {
            if (!recipeDict.ContainsKey(recipe.id))
            {
                recipeDict.Add(recipe.id, recipe);
            }
            else
            {
                Debug.LogError($"Duplicate Recipe ID found in Database: {recipe.id}");
            }
        }
    }

    public RecipeDefinition GetRecipe(int id)
    {
        if (recipeDict.TryGetValue(id, out RecipeDefinition recipe))
        {
            return recipe;
        }
        return default;
    }
}