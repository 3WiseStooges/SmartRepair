using System.Collections.Generic;

namespace SmartRepair
{
    /// <summary>
    /// ObjectDB.GetRecipe walks the whole recipe list comparing display names, and the repair
    /// UI asks once per worn item per frame. Widening the rules would otherwise double an
    /// already hot scan, so keep the answer.
    /// </summary>
    internal static class RecipeCache
    {
        private static readonly Dictionary<string, Recipe> ByItemName = new Dictionary<string, Recipe>();

        private static ObjectDB _builtFor;
        private static int _builtForRecipeCount = -1;

        internal static Recipe GetRecipe(ItemDrop.ItemData item)
        {
            ObjectDB db = ObjectDB.instance;
            if (db == null || item == null || item.m_shared == null) return null;

            // Recipes are appended to the live ObjectDB by mods such as Jotunn, so a stale
            // miss has to be able to expire.
            int recipeCount = db.m_recipes == null ? 0 : db.m_recipes.Count;
            if (!ReferenceEquals(db, _builtFor) || recipeCount != _builtForRecipeCount)
            {
                ByItemName.Clear();
                _builtFor = db;
                _builtForRecipeCount = recipeCount;
            }

            string name = item.m_shared.m_name;
            if (string.IsNullOrEmpty(name)) return null;

            if (ByItemName.TryGetValue(name, out Recipe cached)) return cached;

            Recipe recipe = db.GetRecipe(item);
            ByItemName[name] = recipe;
            return recipe;
        }
    }
}
