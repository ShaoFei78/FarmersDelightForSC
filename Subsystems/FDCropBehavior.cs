using System;
using Engine;
using TemplatesDatabase;

namespace Game {
    /// <summary>Growth, lifecycle and neighbor checks for FD crops and mushroom colonies.</summary>
    public class SubsystemFDCropBehavior : SubsystemPollableBlockBehavior {
        public SubsystemCellChangeQueue m_subsystemCellChangeQueue;
        public SubsystemGameInfo m_subsystemGameInfo;
        public Random m_random = new();

        public override int[] HandledBlocks {
            get {
                int[] values = [
                    BlocksManager.GetBlockIndex<FDCropBlock>(),
                    BlocksManager.GetBlockIndex<FDMushroomColonyBlock>()
                ];
                System.Collections.Generic.List<int> result = [];
                foreach (int value in values) {
                    if (value >= 0 && !result.Contains(value)) {
                        result.Add(value);
                    }
                }
                return result.ToArray();
            }
        }

        public override void Load(ValuesDictionary valuesDictionary) {
            base.Load(valuesDictionary);
            m_subsystemCellChangeQueue = Project.FindSubsystem<SubsystemCellChangeQueue>(true);
            m_subsystemGameInfo = Project.FindSubsystem<SubsystemGameInfo>(true);
        }

        public override void OnPoll(int value, int x, int y, int z, int pollPass) {
            if (m_subsystemGameInfo.WorldSettings.EnvironmentBehaviorMode != EnvironmentBehaviorMode.Living) {
                return;
            }
            int contents = Terrain.ExtractContents(value);
            int data = Terrain.ExtractData(value);
            if (contents == BlocksManager.GetBlockIndex<FDCropBlock>()) {
                GrowCrop(value, data, x, y, z, pollPass);
            }
            else if (contents == BlocksManager.GetBlockIndex<FDMushroomColonyBlock>()) {
                GrowColony(value, data, x, y, z, pollPass);
            }
        }

        void GrowCrop(int value, int data, int x, int y, int z, int pollPass) {
            int type = FDCropBlock.GetType(data);
            int stage = FDCropBlock.GetStage(data);
            int maxStage = type is 1 or 2 or 3 ? 3 : 7;
            if (stage >= maxStage) {
                return;
            }
            if (SubsystemTerrain.Terrain.GetCellLight(x, y, z) < 9) {
                return;
            }
            if (type == 3 && !HasWaterNearby(x, y, z)) {
                return;
            }
            int belowValue = SubsystemTerrain.Terrain.GetCellValue(x, y - 1, z);
            Block below = BlocksManager.Blocks[Terrain.ExtractContents(belowValue)];
            bool rich = below is FDRichSoilBlock or FDRichSoilFarmlandBlock;
            float chance = rich ? 0.22f : 0.1f;
            if (m_random.Float(0f, 1f) > chance) {
                return;
            }
            int newData = FDCropBlock.MakeData(type, stage + 1);
            m_subsystemCellChangeQueue.QueueCellChange(x, y, z, Terrain.ReplaceData(value, newData));
        }

        void GrowColony(int value, int data, int x, int y, int z, int pollPass) {
            int stage = (data >> 4) & 3;
            if (stage >= 3) {
                return;
            }
            if (m_random.Float(0f, 1f) > 0.08f) {
                return;
            }
            int newData = (data & 1) | ((stage + 1) << 4);
            m_subsystemCellChangeQueue.QueueCellChange(x, y, z, Terrain.ReplaceData(value, newData));
        }

        bool HasWaterNearby(int x, int y, int z) {
            for (int dx = -3; dx <= 3; dx++) {
                for (int dz = -3; dz <= 3; dz++) {
                    for (int dy = -1; dy <= 1; dy++) {
                        int value = SubsystemTerrain.Terrain.GetCellValue(x + dx, y + dy, z + dz);
                        if (Terrain.ExtractContents(value) == WaterBlock.Index) {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        public override void OnNeighborBlockChanged(int x, int y, int z, int neighborX, int neighborY, int neighborZ) {
            int value = SubsystemTerrain.Terrain.GetCellValue(x, y, z);
            int contents = Terrain.ExtractContents(value);
            if (contents != BlocksManager.GetBlockIndex<FDCropBlock>()) {
                return;
            }
            int belowValue = SubsystemTerrain.Terrain.GetCellValue(x, y - 1, z);
            if (!BlocksManager.Blocks[Terrain.ExtractContents(belowValue)].IsSuitableForPlants(belowValue, value)) {
                SubsystemTerrain.DestroyCell(0, x, y, z, 0, false, false);
            }
        }
    }
}
