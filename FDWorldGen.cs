using System;
using Engine;

namespace Game {
    /// <summary>Places FD wild crops on newly generated chunks.</summary>
    public static class FDWorldGen {
        public const double WildCropChance = 0.008;

        public static void Generate(TerrainChunk chunk) {
            int wild = BlocksManager.GetBlockIndex<FDWildCropBlock>();
            if (wild < 0) {
                return;
            }
            int seed = unchecked(chunk.Coords.X * 73856093 ^ chunk.Coords.Y * 19349663 ^ 0x464D);
            System.Random random = new(seed);
            for (int x = 0; x < TerrainChunk.Size; x++) {
                for (int z = 0; z < TerrainChunk.Size; z++) {
                    if (random.NextDouble() > WildCropChance) {
                        continue;
                    }
                    int top = chunk.CalculateTopmostCellHeight(x, z);
                    if (top < 1 || top >= TerrainChunk.HeightMinusOne) {
                        continue;
                    }
                    if (chunk.GetCellValueFast(x, top + 1, z) != 0) {
                        continue;
                    }
                    int underValue = chunk.GetCellValueFast(x, top, z);
                    int underContents = Terrain.ExtractContents(underValue);
                    Block under = BlocksManager.Blocks[underContents];
                    int type;
                    if (under is SandBlock) {
                        type = 7; // sandy shrub
                    }
                    else if (under is GrassBlock || under is DirtBlock || under is SoilBlock) {
                        type = HasWaterNearby(chunk, x, top, z) ? 6 : random.Next(0, 6);
                    }
                    else {
                        continue;
                    }
                    chunk.SetCellValueFast(x, top + 1, z, Terrain.MakeBlockValue(wild, 0, type));
                }
            }
        }

        static bool HasWaterNearby(TerrainChunk chunk, int x, int y, int z) {
            for (int dx = -3; dx <= 3; dx++) {
                for (int dz = -3; dz <= 3; dz++) {
                    int value = chunk.GetCellValueFast(Math.Clamp(x + dx, 0, 15), Math.Clamp(y, 0, 255), Math.Clamp(z + dz, 0, 15));
                    if (Terrain.ExtractContents(value) == WaterBlock.Index) {
                        return true;
                    }
                }
            }
            return false;
        }
    }
}
