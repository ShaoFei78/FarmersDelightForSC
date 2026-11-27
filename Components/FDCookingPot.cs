using System;
using System.Collections.Generic;
using Engine;
using GameEntitySystem;
using TemplatesDatabase;

namespace Game {
    public class FDCookingRecipe {
        public int[][] IngredientOptions;
        public int ResultValue;
        public int ResultCount;
        public int ContainerValue;
        public int CookTime;
        public float Experience;
    }

    public class FDCuttingRecipe {
        public int InputValue;
        public int ToolLevel;
        public int[] Results;
        public int[] Counts;
    }

    public static class FDCookingRecipes {
        static List<FDCookingRecipe> s_recipes;

        public static int Count => s_recipes?.Count ?? 0;

        public static void EnsureLoaded() {
            if (s_recipes != null) {
                return;
            }
            s_recipes = [];
            // Filled by FDRecipes.Load() from Assets/FD/cooking.json.
        }

        public static bool TryFind(List<int> inputs, out FDCookingRecipe recipe) {
            EnsureLoaded();
            recipe = null;
            if (inputs.Count == 0) {
                return false;
            }
            foreach (FDCookingRecipe candidate in s_recipes) {
                if (candidate.IngredientOptions.Length != inputs.Count) {
                    continue;
                }
                bool[] used = new bool[inputs.Count];
                if (Match(candidate, inputs, used, 0)) {
                    recipe = candidate;
                    return true;
                }
            }
            return false;
        }

        static bool Match(FDCookingRecipe recipe, List<int> inputs, bool[] used, int index) {
            if (index >= recipe.IngredientOptions.Length) {
                return true;
            }
            int[] options = recipe.IngredientOptions[index];
            for (int i = 0; i < inputs.Count; i++) {
                if (used[i]) {
                    continue;
                }
                if (Array.IndexOf(options, inputs[i]) < 0) {
                    continue;
                }
                used[i] = true;
                if (Match(recipe, inputs, used, index + 1)) {
                    return true;
                }
                used[i] = false;
            }
            return false;
        }

        public static void SetRecipes(List<FDCookingRecipe> recipes) {
            s_recipes = recipes;
        }
    }

    public static class FDCuttingRecipes {
        static List<FDCuttingRecipe> s_recipes;

        public static int Count => s_recipes?.Count ?? 0;

        public static void EnsureLoaded() {
            if (s_recipes != null) {
                return;
            }
            s_recipes = [];
        }

        public static bool TryFind(int input, int toolLevel, out FDCuttingRecipe recipe) {
            EnsureLoaded();
            recipe = null;
            foreach (FDCuttingRecipe candidate in s_recipes) {
                if (candidate.InputValue == input && toolLevel >= candidate.ToolLevel) {
                    recipe = candidate;
                    return true;
                }
            }
            return false;
        }

        public static bool HasInput(int input) {
            EnsureLoaded();
            foreach (FDCuttingRecipe candidate in s_recipes) {
                if (candidate.InputValue == input) {
                    return true;
                }
            }
            return false;
        }

        public static void SetRecipes(List<FDCuttingRecipe> recipes) {
            s_recipes = recipes;
        }
    }

    /// <summary>Cooking pot: six ingredient slots, result slot, container slot.</summary>
    public class ComponentFDCookingPot : ComponentInventoryBase, IUpdateable {
        public const int IngredientSlots = 6;
        public const int ResultSlot = 6;
        public const int ContainerSlot = 7;

        ComponentBlockEntity m_blockEntity;
        SubsystemTerrain m_subsystemTerrain;
        SubsystemTime m_subsystemTime;
        public float CookProgress;
        public FDCookingRecipe CurrentRecipe;
        float m_heat;

        public UpdateOrder UpdateOrder => UpdateOrder.Default;
        public float HeatLevel => m_heat;
        public float Progress => CurrentRecipe == null ? 0f : Math.Clamp(CookProgress / Math.Max(1, CurrentRecipe.CookTime), 0f, 1f);

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            base.Load(valuesDictionary, idToEntityMap);
            m_blockEntity = Entity.FindComponent<ComponentBlockEntity>(true);
            m_subsystemTerrain = Project.FindSubsystem<SubsystemTerrain>(true);
            m_subsystemTime = Project.FindSubsystem<SubsystemTime>(true);
            CookProgress = valuesDictionary.GetValue("CookProgress", 0f);
        }

        public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap) {
            base.Save(valuesDictionary, entityToIdMap);
            valuesDictionary.SetValue("CookProgress", CookProgress);
        }

        public void Update(float dt) {
            if (m_blockEntity == null) {
                return;
            }
            Point3 p = m_blockEntity.Coordinates;
            int below = m_subsystemTerrain.Terrain.GetCellValue(p.X, p.Y - 1, p.Z);
            m_heat = GetHeat(below);
            if (m_heat <= 0f) {
                CookProgress = Math.Max(0f, CookProgress - dt * 2f);
                return;
            }
            List<int> inputs = [];
            for (int i = 0; i < IngredientSlots; i++) {
                if (GetSlotCount(i) > 0) {
                    inputs.Add(GetSlotValue(i));
                }
            }
            if (inputs.Count == 0) {
                CookProgress = 0f;
                CurrentRecipe = null;
                return;
            }
            if (!FDCookingRecipes.TryFind(inputs, out FDCookingRecipe recipe)) {
                CookProgress = 0f;
                CurrentRecipe = null;
                return;
            }
            CurrentRecipe = recipe;
            // FD cooking times are in Minecraft ticks (20 per second); SC's dt is seconds.
            CookProgress += dt * 20f;
            if (CookProgress >= recipe.CookTime) {
                if (!CanAdd(ResultSlot, recipe.ResultValue, recipe.ResultCount)
                    || !CanAdd(ContainerSlot, recipe.ContainerValue, 1)) {
                    CookProgress = recipe.CookTime;
                    return;
                }
                CookProgress = 0f;
                Craft(recipe);
            }
        }

        bool CanAdd(int slot, int value, int count) {
            if (value == 0 || count <= 0) {
                return true;
            }
            if (GetSlotCount(slot) == 0) {
                return count <= GetSlotCapacity(slot, value);
            }
            return GetSlotValue(slot) == value && GetSlotCount(slot) + count <= GetSlotCapacity(slot, value);
        }

        float GetHeat(int belowValue) {
            Block block = BlocksManager.Blocks[Terrain.ExtractContents(belowValue)];
            if (block is FDStoveBlock) {
                return (Terrain.ExtractData(belowValue) & 1) != 0 ? 1f : 0f;
            }
            if (block is FireBlock || block is CampfireBlock || block is MagmaBlock) {
                return 1f;
            }
            return 0f;
        }

        void Craft(FDCookingRecipe recipe) {
            for (int r = 0; r < recipe.IngredientOptions.Length; r++) {
                int[] options = recipe.IngredientOptions[r];
                for (int i = 0; i < IngredientSlots; i++) {
                    if (GetSlotCount(i) > 0 && Array.IndexOf(options, GetSlotValue(i)) >= 0) {
                        RemoveSlotItems(i, 1);
                        break;
                    }
                }
            }
            if (recipe.ResultValue != 0) {
                AddToSlot(ResultSlot, recipe.ResultValue, recipe.ResultCount);
            }
            if (recipe.ContainerValue != 0) {
                AddToSlot(ContainerSlot, recipe.ContainerValue, 1);
            }
        }

        void AddToSlot(int slot, int value, int count) {
            if (GetSlotCount(slot) == 0) {
                AddSlotItems(slot, value, count);
            }
            else if (GetSlotValue(slot) == value) {
                AddSlotItems(slot, value, count);
            }
        }
    }

    /// <summary>Cutting board holds one item until a knife cuts it.</summary>
    public class ComponentFDCuttingBoard : Component {
        public int ItemValue;

        public override void Load(ValuesDictionary valuesDictionary, IdToEntityMap idToEntityMap) {
            base.Load(valuesDictionary, idToEntityMap);
            ItemValue = valuesDictionary.GetValue("ItemValue", 0);
        }

        public override void Save(ValuesDictionary valuesDictionary, EntityToIdMap entityToIdMap) {
            base.Save(valuesDictionary, entityToIdMap);
            valuesDictionary.SetValue("ItemValue", ItemValue);
        }
    }
}
