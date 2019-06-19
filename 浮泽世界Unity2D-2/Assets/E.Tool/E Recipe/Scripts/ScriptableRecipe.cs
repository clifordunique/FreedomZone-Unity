using UnityEngine;
using System.Collections.Generic;
using System.Linq;

[CreateAssetMenu(fileName="新食谱", menuName="E Recipe", order=0)]
public class ScriptableRecipe : ScriptableObject
{
    public static int recipeSize = 6;
    public List<ScriptableItemAndAmount> ingredients = new List<ScriptableItemAndAmount>(6);
    public ScriptableItem result;
    public float craftingTime = 1;
    [Range(0, 1)] public float probability = 1;

    static Dictionary<string, ScriptableRecipe> cache;
    public static Dictionary<string, ScriptableRecipe> Dict
    {
        get
        {
            // not loaded yet?
            if (cache == null)
            {
                // get all ScriptableRecipes in resources
                ScriptableRecipe[] recipes = Resources.LoadAll<ScriptableRecipe>("");

                // check for duplicates, then add to cache
                List<string> duplicates = recipes.ToList().FindDuplicates(recipe => recipe.name);
                if (duplicates.Count == 0)
                {
                    cache = recipes.ToDictionary(recipe => recipe.name, recipe => recipe);
                }
                else
                {
                    foreach (string duplicate in duplicates)
                        Debug.LogError("Resources文件夹包含多个同名的ScriptableRecipes{" + duplicate + "}，如果您使用的是'Warrior / Ring'和'Archer / Ring'等子文件夹，请将它们重命名为'Warrior /（Warrior）Ring'和'Archer /（Archer）Ring'。");
                }
            }
            return cache;
        }
    }

    // check if the list of items works for this recipe. the list shouldn't
    // contain 'null'.
    // (inheriting classes can modify the matching algorithm if needed)
    public virtual bool CanCraftWith(List<ItemSlot> items)
    {
        // items list should not be touched, since it's often used to check more
        // than one recipe. so let's just create a local copy.
        items = new List<ItemSlot>(items);

        // make sure that we have at least one ingredient
        if (ingredients.Any(slot => slot.Amount > 0 && slot.Item != null))
        {
            // each ingredient in the list, with amount?
            foreach (ScriptableItemAndAmount ingredient in ingredients)
            {
                if (ingredient.Amount > 0 && ingredient.Item != null)
                {
                    // is there a stack with at least that amount and that item?
                    int index = items.FindIndex(slot => slot.amount >= ingredient.Amount && slot.item.Data == ingredient.Item);
                    if (index != -1)
                        items.RemoveAt(index);
                    else
                        return false;
                }
            }

            // and nothing else in the list?
            return items.Count == 0;
        }
        else return false;
    }

    void OnValidate()
    {
        // force list size
        // -> add if too few
        for (int i = ingredients.Count; i < recipeSize; ++i)
            ingredients.Add(new ScriptableItemAndAmount());

        // -> remove if too many
        for (int i = recipeSize; i < ingredients.Count; ++i)
            ingredients.RemoveAt(i);
    }
}
