using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    /// <summary>
    /// Pie block. Data low 2 bits = pie type, bits 4..7 = remaining servings.
    /// </summary>
    public class FDPieBlock : FDCubeBlockBase {
        protected override int GetDisplayData(int data) => FDPieBlock.GetType(data);
        public static readonly string[] Types = ["apple_pie", "sweet_berry_cheesecake", "chocolate_pie", "pumpkin_pie"];
        int[] m_top = new int[4];
        int[] m_inner = new int[4];
        int m_side;
        int m_bottom;

        public override void Initialize() {
            base.Initialize();
            for (int i = 0; i < 4; i++) {
                m_top[i] = FDAtlas.Slot($"{Types[i]}_top");
                m_inner[i] = FDAtlas.Slot($"{Types[i]}_inner");
            }
            m_side = FDAtlas.Slot("pie_side");
            m_bottom = FDAtlas.Slot("pie_bottom");
            DefaultDisplayName = "Pie";
            DefaultCategory = "Food";
            DefaultIsInteractive = true;
        }

        public static int GetType(int data) => data & 3;
        public static int GetServings(int data) => (data >> 4) & 15;
        public static int MakeData(int type, int servings) => (type & 3) | ((servings & 15) << 4);

        public override int GetFaceTextureSlot(int face, int value) {
            int data = Terrain.ExtractData(value);
            int type = GetType(data);
            int servings = GetServings(data);
            if (face == 4) {
                return servings > 1 ? m_top[type] : m_inner[type];
            }
            if (face == 5) {
                return m_bottom;
            }
            return m_side;
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < 4; i++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, MakeData(i, 4));
            }
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            if (GetServings(data) > 0) {
                dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, data), Count = 1 });
            }
            showDebris = true;
        }
    }

    /// <summary>
    /// Feast block. Data low 3 bits = feast type, bits 4..7 = remaining servings.
    /// </summary>
    public class FDFeastBlock : FDCubeBlockBase {
        protected override int GetDisplayData(int data) => FDFeastBlock.GetType(data);
        public static readonly string[] Types = ["roast_chicken", "stuffed_pumpkin", "honey_glazed_ham", "shepherds_pie", "gleaming_salad", "rice_roll_medley"];
        int[] m_top = new int[6];
        int[] m_side = new int[6];

        public override void Initialize() {
            base.Initialize();
            m_top[0] = FDAtlas.Slot("roast_chicken");
            m_side[0] = FDAtlas.Slot("roast_chicken_details");
            m_top[1] = FDAtlas.Slot("stuffed_pumpkin_top");
            m_side[1] = FDAtlas.Slot("stuffed_pumpkin_side");
            m_top[2] = FDAtlas.Slot("honey_glazed_ham");
            m_side[2] = FDAtlas.Slot("honey_glazed_ham_side_dish");
            m_top[3] = FDAtlas.Slot("shepherds_pie_top");
            m_side[3] = FDAtlas.Slot("shepherds_pie_side");
            m_top[4] = FDAtlas.Slot("gleaming_salad");
            m_side[4] = FDAtlas.Slot("gleaming_salad_details");
            m_top[5] = FDAtlas.Slot("rice_roll_medley");
            m_side[5] = m_top[5];
            DefaultDisplayName = "Feast";
            DefaultCategory = "Food";
            DefaultIsInteractive = true;
        }

        public static int GetType(int data) => data & 7;
        public static int GetServings(int data) => (data >> 4) & 15;
        public static int MakeData(int type, int servings) => (type & 7) | ((servings & 15) << 4);

        public override int GetFaceTextureSlot(int face, int value) {
            int data = Terrain.ExtractData(value);
            int type = Math.Clamp(GetType(data), 0, 5);
            return face == 4 ? m_top[type] : m_side[type];
        }

        public override IEnumerable<int> GetCreativeValues() {
            for (int i = 0; i < 6; i++) {
                yield return Terrain.MakeBlockValue(BlockIndex, 0, MakeData(i, 4));
            }
        }

        public override void GetDropValues(SubsystemTerrain subsystemTerrain,
            int oldValue,
            int newValue,
            int toolLevel,
            List<BlockDropValue> dropValues,
            out bool showDebris) {
            int data = Terrain.ExtractData(oldValue);
            if (GetServings(data) > 0) {
                dropValues.Add(new BlockDropValue { Value = Terrain.MakeBlockValue(BlockIndex, 0, data), Count = 1 });
            }
            showDebris = true;
        }
    }

    /// <summary>
    /// Canvas sign with 17 colours and four mount styles, encoded in one block:
    /// bits 0..4 colour, bits 5..6 mount (standing / wall / hanging / wall hanging).
    /// </summary>
    public class FDCanvasSignBlock : FDCrossBlockBase {
        public const int MountStanding = 0;
        public const int MountWall = 1;
        public const int MountHanging = 2;
        public const int MountWallHanging = 3;
        public static readonly string[] Colors = ["", "white", "orange", "magenta", "light_blue", "yellow", "lime", "pink", "gray", "light_gray", "cyan", "purple", "blue", "brown", "green", "red", "black"];
        int[] m_textures = new int[17];

        public static int GetColor(int data) => data & 31;
        public static int GetMount(int data) => (data >> 5) & 3;
        public static int MakeData(int color, int mount) => (color & 31) | ((mount & 3) << 5);

        public override void Initialize() {
            base.Initialize();
            m_textures[0] = FDAtlas.Slot("canvas_sign");
            for (int i = 1; i < Colors.Length; i++) {
                m_textures[i] = FDAtlas.Slot($"{Colors[i]}_canvas_sign");
            }
            DefaultDisplayName = "Canvas Sign";
            DefaultCategory = "Construction";
        }

        protected override int GetDisplayData(int data) => GetColor(data);

        public override string GetDisplayName(SubsystemTerrain subsystemTerrain, int value) {
            int data = Terrain.ExtractData(value);
            int color = Math.Clamp(GetColor(data), 0, 16);
            string key = $"{GetType().Name}:{color}";
            string name = LanguageControl.TryGetBlock(key, "DisplayName", out string display) ? display : DefaultDisplayName;
            return GetMount(data) switch {
                MountWall => name + " (Wall)",
                MountHanging => name + " (Hanging)",
                MountWallHanging => name + " (Wall Hanging)",
                _ => name
            };
        }

        public override int GetFaceTextureSlot(int face, int value) => m_textures[Math.Clamp(GetColor(Terrain.ExtractData(value)), 0, 16)];

        public override IEnumerable<int> GetCreativeValues() {
            for (int mount = 0; mount < 4; mount++) {
                for (int color = 0; color < 17; color++) {
                    yield return Terrain.MakeBlockValue(BlockIndex, 0, MakeData(color, mount));
                }
            }
        }

        public override BlockPlacementData GetPlacementValue(SubsystemTerrain subsystemTerrain,
            ComponentMiner componentMiner,
            int value,
            TerrainRaycastResult raycastResult) {
            int color = GetColor(Terrain.ExtractData(value));
            int mount = raycastResult.CellFace.Face switch {
                5 => MountHanging,
                >= 0 and <= 3 => MountWall,
                _ => MountStanding
            };
            BlockPlacementData result = default;
            result.Value = Terrain.MakeBlockValue(BlockIndex, 0, MakeData(color, mount));
            result.CellFace = raycastResult.CellFace;
            return result;
        }
    }
}

