using System;
using System.Collections.Generic;
using Engine;

namespace Game {
    /// <summary>
    /// Name/value bridge between Farmer's Delight's JSON names and Survivalcraft block
    /// values. Used by recipes, drops and world generation.
    /// </summary>
    public static class FDContent {
        static bool s_initialized;
        static Dictionary<int, string> s_valueToName = [];

        public static int ItemIndex => BlocksManager.GetBlockIndex<FDItemBlock>();
        public static int SeedIndex => BlocksManager.GetBlockIndex<FDSeedItemBlock>();
        public static int KnifeIndex => BlocksManager.GetBlockIndex<FDKnifeBlock>();

        public static void Initialize() {
            if (s_initialized) {
                return;
            }
            s_initialized = true;
            FDItemDefs.EnsureLoaded();
            FDBlockBase.EnsureAtlas();
            foreach (FDItemDef def in FDItemDefs.All) {
                if (def.Kind == "item") {
                    s_valueToName[Terrain.MakeBlockValue(ItemIndex, 0, def.Index)] = def.Name;
                }
            }
            for (int i = 0; i < FDSeedItemBlock.SeedNames.Length; i++) {
                s_valueToName[Terrain.MakeBlockValue(SeedIndex, 0, i)] = FDSeedItemBlock.SeedNames[i];
            }
            for (int i = 0; i < FDKnifeBlock.KnifeNames.Length; i++) {
                s_valueToName[Terrain.MakeBlockValue(KnifeIndex, 0, i)] = FDKnifeBlock.KnifeNames[i];
            }
            foreach (Block block in BlocksManager.Blocks) {
                if (block == null || block.BlockIndex < 300) {
                    continue;
                }
                if (block.GetType().Assembly != typeof(FDContent).Assembly) {
                    continue;
                }
                foreach (int value in block.GetCreativeValues()) {
                    string name = GetBlockName(value);
                    if (!string.IsNullOrEmpty(name)) {
                        s_valueToName[value] = name;
                    }
                }
            }
        }

        public static int ItemValue(string name) {
            FDItemDefs.EnsureLoaded();
            FDItemDef def = FDItemDefs.Find(name);
            if (def != null) {
                if (def.Kind == "item") {
                    return Terrain.MakeBlockValue(ItemIndex, 0, def.Index);
                }
                if (def.Kind == "seed") {
                    int seedType = Array.IndexOf(FDSeedItemBlock.SeedNames, name);
                    return seedType >= 0 ? Terrain.MakeBlockValue(SeedIndex, 0, seedType) : 0;
                }
                if (def.Kind == "knife") {
                    int tier = Array.IndexOf(FDKnifeBlock.KnifeNames, name);
                    return tier >= 0 ? Terrain.MakeBlockValue(KnifeIndex, 0, tier) : 0;
                }
            }
            return 0;
        }

        public static int SeedValue(string name) {
            int index = Array.IndexOf(FDSeedItemBlock.SeedNames, name);
            return index >= 0 ? Terrain.MakeBlockValue(SeedIndex, 0, index) : 0;
        }

        public static int GetValue(string name) {
            if (string.IsNullOrEmpty(name)) {
                return 0;
            }
            // Common substitutions for vanilla items that Survivalcraft does not have.
            name = name.Replace("minecraft:", "");
            switch (name) {
                case "bowl": return ItemValue("bowl");
                case "glass_bottle": return ItemValue("glass_bottle");
                case "egg": return Terrain.MakeBlockValue(EggBlock.Index);
                case "milk_bucket": return Terrain.MakeBlockValue(MilkBucketBlock.Index);
                case "wheat": return Terrain.MakeBlockValue(FlourBlock.Index);
                case "sugar": return ItemValue("sugar");
                case "cocoa_beans": return ItemValue("cocoa_beans");
                case "carrot": return ItemValue("carrot");
                case "potato": return ItemValue("potato");
                case "beetroot": return ItemValue("beetroot");
                case "sweet_berries": return ItemValue("sweet_berries");
                case "glow_berries": return ItemValue("glow_berries");
                case "honey_bottle": return ItemValue("honey_bottle");
                case "gold_ingot": return ItemValue("gold_ingot");
                case "gold_nugget": return ItemValue("gold_nugget");
                case "netherite_ingot": return ItemValue("netherite_ingot");
                case "kelp": return ItemValue("kelp");
                case "dried_kelp": return ItemValue("dried_kelp");
                case "bone": return ItemValue("bone");
                case "beef":
                case "porkchop":
                case "mutton": return Terrain.MakeBlockValue(RawMeatBlock.Index);
                case "cooked_beef":
                case "cooked_porkchop":
                case "cooked_mutton": return Terrain.MakeBlockValue(CookedMeatBlock.Index);
                case "chicken": return Terrain.MakeBlockValue(RawBirdBlock.Index);
                case "cooked_chicken": return Terrain.MakeBlockValue(CookedBirdBlock.Index);
                case "cod":
                case "salmon": return Terrain.MakeBlockValue(RawFishBlock.Index);
                case "cooked_cod":
                case "cooked_salmon": return Terrain.MakeBlockValue(CookedFishBlock.Index);
                case "bread": return Terrain.MakeBlockValue(BreadBlock.Index);
                case "apple": return ItemValue("apple");
                case "oak_wood":
                case "wood": return Terrain.MakeBlockValue(OakWoodBlock.Index);
                case "white_flower": return Terrain.MakeBlockValue(WhiteFlowerBlock.Index);
                case "pumpkin": return Terrain.MakeBlockValue(PumpkinBlock.Index);
                case "gravel": return Terrain.MakeBlockValue(GravelBlock.Index);
                case "oak_leaves": return Terrain.MakeBlockValue(OakLeavesBlock.Index);
                case "campfire": return Terrain.MakeBlockValue(CampfireBlock.Index);
                case "dirt": return Terrain.MakeBlockValue(DirtBlock.Index);
                case "chest": return Terrain.MakeBlockValue(ChestBlock.Index);
                case "rotten_meat": return Terrain.MakeBlockValue(RottenMeatBlock.Index);
                case "squid_ink": return ItemValue("squid_ink");
                case "water_bucket": return Terrain.MakeBlockValue(WaterBucketBlock.Index);
                case "ironingot": return Terrain.MakeBlockValue(IronIngotBlock.Index);
                case "diamond": return Terrain.MakeBlockValue(DiamondChunkBlock.Index);
                case "cobblestone": return Terrain.MakeBlockValue(CobblestoneBlock.Index);
                case "planks": return Terrain.MakeBlockValue(PlanksBlock.Index);
                case "stick": return Terrain.MakeBlockValue(StickBlock.Index);
                case "glass": return Terrain.MakeBlockValue(GlassBlock.Index);
                case "brick": return Terrain.MakeBlockValue(BrickBlock.Index);
                case "clay": return Terrain.MakeBlockValue(ClayBlock.Index);
                case "leather": return Terrain.MakeBlockValue(LeatherBlock.Index);
                case "string": return Terrain.MakeBlockValue(StringBlock.Index);
                case "feather": return Terrain.MakeBlockValue(FeatherBlock.Index);
                case "stonechunk": return Terrain.MakeBlockValue(StoneChunkBlock.Index);
                case "canvas": return Terrain.MakeBlockValue(CanvasBlock.Index);
                case "sweet_berry_cheesecake":
                case "apple_pie":
                case "chocolate_pie":
                case "pumpkin_pie":
                    return BlockValue(name);
            }

            FDItemDef def = FDItemDefs.Find(name);
            if (def != null) {
                return ItemValue(name);
            }
            int blockValue = BlockValue(name);
            if (blockValue != 0) {
                return blockValue;
            }
            return 0;
        }

        public static int BlockValue(string name) {
            switch (name) {
                case "stove": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDStoveBlock>());
                case "cooking_pot": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDCookingPotBlock>());
                case "skillet": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDSkilletBlock>());
                case "cutting_board": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDCuttingBoardBlock>());
                case "wooden_basket": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDBasketBlock>(), 0, 0);
                case "bamboo_basket": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDBasketBlock>(), 0, 1);
                case "rope": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDRopeBlock>());
                case "safety_net": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDSafetyNetBlock>());
                case "rope_fence": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDRopeFenceBlock>());
                case "rope_fence_gate": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDRopeFenceGateBlock>());
                case "canvas_rug": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDCanvasRugBlock>());
                case "tatami": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDTatamiBlock>(), 0, 0);
                case "full_tatami_mat": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDTatamiBlock>(), 0, 1);
                case "half_tatami_mat": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDTatamiBlock>(), 0, 2);
                case "organic_compost": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDOrganicCompostBlock>());
                case "rich_soil": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDRichSoilBlock>());
                case "rich_soil_farmland": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDRichSoilFarmlandBlock>());
                case "brown_mushroom_colony": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDMushroomColonyBlock>(), 0, 3 | (3 << 4));
                case "red_mushroom_colony": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDMushroomColonyBlock>(), 0, 1 | (3 << 4));
                case "sandy_shrub": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDSandyShrubBlock>());
                case "cabbage": return ItemValue("cabbage");
                case "tomato": return ItemValue("tomato");
                case "onion": return ItemValue("onion");
                case "rice": return ItemValue("rice");
                case "apple_pie": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDPieBlock>(), 0, FDPieBlock.MakeData(0, 4));
                case "sweet_berry_cheesecake": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDPieBlock>(), 0, FDPieBlock.MakeData(1, 4));
                case "chocolate_pie": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDPieBlock>(), 0, FDPieBlock.MakeData(2, 4));
                case "pumpkin_pie": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDPieBlock>(), 0, FDPieBlock.MakeData(3, 4));
                case "roast_chicken_block": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDFeastBlock>(), 0, FDFeastBlock.MakeData(0, 4));
                case "stuffed_pumpkin_block": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDFeastBlock>(), 0, FDFeastBlock.MakeData(1, 4));
                case "honey_glazed_ham_block": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDFeastBlock>(), 0, FDFeastBlock.MakeData(2, 4));
                case "shepherds_pie_block": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDFeastBlock>(), 0, FDFeastBlock.MakeData(3, 4));
                case "gleaming_salad_block": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDFeastBlock>(), 0, FDFeastBlock.MakeData(4, 4));
                case "rice_roll_medley_block": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDFeastBlock>(), 0, FDFeastBlock.MakeData(5, 4));
                case "wild_cabbages": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDWildCropBlock>(), 0, 0);
                case "wild_onions": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDWildCropBlock>(), 0, 1);
                case "wild_tomatoes": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDWildCropBlock>(), 0, 2);
                case "wild_carrots": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDWildCropBlock>(), 0, 3);
                case "wild_potatoes": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDWildCropBlock>(), 0, 4);
                case "wild_beetroots": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDWildCropBlock>(), 0, 5);
                case "wild_rice": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDWildCropBlock>(), 0, 6);
                case "carrot_crate": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDCrateBlock>(), 0, 0);
                case "potato_crate": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDCrateBlock>(), 0, 1);
                case "beetroot_crate": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDCrateBlock>(), 0, 2);
                case "cabbage_crate": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDCrateBlock>(), 0, 3);
                case "tomato_crate": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDCrateBlock>(), 0, 4);
                case "onion_crate": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDCrateBlock>(), 0, 5);
                case "rice_bale": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDBaleBlock>(), 0, 0);
                case "rice_bag": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDBaleBlock>(), 0, 1);
                case "straw_bale": return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDBaleBlock>(), 0, 2);
            }
            int cabinet = Array.IndexOf(FDCabinetBlock.Woods, name.Replace("_cabinet", ""));
            if (cabinet >= 0 && name.EndsWith("_cabinet")) {
                return Terrain.MakeBlockValue(BlocksManager.GetBlockIndex<FDCabinetBlock>(), 0, cabinet);
            }
            if (name.EndsWith("canvas_sign")) {
                int sign = CanvasSignValue(name);
                if (sign != 0) {
                    return sign;
                }
            }
            return 0;
        }

        static int CanvasSignValue(string name) {
            int mount;
            string colorPart;
            if (name == "canvas_sign") {
                mount = FDCanvasSignBlock.MountStanding;
                colorPart = "";
            }
            else if (name == "canvas_wall_sign") {
                mount = FDCanvasSignBlock.MountWall;
                colorPart = "";
            }
            else if (name == "hanging_canvas_sign") {
                mount = FDCanvasSignBlock.MountHanging;
                colorPart = "";
            }
            else if (name == "wall_hanging_canvas_sign") {
                mount = FDCanvasSignBlock.MountWallHanging;
                colorPart = "";
            }
            else if (name.EndsWith("_wall_hanging_canvas_sign")) {
                mount = FDCanvasSignBlock.MountWallHanging;
                colorPart = name[..^"_wall_hanging_canvas_sign".Length];
            }
            else if (name.EndsWith("_hanging_canvas_sign")) {
                mount = FDCanvasSignBlock.MountHanging;
                colorPart = name[..^"_hanging_canvas_sign".Length];
            }
            else if (name.EndsWith("_canvas_wall_sign")) {
                mount = FDCanvasSignBlock.MountWall;
                colorPart = name[..^"_canvas_wall_sign".Length];
            }
            else if (name.EndsWith("_canvas_sign")) {
                mount = FDCanvasSignBlock.MountStanding;
                colorPart = name[..^"_canvas_sign".Length];
            }
            else {
                return 0;
            }
            int color = string.IsNullOrEmpty(colorPart) ? 0 : Array.IndexOf(FDCanvasSignBlock.Colors, colorPart);
            if (color < 0) {
                return 0;
            }
            int index = BlocksManager.GetBlockIndex<FDCanvasSignBlock>();
            return index < 0 ? 0 : Terrain.MakeBlockValue(index, 0, FDCanvasSignBlock.MakeData(color, mount));
        }

        public static string NameOf(int value) {
            Initialize();
            if (s_valueToName.TryGetValue(value, out string name)) {
                return name;
            }
            return GetBlockName(value) ?? string.Empty;
        }

        public static string GetBlockName(int value) {
            int contents = Terrain.ExtractContents(value);
            if (contents <= 0 || contents >= BlocksManager.Blocks.Length) {
                return null;
            }
            Block block = BlocksManager.Blocks[contents];
            int data = Terrain.ExtractData(value);
            if (block is FDItemBlock) {
                return FDItemDefs.Get(data)?.Name;
            }
            if (block is FDSeedItemBlock) {
                return FDSeedItemBlock.SeedNames[Math.Clamp(data, 0, 3)];
            }
            if (block is FDKnifeBlock) {
                return FDKnifeBlock.KnifeNames[Math.Clamp(data & 15, 0, 4)];
            }
            if (block is FDCabinetBlock) {
                return FDCabinetBlock.Woods[Math.Clamp(data & 15, 0, 10)] + "_cabinet";
            }
            if (block is FDCrateBlock) {
                return FDCrateBlock.Crops[Math.Clamp(data & 15, 0, 5)] + "_crate";
            }
            if (block is FDBaleBlock) {
                return new[] { "rice_bale", "rice_bag", "straw_bale" }[Math.Clamp(data & 15, 0, 2)];
            }
            if (block is FDTatamiBlock) {
                return new[] { "tatami", "full_tatami_mat", "half_tatami_mat" }[Math.Clamp(data & 15, 0, 2)];
            }
            if (block is FDBasketBlock) {
                return (data & 1) == 0 ? "wooden_basket" : "bamboo_basket";
            }
            if (block is FDPieBlock) {
                return new[] { "apple_pie", "sweet_berry_cheesecake", "chocolate_pie", "pumpkin_pie" }[Math.Clamp(FDPieBlock.GetType(data), 0, 3)];
            }
            if (block is FDFeastBlock) {
                return new[] { "roast_chicken_block", "stuffed_pumpkin_block", "honey_glazed_ham_block", "shepherds_pie_block", "gleaming_salad_block", "rice_roll_medley_block" }[Math.Clamp(FDFeastBlock.GetType(data), 0, 5)];
            }
            if (block is FDMushroomColonyBlock) {
                return (data & 1) == 0 ? "brown_mushroom_colony" : "red_mushroom_colony";
            }
            if (block is FDCropBlock) {
                return new[] { "cabbages", "onions", "tomatoes", "rice" }[Math.Clamp(FDCropBlock.GetType(data), 0, 3)];
            }
            if (block is FDWildCropBlock) {
                return new[] { "wild_cabbages", "wild_onions", "wild_tomatoes", "wild_carrots", "wild_potatoes", "wild_beetroots", "wild_rice", "sandy_shrub" }[Math.Clamp(data & 15, 0, 7)];
            }
            if (block is FDCanvasSignBlock) {
                int color = Math.Clamp(FDCanvasSignBlock.GetColor(data), 0, 16);
                int mount = FDCanvasSignBlock.GetMount(data);
                string prefix = color == 0 ? "" : FDCanvasSignBlock.Colors[color] + "_";
                string suffix = mount switch {
                    FDCanvasSignBlock.MountWall => "canvas_wall_sign",
                    FDCanvasSignBlock.MountHanging => "hanging_canvas_sign",
                    FDCanvasSignBlock.MountWallHanging => "wall_hanging_canvas_sign",
                    _ => "canvas_sign"
                };
                return prefix + suffix;
            }
            return block.GetType().Name switch {
                nameof(FDStoveBlock) => "stove",
                nameof(FDCookingPotBlock) => "cooking_pot",
                nameof(FDSkilletBlock) => "skillet",
                nameof(FDCuttingBoardBlock) => "cutting_board",
                nameof(FDRopeBlock) => "rope",
                nameof(FDSafetyNetBlock) => "safety_net",
                nameof(FDRopeFenceBlock) => "rope_fence",
                nameof(FDRopeFenceGateBlock) => "rope_fence_gate",
                nameof(FDCanvasRugBlock) => "canvas_rug",
                nameof(FDOrganicCompostBlock) => "organic_compost",
                nameof(FDRichSoilBlock) => "rich_soil",
                nameof(FDRichSoilFarmlandBlock) => "rich_soil_farmland",
                nameof(FDSandyShrubBlock) => "sandy_shrub",
                _ => null
            };
        }
    }
}

