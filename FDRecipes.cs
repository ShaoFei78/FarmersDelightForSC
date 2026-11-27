using System;
using System.Collections.Generic;
using System.Text.Json;
using Engine;

namespace Game {
    public class FDCraftingEntry {
        public int[][] IngredientOptions;
        public int ResultValue;
        public int ResultCount;
    }

    public class FDSmeltingEntry {
        public int[][] IngredientOptions;
        public int ResultValue;
        public int ResultCount;
    }

    public static class FDRecipes {
        static List<FDCraftingEntry> s_crafting;
        static List<FDSmeltingEntry> s_smelting;
        static bool s_loaded;

        public static void EnsureLoaded() {
            if (s_loaded) {
                return;
            }
            s_loaded = true;
            s_crafting = [];
            s_smelting = [];
            FDCookingRecipes.EnsureLoaded();
            FDCuttingRecipes.EnsureLoaded();
            LoadCrafting();
            LoadCooking();
            LoadCutting();
            LoadSmelting();
            Log.Information(
                $"[FD] Recipes: crafting={s_crafting.Count}, cooking={FDCookingRecipes.Count}, cutting={FDCuttingRecipes.Count}, smelting={s_smelting.Count}"
            );
        }

        static int[] ReadOptions(JsonElement ingredient) {
            List<int> values = [];
            foreach (JsonElement option in ingredient.EnumerateArray()) {
                int value = FDContent.GetValue(option.GetString());
                if (value != 0 && !values.Contains(value)) {
                    values.Add(value);
                }
            }
            return values.Count == 0 ? null : values.ToArray();
        }

        static void LoadCrafting() {
            try {
                JsonDocument document = ContentManager.Get<JsonDocument>("FD/crafting");
                if (document == null) {
                    return;
                }
                foreach (JsonElement element in document.RootElement.EnumerateArray()) {
                    List<int[]> options = [];
                    foreach (JsonElement ingredient in element.GetProperty("ingredients").EnumerateArray()) {
                        int[] values = ReadOptions(ingredient);
                        if (values == null) {
                            options.Clear();
                            break;
                        }
                        options.Add(values);
                    }
                    if (options.Count == 0) {
                        continue;
                    }
                    int result = FDContent.GetValue(element.GetProperty("result").GetString());
                    if (result == 0) {
                        continue;
                    }
                    s_crafting.Add(new FDCraftingEntry {
                        IngredientOptions = options.ToArray(),
                        ResultValue = result,
                        ResultCount = element.GetProperty("count").GetInt32()
                    });
                }
            }
            catch (Exception e) {
                Log.Error("[FD] Failed to load crafting recipes: " + e);
            }
        }

        static void LoadCooking() {
            try {
                JsonDocument document = ContentManager.Get<JsonDocument>("FD/cooking");
                if (document == null) {
                    return;
                }
                List<FDCookingRecipe> recipes = [];
                foreach (JsonElement element in document.RootElement.EnumerateArray()) {
                    List<int[]> options = [];
                    foreach (JsonElement ingredient in element.GetProperty("ingredients").EnumerateArray()) {
                        int[] values = ReadOptions(ingredient);
                        if (values == null) {
                            options.Clear();
                            break;
                        }
                        options.Add(values);
                    }
                    if (options.Count == 0) {
                        continue;
                    }
                    int result = FDContent.GetValue(element.GetProperty("result").GetString());
                    if (result == 0) {
                        continue;
                    }
                    string containerName = element.TryGetProperty("container", out JsonElement container) && container.ValueKind == JsonValueKind.String
                        ? container.GetString()
                        : null;
                    int containerValue = string.IsNullOrEmpty(containerName) ? 0 : FDContent.GetValue(containerName);
                    recipes.Add(new FDCookingRecipe {
                        IngredientOptions = options.ToArray(),
                        ResultValue = result,
                        ResultCount = element.GetProperty("count").GetInt32(),
                        ContainerValue = containerValue,
                        CookTime = element.GetProperty("time").GetInt32(),
                        Experience = 0f
                    });
                }
                FDCookingRecipes.SetRecipes(recipes);
            }
            catch (Exception e) {
                Log.Error("[FD] Failed to load cooking recipes: " + e);
            }
        }

        static void LoadCutting() {
            try {
                JsonDocument document = ContentManager.Get<JsonDocument>("FD/cutting");
                if (document == null) {
                    return;
                }
                List<FDCuttingRecipe> recipes = [];
                foreach (JsonElement element in document.RootElement.EnumerateArray()) {
                    int input = FDContent.GetValue(element.GetProperty("input").GetString());
                    if (input == 0) {
                        continue;
                    }
                    List<int> results = [];
                    List<int> counts = [];
                    foreach (JsonElement result in element.GetProperty("results").EnumerateArray()) {
                        int value = FDContent.GetValue(result.GetProperty("name").GetString());
                        if (value != 0) {
                            results.Add(value);
                            counts.Add(result.GetProperty("count").GetInt32());
                        }
                    }
                    if (results.Count == 0) {
                        continue;
                    }
                    recipes.Add(new FDCuttingRecipe {
                        InputValue = input,
                        ToolLevel = element.TryGetProperty("tool", out JsonElement tool) ? tool.GetInt32() : 0,
                        Results = results.ToArray(),
                        Counts = counts.ToArray()
                    });
                }
                FDCuttingRecipes.SetRecipes(recipes);
            }
            catch (Exception e) {
                Log.Error("[FD] Failed to load cutting recipes: " + e);
            }
        }

        static void LoadSmelting() {
            try {
                JsonDocument document = ContentManager.Get<JsonDocument>("FD/smelting");
                if (document == null) {
                    return;
                }
                foreach (JsonElement element in document.RootElement.EnumerateArray()) {
                    int[] values = ReadOptions(element.GetProperty("ingredients")[0]);
                    if (values == null) {
                        continue;
                    }
                    int result = FDContent.GetValue(element.GetProperty("result").GetString());
                    if (result == 0) {
                        continue;
                    }
                    s_smelting.Add(new FDSmeltingEntry {
                        IngredientOptions = [values],
                        ResultValue = result,
                        ResultCount = element.GetProperty("count").GetInt32()
                    });
                }
            }
            catch (Exception e) {
                Log.Error("[FD] Failed to load smelting recipes: " + e);
            }
        }

        static List<int> GetActual(string[] actualIngredients) {
            List<int> actual = [];
            foreach (string ingredient in actualIngredients) {
                if (string.IsNullOrEmpty(ingredient)) {
                    continue;
                }
                CraftingRecipesManager.DecodeIngredient(ingredient, out string craftingId, out int? data);
                Block[] blocks = BlocksManager.FindBlocksByCraftingId(craftingId);
                if (blocks.Length == 0) {
                    return null;
                }
                actual.Add(Terrain.MakeBlockValue(blocks[0].BlockIndex, 0, data ?? 0));
            }
            return actual;
        }

        public static bool TryCraft(string[] actualIngredients, out CraftingRecipe result) {
            EnsureLoaded();
            result = null;
            List<int> actual = GetActual(actualIngredients);
            if (actual == null || actual.Count == 0) {
                return false;
            }
            foreach (FDCraftingEntry entry in s_crafting) {
                if (entry.IngredientOptions.Length != actual.Count) {
                    continue;
                }
                bool[] used = new bool[actual.Count];
                if (!Match(entry.IngredientOptions, actual, used, 0)) {
                    continue;
                }
                result = new CraftingRecipe {
                    ResultValue = entry.ResultValue,
                    ResultCount = entry.ResultCount,
                    Ingredients = actualIngredients,
                    Description = "FD"
                };
                return true;
            }
            return false;
        }

        public static bool TrySmelt(string[] actualIngredients, out CraftingRecipe result) {
            EnsureLoaded();
            result = null;
            List<int> actual = GetActual(actualIngredients);
            if (actual == null || actual.Count != 1) {
                return false;
            }
            return TrySmeltValue(actual[0], out result, actualIngredients);
        }

        public static bool TrySmeltValue(int value, out CraftingRecipe result, string[] ingredients = null) {
            EnsureLoaded();
            result = null;
            foreach (FDSmeltingEntry entry in s_smelting) {
                if (Array.IndexOf(entry.IngredientOptions[0], value) < 0) {
                    continue;
                }
                result = new CraftingRecipe {
                    ResultValue = entry.ResultValue,
                    ResultCount = entry.ResultCount,
                    Ingredients = ingredients ?? [],
                    Description = "FD smelting"
                };
                return true;
            }
            return false;
        }

        static bool Match(int[][] options, List<int> actual, bool[] used, int index) {
            if (index >= options.Length) {
                return true;
            }
            for (int i = 0; i < actual.Count; i++) {
                if (used[i] || Array.IndexOf(options[index], actual[i]) < 0) {
                    continue;
                }
                used[i] = true;
                if (Match(options, actual, used, index + 1)) {
                    return true;
                }
                used[i] = false;
            }
            return false;
        }
    }

    /// <summary>Hidden block that exposes FD shapeless and smelting recipes.</summary>
    public class FDCraftingProxyBlock : FlatBlock {
        public override void Initialize() {
            base.Initialize();
            DefaultDisplayName = "FD Crafting Proxy";
            DefaultCategory = "Items";
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield break;
        }

        public override int GetTextureSlotCount(int value) => 1;

        public override int GetFaceTextureSlot(int face, int value) => 0;

        public override CraftingRecipe GetAdHocCraftingRecipe(SubsystemTerrain subsystemTerrain,
            string[] ingredients,
            float heatLevel,
            float playerLevel) {
            if (heatLevel >= 1f && FDRecipes.TrySmelt(ingredients, out CraftingRecipe smelted)) {
                return smelted;
            }
            return FDRecipes.TryCraft(ingredients, out CraftingRecipe recipe) ? recipe : null;
        }
    }
}
