using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using E.Utility;

namespace E.Tool
{
    [CreateAssetMenu(fileName = "新食谱", menuName = "E Recipe", order = 0)]
    public class ScriptableRecipe : ScriptableObjectDictionary<ScriptableRecipe>
    {
        public static int recipeSize = 6;
        public List<ScriptableItemAndAmount> ingredients = new List<ScriptableItemAndAmount>(6);
        public ScriptableItem result;
        public float craftingTime = 1;
        [Range(0, 1)] public float probability = 1;

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
}