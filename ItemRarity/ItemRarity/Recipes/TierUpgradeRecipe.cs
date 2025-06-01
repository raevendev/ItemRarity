using System.Collections.Generic;
using Vintagestory.API.Common;

namespace ItemRarity.Recipes;

public sealed class TierUpgradeRecipe : GridRecipe
{
    public TierUpgradeRecipe(IWorldAccessor worldAccessor)
    {
        Name = "itemrarity:tier-recipe-output";
        IngredientPattern = "T";
        Width = 1;
        Height = 1;
        Shapeless = true;

        Ingredients = new Dictionary<string, CraftingRecipeIngredient>
        {
            {
                "T", new CraftingRecipeIngredient
                {
                    Type = EnumItemClass.Item,
                    Code = "itemrarity:tier-*",
                    IsWildCard = true
                }
            }
        };

        Output = new CraftingRecipeIngredient
        {
            Type = EnumItemClass.Item,
            Code = "itemrarity:tier-output"
        };

        ResolveIngredients(worldAccessor);
    }
}