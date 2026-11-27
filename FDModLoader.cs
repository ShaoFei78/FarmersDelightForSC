using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    public class FDModLoader : ModLoader {
        public override void __ModInitialize() {
            ModsManager.RegisterHook("BlocksInitalized", this);
            ModsManager.RegisterHook("OnTerrainContentsGenerated", this);
            ModsManager.RegisterHook("OnLoadingFinished", this);
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
