using System;
using System.Collections.Generic;
using Engine;
using TemplatesDatabase;

namespace Game {
    /// <summary>
    /// Interaction and block-entity wiring for FD workstations, storage and serving blocks.
    /// </summary>
    public class SubsystemFDBlockBehavior : SubsystemEntityBlockBehavior {
        public SubsystemPickables m_subsystemPickables;
        readonly System.Random m_random = new System.Random();

        public override int[] HandledBlocks {
            get {
                List<int> result = [];
                void Add<T>() where T : Block {
                    int index = BlocksManager.GetBlockIndex<T>();
                    if (index >= 0 && !result.Contains(index)) {
                        result.Add(index);
                    }
                }
                Add<FDCookingPotBlock>();
                Add<FDCuttingBoardBlock>();
                Add<FDBasketBlock>();
                Add<FDCabinetBlock>();
                Add<FDStoveBlock>();
                Add<FDSkilletBlock>();
                Add<FDPieBlock>();
                Add<FDFeastBlock>();
                Add<OakLeavesBlock>();
                return result.ToArray();
            }
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemPickables = Project.FindSubsystem<SubsystemPickables>(true);
        }

        public override void OnBlockAdded(int value, int oldValue, int x, int y, int z) {
            int contents = Terrain.ExtractContents(value);
            string databaseName = null;
            if (contents == BlocksManager.GetBlockIndex<FDCookingPotBlock>()) {
                databaseName = "FDCookingPotEntity";
            }
            else if (contents == BlocksManager.GetBlockIndex<FDCuttingBoardBlock>()) {
                databaseName = "FDCuttingBoardEntity";
            }
            else if (contents == BlocksManager.GetBlockIndex<FDBasketBlock>()
                || contents == BlocksManager.GetBlockIndex<FDCabinetBlock>()) {
                databaseName = "FDContainerEntity";
            }
            if (databaseName == null) {
                return;
            }
            m_databaseObject = Project.GameDatabase.Database.FindDatabaseObject(
                databaseName,
                Project.GameDatabase.EntityTemplateType,
                true
            );
            base.OnBlockAdded(value, oldValue, x, y, z);
        }

        public override void OnBlockRemoved(int value, int newValue, int x, int y, int z) {
            int contents = Terrain.ExtractContents(value);
            if (contents == BlocksManager.GetBlockIndex<OakLeavesBlock>() && newValue == 0) {
                Vector3 position = new(x + 0.5f, y + 0.5f, z + 0.5f);
                if (m_random.Next(0, 5) == 0) {
                    m_subsystemPickables.AddPickable(FDContent.ItemValue("sweet_berries"), 1, position, new Vector3(0f, 2f, 0f), null);
                }
                if (m_random.Next(0, 8) == 0) {
                    m_subsystemPickables.AddPickable(FDContent.ItemValue("apple"), 1, position, new Vector3(0f, 2f, 0f), null);
                }
            }
            if (contents == BlocksManager.GetBlockIndex<FDCuttingBoardBlock>()) {
                ComponentBlockEntity blockEntity = m_subsystemBlockEntities.GetBlockEntity(x, y, z);
                ComponentFDCuttingBoard board = blockEntity?.Entity.FindComponent<ComponentFDCuttingBoard>();
                if (board != null && board.ItemValue != 0) {
                    m_subsystemPickables.AddPickable(
                        board.ItemValue,
                        1,
                        new Vector3(x + 0.5f, y + 0.5f, z + 0.5f),
                        new Vector3(0f, 2f, 0f),
                        null
                    );
                }
            }
            if (contents == BlocksManager.GetBlockIndex<FDCookingPotBlock>()
                || contents == BlocksManager.GetBlockIndex<FDCuttingBoardBlock>()
                || contents == BlocksManager.GetBlockIndex<FDBasketBlock>()
                || contents == BlocksManager.GetBlockIndex<FDCabinetBlock>()) {
                base.OnBlockRemoved(value, newValue, x, y, z);
            }
        }

        public override bool OnInteract(TerrainRaycastResult raycastResult, ComponentMiner componentMiner) {
            int value = SubsystemTerrain.Terrain.GetCellValue(
                raycastResult.CellFace.X,
                raycastResult.CellFace.Y,
                raycastResult.CellFace.Z
            );
            int contents = Terrain.ExtractContents(value);
            if (contents == BlocksManager.GetBlockIndex<FDStoveBlock>()) {
                return InteractStove(raycastResult, componentMiner, value);
            }
            if (contents == BlocksManager.GetBlockIndex<FDSkilletBlock>()) {
                return InteractSkillet(raycastResult, componentMiner);
            }
            if (contents == BlocksManager.GetBlockIndex<FDPieBlock>()) {
                return InteractPie(raycastResult, componentMiner, value);
            }
            if (contents == BlocksManager.GetBlockIndex<FDFeastBlock>()) {
                return InteractFeast(raycastResult, componentMiner, value);
            }
            return base.OnInteract(raycastResult, componentMiner);
        }

        public override bool InteractBlockEntity(ComponentBlockEntity blockEntity, ComponentMiner componentMiner) {
            if (blockEntity == null || componentMiner?.ComponentPlayer == null) {
                return false;
            }
            int value = blockEntity.BlockValue;
            int contents = Terrain.ExtractContents(value);
            if (contents == BlocksManager.GetBlockIndex<FDCookingPotBlock>()) {
                ComponentFDCookingPot pot = blockEntity.Entity.FindComponent<ComponentFDCookingPot>();
                if (pot != null) {
                    componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new FDCookingPotWidget(componentMiner.Inventory, pot);
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                    return true;
                }
            }
            if (contents == BlocksManager.GetBlockIndex<FDBasketBlock>()
                || contents == BlocksManager.GetBlockIndex<FDCabinetBlock>()) {
                ComponentChest chest = blockEntity.Entity.FindComponent<ComponentChest>();
                if (chest != null) {
                    componentMiner.ComponentPlayer.ComponentGui.ModalPanelWidget = new FDStorageWidget(componentMiner.Inventory, chest);
                    AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                    return true;
                }
            }
            if (contents == BlocksManager.GetBlockIndex<FDCuttingBoardBlock>()) {
                return InteractCuttingBoard(blockEntity, componentMiner, value);
            }
            return false;
        }

        bool InteractStove(TerrainRaycastResult raycast, ComponentMiner miner, int value) {
            int active = miner.ActiveBlockValue;
            int data = Terrain.ExtractData(value);
            bool lit = (data & 1) != 0;
            Block activeBlock = BlocksManager.Blocks[Terrain.ExtractContents(active)];
            bool fuel = active != 0 && (activeBlock.GetFuelFireDuration(active) > 0f || activeBlock.GetFuelHeatLevel(active) > 0f);
            if (fuel && !lit) {
                miner.RemoveActiveTool(1);
                SubsystemTerrain.ChangeCell(
                    raycast.CellFace.X,
                    raycast.CellFace.Y,
                    raycast.CellFace.Z,
                    Terrain.ReplaceData(value, data | 1)
                );
                AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                Message(miner, "Stove lit. 炉灶已点燃。");
                return true;
            }
            if (active == 0) {
                if (lit) {
                    SubsystemTerrain.ChangeCell(
                        raycast.CellFace.X,
                        raycast.CellFace.Y,
                        raycast.CellFace.Z,
                        Terrain.ReplaceData(value, data & ~1)
                    );
                    Message(miner, "Stove extinguished. 炉灶已熄灭。");
                }
                else {
                    Message(miner, "Use fuel to light the stove. 手持燃料右击点燃炉灶。");
                }
                return true;
            }
            // A placeable block (cooking pot, skillet, etc.) must fall through to
            // normal placement instead of being swallowed by this interaction.
            if (activeBlock.IsPlaceable_(active)) {
                return false;
            }
            if (lit) {
                Message(miner, "Stove only provides heat. Place a cooking pot or skillet above it first. 炉灶只提供热量，请先把烹饪锅或煎锅放在上方，再对锅放入食物。");
            }
            else {
                Message(miner, "Light the stove with fuel first. 先手持燃料右击点燃炉灶。");
            }
            return true;
        }

        bool InteractSkillet(TerrainRaycastResult raycast, ComponentMiner miner) {
            int below = SubsystemTerrain.Terrain.GetCellValue(raycast.CellFace.X, raycast.CellFace.Y - 1, raycast.CellFace.Z);
            Block belowBlock = BlocksManager.Blocks[Terrain.ExtractContents(below)];
            bool hot = belowBlock is FireBlock or CampfireBlock or MagmaBlock
                || belowBlock is FDStoveBlock && (Terrain.ExtractData(below) & 1) != 0;
            if (!hot) {
                Message(miner, "The skillet needs a heat source below. 煎锅下方需要热源。");
                return true;
            }
            int active = miner.ActiveBlockValue;
            if (active == 0) {
                Message(miner, "Hold food to cook in the skillet. 手持食材右击煎锅。");
                return true;
            }
            if (!FDRecipes.TrySmeltValue(active, out CraftingRecipe cooked)) {
                Message(miner, "This cannot be cooked in a skillet. 这个食材不能煎。");
                return true;
            }
            miner.RemoveActiveTool(1);
            GiveItem(miner, cooked.ResultValue, cooked.ResultCount, raycast);
            AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
            return true;
        }

        bool InteractCuttingBoard(ComponentBlockEntity blockEntity, ComponentMiner miner, int value) {
            ComponentFDCuttingBoard board = blockEntity.Entity.FindComponent<ComponentFDCuttingBoard>();
            if (board == null) {
                return false;
            }
            int active = miner.ActiveBlockValue;
            int activeContents = Terrain.ExtractContents(active);
            if (activeContents == BlocksManager.GetBlockIndex<FDKnifeBlock>()) {
                int tier = Terrain.ExtractData(active) & 15;
                int toolLevel = FDKnifeBlock.ToolLevels[Math.Clamp(tier, 0, 4)];
                if (board.ItemValue == 0) {
                    Message(miner, "Place an ingredient first. 先放上食材。");
                    return true;
                }
                if (!FDCuttingRecipes.TryFind(board.ItemValue, toolLevel, out FDCuttingRecipe recipe)) {
                    Message(miner, "This knife cannot cut that. 这把刀无法处理这个食材。");
                    return true;
                }
                Vector3 position = blockEntity.Position + new Vector3(0.5f, 0.6f, 0.5f);
                for (int i = 0; i < recipe.Results.Length; i++) {
                    if (recipe.Results[i] != 0) {
                        m_subsystemPickables.AddPickable(recipe.Results[i], recipe.Counts[i], position, new Vector3(0f, 1.5f, 0f), null);
                    }
                }
                board.ItemValue = 0;
                miner.DamageActiveTool(1);
                AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                return true;
            }
            if (board.ItemValue != 0) {
                m_subsystemPickables.AddPickable(
                    board.ItemValue,
                    1,
                    blockEntity.Position + new Vector3(0.5f, 0.6f, 0.5f),
                    new Vector3(0f, 1.5f, 0f),
                    null
                );
                board.ItemValue = 0;
                Message(miner, "Removed the ingredient. 已取下食材。");
                return true;
            }
            if (active != 0 && FDCuttingRecipes.HasInput(active)) {
                miner.RemoveActiveTool(1);
                board.ItemValue = active;
                AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
                Message(miner, "Placed on the cutting board. 已放上切菜板。");
                return true;
            }
            Message(miner, "Hold an ingredient to place it. 手持食材右击放置。");
            return true;
        }

        bool InteractPie(TerrainRaycastResult raycast, ComponentMiner miner, int value) {
            int data = Terrain.ExtractData(value);
            int servings = FDPieBlock.GetServings(data);
            if (servings <= 0) {
                return false;
            }
            int type = FDPieBlock.GetType(data);
            string[] slices = ["apple_pie_slice", "sweet_berry_cheesecake_slice", "chocolate_pie_slice", "pumpkin_pie_slice"];
            int sliceValue = FDContent.ItemValue(slices[Math.Clamp(type, 0, 3)]);
            if (!GiveItem(miner, sliceValue, 1, raycast)) {
                return true;
            }
            servings--;
            if (servings <= 0) {
                SubsystemTerrain.ChangeCell(raycast.CellFace.X, raycast.CellFace.Y, raycast.CellFace.Z, 0);
            }
            else {
                SubsystemTerrain.ChangeCell(
                    raycast.CellFace.X,
                    raycast.CellFace.Y,
                    raycast.CellFace.Z,
                    Terrain.ReplaceData(value, FDPieBlock.MakeData(type, servings))
                );
            }
            AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
            return true;
        }

        bool InteractFeast(TerrainRaycastResult raycast, ComponentMiner miner, int value) {
            int data = Terrain.ExtractData(value);
            int servings = FDFeastBlock.GetServings(data);
            if (servings <= 0) {
                return false;
            }
            int type = FDFeastBlock.GetType(data);
            string[] portions = ["roast_chicken", "stuffed_pumpkin", "honey_glazed_ham", "shepherds_pie", "gleaming_salad", "kelp_roll_slice"];
            int portionValue = FDContent.ItemValue(portions[Math.Clamp(type, 0, 5)]);
            if (!GiveItem(miner, portionValue, 1, raycast)) {
                return true;
            }
            servings--;
            if (servings <= 0) {
                SubsystemTerrain.ChangeCell(raycast.CellFace.X, raycast.CellFace.Y, raycast.CellFace.Z, 0);
            }
            else {
                SubsystemTerrain.ChangeCell(
                    raycast.CellFace.X,
                    raycast.CellFace.Y,
                    raycast.CellFace.Z,
                    Terrain.ReplaceData(value, FDFeastBlock.MakeData(type, servings))
                );
            }
            AudioManager.PlaySound("Audio/UI/ButtonClick", 1f, 0f, 0f);
            return true;
        }

        bool GiveItem(ComponentMiner miner, int value, int count, TerrainRaycastResult raycast) {
            if (value == 0) {
                return true;
            }
            int remaining = ComponentInventoryBase.AcquireItems(miner.Inventory, value, count);
            if (remaining > 0) {
                m_subsystemPickables.AddPickable(
                    value,
                    remaining,
                    new Vector3(raycast.CellFace.X + 0.5f, raycast.CellFace.Y + 0.8f, raycast.CellFace.Z + 0.5f),
                    new Vector3(0f, 2f, 0f),
                    null
                );
            }
            return true;
        }

        static void Message(ComponentMiner miner, string text) {
            miner.ComponentPlayer?.ComponentGui.DisplaySmallMessage(text, Color.White, true, false);
        }
    }
}
