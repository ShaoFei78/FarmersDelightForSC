using System;
using System.Collections.Generic;
using Engine;
using Engine.Graphics;

namespace Game {
    public class FDRopeBlock : FDCrossBlockBase {
        int m_texture;

        public override void Initialize() {
            base.Initialize();
            m_texture = FDAtlas.Slot("rope");
            DefaultDisplayName = "Rope";
            DefaultCategory = "Construction";
        }

        public override int GetFaceTextureSlot(int face, int value) => m_texture;

        public override int GetShadowStrength(int value) => 0;
    }

    /// <summary>Thin placed decoration (rug, tatami, safety net) with real terrain geometry.</summary>
    public abstract class FDFlatDecorBlock : FDCubeBlockBase {
        public virtual float Height => 0.0625f;
        public virtual bool HasCollision => true;

        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value) => face != 5;

        public override void GenerateTerrainVertices(BlockGeometryGenerator generator, TerrainGeometry geometry, int value, int x, int y, int z) {
            Texture2D texture = GetDefaultTexture(value);
            TerrainGeometrySubset[] subsets = texture == null
                ? geometry.OpaqueSubsetsByFace
                : geometry.GetGeometry(texture).OpaqueSubsetsByFace;
            generator.GenerateCubeVertices(
                this,
                value,
                x,
                y,
                z,
                Height,
                Height,
                Height,
                Height,
                Color.White,
                Color.White,
                Color.White,
                Color.White,
                Color.White,
                -1,
                subsets
            );
        }

        public override BoundingBox[] GetCustomCollisionBoxes(SubsystemTerrain terrain, int value) {
            return HasCollision
                ? [new BoundingBox(new Vector3(0f, 0f, 0f), new Vector3(1f, Height, 1f))]
                : [];
        }
    }

    public class FDSafetyNetBlock : FDFlatDecorBlock {
        public override bool HasCollision => false;
        int m_texture;

        public override void Initialize() {
            base.Initialize();
            m_texture = FDAtlas.Slot("safety_net");
            DefaultDisplayName = "Safety Net";
            DefaultCategory = "Construction";
        }

        public override Texture2D GetDefaultTexture(int value) => FDBlockBase.Atlas;

        public override int GetTextureSlotCount(int value) => 16;

        public override int GetFaceTextureSlot(int face, int value) => m_texture;
    }

    public class FDRopeFenceBlock : FDCubeBlockBase {
        int m_texture;

        public override void Initialize() {
            base.Initialize();
            m_texture = FDAtlas.Slot("rope_fence");
            DefaultDisplayName = "Rope Fence";
            DefaultCategory = "Construction";
        }

        public override int GetFaceTextureSlot(int face, int value) => m_texture;
    }

    public class FDRopeFenceGateBlock : FDCubeBlockBase {
        int m_texture;

        public override void Initialize() {
            base.Initialize();
            m_texture = FDAtlas.Slot("rope_fence_gate");
            DefaultDisplayName = "Rope Fence Gate";
            DefaultCategory = "Construction";
        }

        public override int GetFaceTextureSlot(int face, int value) => m_texture;
    }

    public class FDTatamiBlock : FDFlatDecorBlock {
        int[] m_textures = new int[3];

        public override void Initialize() {
            base.Initialize();
            m_textures[0] = FDAtlas.Slot("tatami");
            m_textures[1] = FDAtlas.Slot("tatami_mat_side");
            m_textures[2] = FDAtlas.Slot("tatami_mat_half");
            DefaultDisplayName = "Tatami";
            DefaultCategory = "Construction";
        }

        public override Texture2D GetDefaultTexture(int value) => FDBlockBase.Atlas;

        public override int GetTextureSlotCount(int value) => 16;

        public override int GetFaceTextureSlot(int face, int value) {
            int index = Math.Clamp(Terrain.ExtractData(value) & 15, 0, 2);
            return m_textures[index];
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < 3; i++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, i);
            }
        }
    }

    public class FDCanvasRugBlock : FDFlatDecorBlock {
        public override bool HasCollision => false;
        int m_texture;

        public override void Initialize() {
            base.Initialize();
            m_texture = FDAtlas.Slot("canvas_rug");
            DefaultDisplayName = "Canvas Rug";
            DefaultCategory = "Construction";
        }

        public override Texture2D GetDefaultTexture(int value) => FDBlockBase.Atlas;

        public override int GetTextureSlotCount(int value) => 16;

        public override int GetFaceTextureSlot(int face, int value) => m_texture;
    }

    public class FDOrganicCompostBlock : FDCubeBlockBase {
        int[] m_textures = new int[4];

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < 4; i++) {
                m_textures[i] = FDAtlas.Slot($"organic_compost_stage{i}");
            }
            DefaultDisplayName = "Organic Compost";
            DefaultCategory = "Construction";
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int stage = Math.Clamp(Terrain.ExtractData(value) & 3, 0, 3);
            return m_textures[stage];
        }
    }

    public class FDRichSoilBlock : FDCubeBlockBase {
        int m_texture;

        public override void Initialize() {
            base.Initialize();
            m_texture = FDAtlas.Slot("rich_soil");
            DefaultDisplayName = "Rich Soil";
            DefaultCategory = "Construction";
        }

        public override int GetFaceTextureSlot(int face, int value) => m_texture;

        public override bool IsSuitableForPlants(int value, int plantValue) => true;
    }

    public class FDRichSoilFarmlandBlock : FDCubeBlockBase {
        int m_dry;
        int m_moist;

        public override void Initialize() {
            base.Initialize();
            m_dry = FDAtlas.Slot("rich_soil_farmland");
            m_moist = FDAtlas.Slot("rich_soil_farmland_moist");
            DefaultDisplayName = "Rich Soil Farmland";
            DefaultCategory = "Construction";
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int data = Terrain.ExtractData(value);
            return (data & 1) != 0 ? m_moist : m_dry;
        }

        public override bool IsSuitableForPlants(int value, int plantValue) => true;
    }

    public class FDMushroomColonyBlock : FDCrossBlockBase {
        protected override int GetDisplayData(int data) => data & 1;
        int[,] m_textures = new int[2, 4];

        public override void Initialize() {
            base.Initialize();
            for (int type = 0; type < 2; type++) {
                string color = type == 0 ? "brown" : "red";
                for (int stage = 0; stage < 4; stage++) {
                    m_textures[type, stage] = FDAtlas.Slot($"{color}_mushroom_colony_stage{stage}");
                }
            }
            DefaultDisplayName = "Mushroom Colony";
            DefaultCategory = "Plants";
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int data = Terrain.ExtractData(value);
            int type = data & 1;
            int stage = Math.Clamp((data >> 4) & 3, 0, 3);
            return m_textures[type, stage];
        }

        public override IEnumerable<int> GetCreativeValues() {
            yield return Terrain.MakeBlockValue(BlockIndex, 0, 3 | (3 << 4));
            yield return Terrain.MakeBlockValue(BlockIndex, 0, 1 | (3 << 4));
        }
    }

    /// <summary>
    /// Four FD crops. Data low nibble = stage, high nibble = type
    /// (0 cabbage, 1 onion, 2 tomato, 3 rice).
    /// </summary>
    public class FDCropBlock : FDCrossBlockBase {
        protected override int GetDisplayData(int data) => FDCropBlock.GetType(data);
        public const int TypeCabbage = 0;
        public const int TypeOnion = 1;
        public const int TypeTomato = 2;
        public const int TypeRice = 3;
        int[,] m_textures = new int[4, 8];

        public override void Initialize() {
            base.Initialize();
            string[] names = ["cabbages", "onions", "tomatoes", "rice"];
            int[] stages = [8, 4, 4, 4];
            for (int type = 0; type < 4; type++) {
                for (int stage = 0; stage < stages[type]; stage++) {
                    string texture = type == 2 && stage < 3
                        ? $"budding_tomatoes_stage{stage}"
                        : $"{names[type]}_stage{stage}";
                    m_textures[type, stage] = FDAtlas.Slot(texture);
                }
            }
            DefaultDisplayName = "Crop";
            DefaultCategory = "Plants";
        }

        public static int GetType(int data) => (data >> 4) & 15;
        public static int GetStage(int data) => data & 15;
        public static int MakeData(int type, int stage) => (type << 4) | (stage & 15);

        public override int GetFaceTextureSlot(int face, int value) {
            int data = Terrain.ExtractData(value);
            int type = Math.Clamp(GetType(data), 0, 3);
            int stage = Math.Clamp(GetStage(data), 0, type switch { 1 or 2 or 3 => 3, _ => 7 });
            return m_textures[type, stage];
        }

        public override bool IsFaceTransparent(SubsystemTerrain subsystemTerrain, int face, int value) => true;

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            int type = GetType(data);
            int stage = GetStage(data);
            int matureStage = type is 1 or 2 or 3 ? 3 : 7;
            if (stage >= matureStage) {
                string produce = type switch {
                    0 => "cabbage",
                    1 => "onion",
                    2 => "tomato",
                    _ => "rice"
                };
                dropValues.Add(new BlockDropValue { Value = FDContent.ItemValue(produce), Count = 1 });
            }
            string seed = type switch {
                0 => "cabbage_seeds",
                1 => "onion",
                2 => "tomato_seeds",
                _ => "rice"
            };
            dropValues.Add(new BlockDropValue { Value = FDContent.SeedValue(seed), Count = 1 });
            showDebris = true;
        }
    }

    public class FDWildCropBlock : FDCrossBlockBase {
        public static readonly string[] Textures = ["wild_cabbages", "wild_onions", "wild_tomatoes", "wild_carrots", "wild_potatoes", "wild_beetroots", "wild_rice_bottom", "sandy_shrub"];
        public static readonly string[] Drops = ["cabbage", "onion", "tomato", "carrot", "potato", "beetroot", "rice", "straw"];
        int[] m_textures = new int[Textures.Length];

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < Textures.Length; i++) {
                m_textures[i] = FDAtlas.Slot(Textures[i]);
            }
            DefaultDisplayName = "Wild Crop";
            DefaultCategory = "Plants";
        }

        public override int GetFaceTextureSlot(int face, int value) {
            int index = Math.Clamp(Terrain.ExtractData(value) & 15, 0, Textures.Length - 1);
            return m_textures[index];
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < Textures.Length; i++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, i);
            }
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            int index = Math.Clamp(Terrain.ExtractData(oldValue) & 15, 0, Drops.Length - 1);
            string drop = Drops[index];
            dropValues.Add(new BlockDropValue { Value = FDContent.ItemValue(drop), Count = 1 });
            showDebris = true;
        }
    }

    public class FDSandyShrubBlock : FDCrossBlockBase {
        int m_texture;

        public override void Initialize() {
            base.Initialize();
            m_texture = FDAtlas.Slot("sandy_shrub");
            DefaultDisplayName = "Sandy Shrub";
            DefaultCategory = "Plants";
        }

        public override int GetFaceTextureSlot(int face, int value) => m_texture;
    }
}


