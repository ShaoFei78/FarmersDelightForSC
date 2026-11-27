using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    /// <summary>
    /// All ordinary Farmer's Delight items (food, ingredients, drinks, staples) in
    /// one FlatBlock. Data selects an entry in FDItemDefs.
    /// </summary>
    public class FDItemBlock : FlatBlock {
        public Texture2D m_texture;

        public override void Initialize() {
            base.Initialize();
            FDItemDefs.EnsureLoaded();
            m_texture = ContentManager.Get<Texture2D>("Textures/FD/Items");
            DefaultCategory = "Items";
            DefaultDisplayName = "Farmer's Delight Item";
        }

        public override Texture2D GetDefaultTexture(int value) => m_texture ??= ContentManager.Get<Texture2D>("Textures/FD/Items");

        public override int GetTextureSlotCount(int value) => 16;

        public override int GetFaceTextureSlot(int face, int value) {
            FDItemDef def = FDItemDefs.Get(Terrain.ExtractData(value));
            return def?.Slot ?? 0;
        }

        public override IEnumerable<int> GetCreativeValues() {
            FDItemDefs.EnsureLoaded();
            foreach (FDItemDef def in FDItemDefs.All) {
                if (def.Kind == "item") {
                    yield return Terrain.MakeBlockValue(BlockIndex, 0, def.Index);
                }
            }
        }

        public override string GetCategory(int value) {
            FDItemDef def = FDItemDefs.Get(Terrain.ExtractData(value));
            return def != null && def.Nutrition > 0 ? "Food" : "Items";
        }

        public override int GetMaxStacking(int value) {
            FDItemDef def = FDItemDefs.Get(Terrain.ExtractData(value));
            return Math.Clamp(def?.Stack ?? 40, 1, 40);
        }

        public override float GetNutritionalValue(int value) {
            FDItemDef def = FDItemDefs.Get(Terrain.ExtractData(value));
            return def == null ? 0f : def.Nutrition * 0.05f;
        }

        public override FoodType GetFoodType(int value) {
            FDItemDef def = FDItemDefs.Get(Terrain.ExtractData(value));
            return def != null && Enum.TryParse(def.FoodType, out FoodType type) ? type : FoodType.None;
        }

        public override float GetSicknessProbability(int value) {
            FDItemDef def = FDItemDefs.Get(Terrain.ExtractData(value));
            return def?.Sickness ?? 0f;
        }

        public override int GetRotPeriod(int value) {
            FDItemDef def = FDItemDefs.Get(Terrain.ExtractData(value));
            return def?.Rot ?? 0;
        }

        public override string GetCraftingId(int value) => "fditem";
    }

    /// <summary>Five Farmer's Delight knife tiers (flint/iron/gold/diamond/netherite).</summary>
    public class FDKnifeBlock : FlatBlock {
        public static readonly string[] KnifeNames = ["flint_knife", "iron_knife", "golden_knife", "diamond_knife", "netherite_knife"];
        public static readonly int[] Durabilities = [80, 250, 32, 1561, 2031];
        public static readonly int[] ToolLevels = [1, 2, 2, 3, 4];
        public static readonly float[] MeleePowers = [2f, 3f, 2f, 4f, 5f];

        public Texture2D m_texture;

        public override void Initialize() {
            base.Initialize();
            FDItemDefs.EnsureLoaded();
            m_texture = ContentManager.Get<Texture2D>("Textures/FD/Items");
            DefaultCategory = "Tools";
            DefaultDisplayName = "Knife";
        }

        public override Texture2D GetDefaultTexture(int value) => m_texture ??= ContentManager.Get<Texture2D>("Textures/FD/Items");

        public override int GetTextureSlotCount(int value) => 16;

        public override int GetFaceTextureSlot(int face, int value) {
            int tier = Math.Clamp(Terrain.ExtractData(value) & 15, 0, KnifeNames.Length - 1);
            FDItemDef def = FDItemDefs.Find(KnifeNames[tier]);
            return def?.Slot ?? 0;
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < KnifeNames.Length; i++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, i);
            }
        }

        public override int GetMaxStacking(int value) => 1;

        public override int GetDurability(int value) => Durabilities[Math.Clamp(Terrain.ExtractData(value) & 15, 0, Durabilities.Length - 1)];

        public override int GetToolLevel(int value) => ToolLevels[Math.Clamp(Terrain.ExtractData(value) & 15, 0, ToolLevels.Length - 1)];

        public override float GetMeleePower(int value) => MeleePowers[Math.Clamp(Terrain.ExtractData(value) & 15, 0, MeleePowers.Length - 1)];

        public override float GetMeleeHitProbability(int value) => 0.8f;

        public override int GetDamageDestructionValue(int value) => 0;

        public override string GetCraftingId(int value) => "fdknife";
    }

    /// <summary>
    /// Plantable FD seeds. Data: 0 cabbage, 1 tomato, 2 onion, 3 rice.
    /// Onion is edible, so it also carries food values.
    /// </summary>
    public class FDSeedItemBlock : FlatBlock {
        public static readonly string[] SeedNames = ["cabbage_seeds", "tomato_seeds", "onion", "rice"];

        public Texture2D m_texture;

        public override void Initialize() {
            base.Initialize();
            FDItemDefs.EnsureLoaded();
            m_texture = ContentManager.Get<Texture2D>("Textures/FD/Items");
            DefaultCategory = "Plants";
            DefaultDisplayName = "Seeds";
        }

        public override Texture2D GetDefaultTexture(int value) => m_texture ??= ContentManager.Get<Texture2D>("Textures/FD/Items");

        public override int GetTextureSlotCount(int value) => 16;

        public override int GetFaceTextureSlot(int face, int value) {
            FDItemDef def = FDItemDefs.Find(SeedNames[Math.Clamp(Terrain.ExtractData(value) & 15, 0, SeedNames.Length - 1)]);
            return def?.Slot ?? 0;
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < SeedNames.Length; i++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, i);
            }
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            BlockPlacementData result = default;
            if (raycastResult.CellFace.Face != 4) {
                return result;
            }
            int belowValue = subsystemTerrain.Terrain.GetCellValue(
                raycastResult.CellFace.X,
                raycastResult.CellFace.Y,
                raycastResult.CellFace.Z
            );
            if (!BlocksManager.Blocks[Terrain.ExtractContents(belowValue)].IsSuitableForPlants(belowValue, value)) {
                return result;
            }
            int cropIndex = BlocksManager.GetBlockIndex<FDCropBlock>();
            if (cropIndex < 0) {
                return result;
            }
            int type = Math.Clamp(Terrain.ExtractData(value) & 15, 0, 3);
            result.Value = Terrain.MakeBlockValue(cropIndex, 0, FDCropBlock.MakeData(type, 0));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override string GetCraftingId(int value) => "fdseed";

        public override float GetNutritionalValue(int value) => (Terrain.ExtractData(value) & 15) == 2 ? 0.1f : 0f;

        public override FoodType GetFoodType(int value) => (Terrain.ExtractData(value) & 15) == 2 ? FoodType.Fruit : FoodType.None;

        public override int GetMaxStacking(int value) => 40;
    }
}

