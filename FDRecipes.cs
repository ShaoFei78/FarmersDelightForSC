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

        static string IngredientName(int value) {
            int contents = Terrain.ExtractContents(value);
            if (contents <= 0 || contents >= BlocksManager.Blocks.Length) {
                return string.Empty;
            }
            Block block = BlocksManager.Blocks[contents];
            string id = block.GetCraftingId(value);
            if (string.IsNullOrEmpty(id)) {
                id = block.GetType().Name;
            }
            int data = Terrain.ExtractData(value);
            return data == 0 ? id : $"{id}:{data}";
        }

        static string[] Shape(int[][] options) {
            string[] ingredients = new string[9];
            for (int i = 0; i < options.Length && i < 9; i++) {
                int[] values = options[i];
                if (values != null && values.Length > 0 && values[0] != 0) {
                    ingredients[i] = IngredientName(values[0]);
                }
            }
            return ingredients;
        }

        static void AddHelpRecipe(List<CraftingRecipe> recipes, string[] ingredients, int result, int count, float heat, string description) {
            if (result == 0) {
                return;
            }
            recipes.Add(new CraftingRecipe {
                ResultValue = result,
                ResultCount = Math.Max(1, count),
                Ingredients = ingredients,
                RequiredHeatLevel = heat,
                DisplayOrder = 100,
                Description = description
            });
        }

        /// <summary>
        /// Put every FD recipe into the static recipe list so the in-game recipe
        /// lookup (Recipaedia/help) can find it. Actual shapeless crafting still
        /// uses the ad-hoc proxy; these entries are the browsable representation.
        /// </summary>
        public static void RegisterHelpRecipes(List<CraftingRecipe> recipes) {
            EnsureLoaded();
            foreach (FDCraftingEntry entry in s_crafting) {
                AddHelpRecipe(recipes, Shape(entry.IngredientOptions), entry.ResultValue, entry.ResultCount, 0f, "FD crafting");
            }
            foreach (FDSmeltingEntry entry in s_smelting) {
                AddHelpRecipe(recipes, Shape(entry.IngredientOptions), entry.ResultValue, entry.ResultCount, 1f, "FD smelting");
            }

            int pot = BlocksManager.GetBlockIndex<FDCookingPotBlock>();
            int board = BlocksManager.GetBlockIndex<FDCuttingBoardBlock>();
            int knife = BlocksManager.GetBlockIndex<FDKnifeBlock>();
            int potValue = pot >= 0 ? Terrain.MakeBlockValue(pot) : 0;
            int boardValue = board >= 0 ? Terrain.MakeBlockValue(board) : 0;
            int knifeValue = knife >= 0 ? Terrain.MakeBlockValue(knife, 0, 0) : 0;

            foreach (FDCookingRecipe recipe in FDCookingRecipes.Recipes) {
                if (potValue == 0 || recipe.IngredientOptions.Length == 0) {
                    continue;
                }
                AddHelpRecipe(
                    recipes,
                    Shape([[potValue], recipe.IngredientOptions[0]]),
                    recipe.ResultValue,
                    recipe.ResultCount,
                    1f,
                    "FD cooking pot"
                );
            }
            foreach (FDCuttingRecipe recipe in FDCuttingRecipes.Recipes) {
                if (boardValue == 0 || knifeValue == 0) {
                    continue;
                }
                for (int i = 0; i < recipe.Results.Length; i++) {
                    AddHelpRecipe(
                        recipes,
                        Shape([[boardValue], [recipe.InputValue], [knifeValue]]),
                        recipe.Results[i],
                        recipe.Counts[i],
                        1f,
                        "FD cutting board"
                    );
                }
            }
            AddExtraRecipes(recipes);
        }

        static void AddExtraRecipe(List<CraftingRecipe> recipes,
            string resultName,
            int resultCount,
            float heat,
            params string[] ingredientNames) {
            int result = FDContent.GetValue(resultName);
            if (result == 0) {
                return;
            }
            int[][] options = new int[ingredientNames.Length][];
            for (int i = 0; i < ingredientNames.Length; i++) {
                int value = FDContent.GetValue(ingredientNames[i]);
                if (value == 0) {
                    return;
                }
                options[i] = [value];
            }
            AddHelpRecipe(recipes, Shape(options), result, resultCount, heat, "FD extra");
        }

        static void AddExtraRecipes(List<CraftingRecipe> recipes) {
            // Recipes present in the Java data but not representable by the JSON
            // shape converter (dough recipe, special tags, feast portions).
            AddExtraRecipe(recipes, "cooking_pot", 1, 0f, "ironingot", "ironingot", "ironingot", "brick", "water_bucket", "stick");
            AddExtraRecipe(recipes, "stove", 1, 0f, "ironingot", "ironingot", "ironingot", "brick", "brick", "brick", "campfire");
            AddExtraRecipe(recipes, "skillet", 1, 0f, "ironingot", "ironingot", "ironingot", "ironingot", "brick");
            AddExtraRecipe(recipes, "wheat_dough", 3, 0f, "wheat", "wheat", "wheat", "egg");
            AddExtraRecipe(recipes, "pie_crust", 1, 0f, "wheat", "wheat", "wheat", "milk_bucket");
            AddExtraRecipe(recipes, "sweet_berry_cookie", 8, 0f, "sweet_berries", "wheat", "wheat");
            AddExtraRecipe(recipes, "honey_cookie", 8, 0f, "honey_bottle", "wheat", "wheat");
            AddExtraRecipe(recipes, "fruit_salad", 1, 0f, "apple", "pumpkin", "pumpkin", "sweet_berries", "sweet_berries", "bowl");
            AddExtraRecipe(recipes, "melon_juice", 1, 0f, "pumpkin", "pumpkin", "pumpkin", "pumpkin", "sugar", "glass_bottle");
            AddExtraRecipe(recipes, "stuffed_potato", 1, 0f, "potato", "cooked_beef", "milk_bucket");
            AddExtraRecipe(recipes, "steak_and_potatoes", 1, 0f, "potato", "cooked_beef", "bowl", "onion", "cooked_rice");
            AddExtraRecipe(recipes, "roast_chicken_block", 1, 0f, "cooked_chicken", "potato", "carrot", "egg", "bread", "onion", "bowl", "potato", "potato");
            AddExtraRecipe(recipes, "horse_feed", 1, 0f, "straw_bale", "apple", "apple", "carrot");
            AddExtraRecipe(recipes, "ham", 1, 1f, "beef");
            AddExtraRecipe(recipes, "smoked_ham", 1, 1f, "ham");
            AddExtraRecipe(recipes, "dried_kelp", 1, 1f, "kelp");
            AddExtraRecipe(recipes, "pumpkin_slice", 4, 0f, "pumpkin");
            AddExtraRecipe(recipes, "cake_slice", 4, 0f, "sweet_berry_cheesecake");
            AddExtraRecipe(recipes, "melon_popsicle", 1, 0f, "melon_juice", "sugar");
            AddExtraRecipe(recipes, "nether_salad", 1, 0f, "brown_mushroom", "red_mushroom", "bowl");
            AddExtraRecipe(recipes, "sugar", 1, 0f, "sweet_berries", "sweet_berries");
            AddExtraRecipe(recipes, "cocoa_beans", 1, 0f, "sugar", "brown_mushroom");
            AddExtraRecipe(recipes, "honey_bottle", 1, 0f, "sugar", "glass_bottle", "water_bucket");
            AddExtraRecipe(recipes, "gold_ingot", 1, 0f, "ironingot", "diamond");
            AddExtraRecipe(recipes, "netherite_ingot", 1, 0f, "gold_ingot", "diamond");
            AddExtraRecipe(recipes, "roast_chicken", 4, 0f, "roast_chicken_block");
            AddExtraRecipe(recipes, "stuffed_pumpkin", 4, 0f, "stuffed_pumpkin_block");
            AddExtraRecipe(recipes, "honey_glazed_ham", 4, 0f, "honey_glazed_ham_block");
            AddExtraRecipe(recipes, "shepherds_pie", 4, 0f, "shepherds_pie_block");
            AddExtraRecipe(recipes, "gleaming_salad", 4, 0f, "gleaming_salad_block");
            AddExtraRecipe(recipes, "apple", 1, 0f, "oak_leaves");
            AddExtraRecipe(recipes, "sweet_berries", 1, 0f, "oak_leaves");
            AddExtraRecipe(recipes, "glow_berries", 1, 0f, "sweet_berries", "diamond");
            AddExtraRecipe(recipes, "rotten_tomato", 1, 0f, "tomato", "rotten_meat");
            AddExtraRecipe(recipes, "squid_ink_pasta", 1, 0f, "raw_pasta", "kelp");
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
