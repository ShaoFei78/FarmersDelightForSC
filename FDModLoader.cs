using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    public class FDModLoader : ModLoader {
        public override void __ModInitialize() {
            ModsManager.RegisterHook("BlocksInitalized", this);
            ModsManager.RegisterHook("OnTerrainContentsGenerated", this);
            ModsManager.RegisterHook("OnLoadingFinished", this);
            ModsManager.RegisterHook("CraftingRecipesManagerInitialize", this);
        }

        public override void CraftingRecipesManagerInitialize(List<CraftingRecipe> recipes, ref bool sort) {
            FDContent.Initialize();
            FDRecipes.EnsureLoaded();
            FDRecipes.RegisterHelpRecipes(recipes);
            Log.Information($"[FD] Registered browsable recipes: {recipes.Count}");
        }

        public override void BlocksInitalized() {
            FDContent.Initialize();
            FDRecipes.EnsureLoaded();
            Log.Information("[FD] Farmer's Delight blocks initialized.");
        }

        public override void OnTerrainContentsGenerated(TerrainChunk chunk) {
            if (chunk != null) {
                FDWorldGen.Generate(chunk);
            }
        }

        public override void OnLoadingFinished(List<Action> actions) {
            Log.Information("[FD] Farmer's Delight for Survivalcraft loaded. 农夫乐事移植版已加载。");
        }
    }
}
