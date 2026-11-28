using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    /// <summary>Base helpers for Farmer's Delight blocks that use the shared 16x16 block atlas.</summary>
    public abstract class FDBlockBase : Block {
        static Texture2D s_atlas;

        public static Texture2D Atlas {
            get {
                if (s_atlas == null) {
                    s_atlas = ContentManager.Get<Texture2D>("Textures/FD/Blocks");
                }
                return s_atlas;
            }
        }

        public static void EnsureAtlas() {
            _ = Atlas;
        }

        public override Texture2D GetDefaultTexture(int value) => Atlas;

        public override int GetTextureSlotCount(int value) => 16;
    }

    public abstract class FDCubeBlockBase : CubeBlock {
        protected virtual int GetDisplayData(int data) => data;

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int data = GetDisplayData(Terrain.ExtractData(value));
            string key = $"{GetType().Name}:{data}";
            return LanguageControl.TryGetBlock(key, "DisplayName", out string name) ? name : DefaultDisplayName;
        }

        public override string GetDescription(int value) {
            int data = GetDisplayData(Terrain.ExtractData(value));
            string key = $"{GetType().Name}:{data}";
            return LanguageControl.TryGetBlock(key, "Description", out string description) ? description : DefaultDescription;
        }

        public override Texture2D GetDefaultTexture(int value) => FDBlockBase.Atlas;

        public override int GetTextureSlotCount(int value) => 16;

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, Terrain.ExtractData(oldValue)), Count = 1 });
            showDebris = true;
        }
    }

    public abstract class FDCrossBlockBase : CrossBlock {
        protected virtual int GetDisplayData(int data) => data;

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int data = GetDisplayData(Terrain.ExtractData(value));
            string key = $"{GetType().Name}:{data}";
            return LanguageControl.TryGetBlock(key, "DisplayName", out string name) ? name : DefaultDisplayName;
        }

        public override string GetDescription(int value) {
            int data = GetDisplayData(Terrain.ExtractData(value));
            string key = $"{GetType().Name}:{data}";
            return LanguageControl.TryGetBlock(key, "Description", out string description) ? description : DefaultDescription;
        }

        public override Texture2D GetDefaultTexture(int value) => FDBlockBase.Atlas;

        public override int GetTextureSlotCount(int value) => 16;

        public override int GetShadowStrength(int value) => 5;

        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value) => true;

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, Terrain.ExtractData(oldValue)), Count = 1 });
            showDebris = true;
        }
    }

    public class FDStoveBlock : FDCubeBlockBase {
        protected override int GetDisplayData(int data) => 0;
        int m_side;
        int m_top;
        int m_topOn;
        int m_front;
        int m_frontOn;
        int m_bottom;

        public override void Initialize() {
            base.Initialize();
            m_side = FDAtlas.Slot("stove_side");
            m_top = FDAtlas.Slot("stove_top");
            m_topOn = FDAtlas.Slot("stove_top_on");
            m_front = FDAtlas.Slot("stove_front");
            m_frontOn = FDAtlas.Slot("stove_front_on");
            m_bottom = FDAtlas.Slot("stove_bottom");
            DefaultDisplayName = "Stove";
            DefaultCategory = "Construction";
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int data = Terrain.ExtractData(value);
            bool lit = (data & 1) != 0;
            int facing = (data >> 4) & 3;
            if (face == 4) {
                return lit ? m_topOn : m_top;
            }
            if (face == 5) {
                return m_bottom;
            }
            if (face == facing) {
                return lit ? m_frontOn : m_front;
            }
            return m_side;
        }

        public override int GetEmittedLightAmount(int value) => (Terrain.ExtractData(value) & 1) != 0 ? 13 : 0;

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            int facing = 0;
            float maxDot = float.MinValue;
            for (int i = 0; i < 4; i++) {
                float dot = Vector3.Dot(CellFace.FaceToVector3(i), forward);
                if (dot > maxDot) {
                    maxDot = dot;
                    facing = i;
                }
            }
            BlockPlacementData result = default;
            result.Value = Terrain.MakeBlockValue(BlockIndex, 0, facing << 4);
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(BlockIndex);
            yield return Terrain.MakeBlockValue(BlockIndex, 0, 1);
        }
    }

    public class FDCookingPotBlock : FDCubeBlockBase {
        int m_side;
        int m_top;
        int m_bottom;

        public override void Initialize() {
            base.Initialize();
            m_side = FDAtlas.Slot("cooking_pot_side");
            m_top = FDAtlas.Slot("cooking_pot_top");
            m_bottom = FDAtlas.Slot("cooking_pot_bottom");
            DefaultDisplayName = "Cooking Pot";
            DefaultCategory = "Construction";
            DefaultIsInteractive = true;
        }

        public override int GetFaceTextureSlot(int face, int value) {
            if (face == 4) {
                return m_top;
            }
            if (face == 5) {
                return m_bottom;
            }
            return m_side;
        }
    }

    public class FDSkilletBlock : FDCubeBlockBase {
        int m_side;
        int m_top;
        int m_bottom;

        public override void Initialize() {
            base.Initialize();
            m_side = FDAtlas.Slot("skillet_side");
            m_top = FDAtlas.Slot("skillet_top");
            m_bottom = FDAtlas.Slot("skillet_bottom");
            DefaultDisplayName = "Skillet";
            DefaultCategory = "Tools";
            DefaultIsInteractive = true;
        }

        public override int GetFaceTextureSlot(int face, int value) {
            if (face == 4) {
                return m_top;
            }
            if (face == 5) {
                return m_bottom;
            }
            return m_side;
        }
    }

    public class FDCuttingBoardBlock : FDCubeBlockBase {
        int m_texture;

        public override void Initialize() {
            base.Initialize();
            m_texture = FDAtlas.Slot("cutting_board");
            DefaultDisplayName = "Cutting Board";
            DefaultCategory = "Construction";
            DefaultIsInteractive = true;
        }

        public override int GetFaceTextureSlot(int face, int value) => m_texture;
    }

    public class FDBasketBlock : FDCubeBlockBase {
        protected override int GetDisplayData(int data) => data & 1;
        int[] m_side = new int[2];
        int[] m_top = new int[2];
        int[] m_bottom = new int[2];

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < 2; i++) {
                string wood = i == 0 ? "wooden" : "bamboo";
                m_side[i] = FDAtlas.Slot($"{wood}_basket_side");
                m_top[i] = FDAtlas.Slot($"{wood}_basket_top");
                m_bottom[i] = FDAtlas.Slot($"{wood}_basket_bottom");
            }
            DefaultDisplayName = "Basket";
            DefaultCategory = "Construction";
            DefaultIsInteractive = true;
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int variant = Terrain.ExtractData(value) & 1;
            if (face == 4) {
                return m_top[variant];
            }
            if (face == 5) {
                return m_bottom[variant];
            }
            return m_side[variant];
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(BlockIndex, 0, 0);
            yield return Terrain.MakeBlockValue(BlockIndex, 0, 1);
        }
    }

    public class FDCabinetBlock : FDCubeBlockBase {
        protected override int GetDisplayData(int data) => data & 15;
        public static readonly string[] Woods = ["oak", "spruce", "birch", "jungle", "acacia", "dark_oak", "mangrove", "cherry", "bamboo", "crimson", "warped"];
        int[] m_front = new int[11];
        int[] m_side = new int[11];
        int[] m_top = new int[11];

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < Woods.Length; i++) {
                m_front[i] = FDAtlas.Slot($"{Woods[i]}_cabinet_front");
                m_side[i] = FDAtlas.Slot($"{Woods[i]}_cabinet_side");
                m_top[i] = FDAtlas.Slot($"{Woods[i]}_cabinet_top");
            }
            DefaultDisplayName = "Cabinet";
            DefaultCategory = "Construction";
            DefaultIsInteractive = true;
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int data = Terrain.ExtractData(value);
            int wood = Math.Clamp(data & 15, 0, 10);
            int facing = (data >> 4) & 3;
            if (face == 4) {
                return m_top[wood];
            }
            if (face == 5) {
                return m_side[wood];
            }
            if (face == facing) {
                return m_front[wood];
            }
            return m_side[wood];
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            Vector3 forward = Matrix.CreateFromQuaternion(componentMiner.ComponentCreature.ComponentCreatureModel.EyeRotation).Forward;
            int facing = 0;
            float maxDot = float.MinValue;
            for (int i = 0; i < 4; i++) {
                float dot = Vector3.Dot(CellFace.FaceToVector3(i), forward);
                if (dot > maxDot) {
                    maxDot = dot;
                    facing = i;
                }
            }
            int wood = Terrain.ExtractData(value) & 15;
            BlockPlacementData result = default;
            result.Value = Terrain.MakeBlockValue(BlockIndex, 0, wood | (facing << 4));
            result.CellFace = raycastResult.CellFace;
            return result;
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < Woods.Length; i++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, i);
            }
        }
    }

    public class FDCrateBlock : FDCubeBlockBase {
        protected override int GetDisplayData(int data) => data & 15;
        public static readonly string[] Crops = ["carrot", "potato", "beetroot", "cabbage", "tomato", "onion"];
        int[] m_side = new int[6];
        int[] m_top = new int[6];

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < Crops.Length; i++) {
                m_side[i] = FDAtlas.Slot($"{Crops[i]}_crate_side");
                m_top[i] = FDAtlas.Slot($"{Crops[i]}_crate_top");
            }
            DefaultDisplayName = "Crate";
            DefaultCategory = "Construction";
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int index = Math.Clamp(Terrain.ExtractData(value) & 15, 0, 5);
            return face == 4 ? m_top[index] : m_side[index];
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < Crops.Length; i++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, i);
            }
        }
    }

    public class FDBaleBlock : FDCubeBlockBase {
        protected override int GetDisplayData(int data) => data & 15;
        int[] m_side = new int[3];
        int[] m_top = new int[3];

        public override void Initialize() {
            base.Initialize();
            m_side[0] = FDAtlas.Slot("rice_bale_side");
            m_top[0] = FDAtlas.Slot("rice_bale_top");
            m_side[1] = FDAtlas.Slot("rice_bag_side");
            m_top[1] = FDAtlas.Slot("rice_bag_top");
            m_side[2] = FDAtlas.Slot("straw_bale_side");
            m_top[2] = FDAtlas.Slot("straw_bale_end");
            DefaultDisplayName = "Bale";
            DefaultCategory = "Construction";
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int index = Math.Clamp(Terrain.ExtractData(value) & 15, 0, 2);
            return face == 4 || face == 5 ? m_top[index] : m_side[index];
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(BlockIndex, 0, 0);
            yield return Terrain.MakeBlockValue(BlockIndex, 0, 1);
            yield return Terrain.MakeBlockValue(BlockIndex, 0, 2);
        }
    }
}



