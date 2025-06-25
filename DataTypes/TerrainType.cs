using System;

namespace mtw3dviewer.DataTypes
{
    public enum TerrainType
    {
        LUSH = 1,
        ARID = 2,
        ROCK_DES = 4,
        TEMPERATE = 8
    }

    public static class TerrainTypeExtensions
    {
        public static string ToTextureFolder(this TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.LUSH:
                    return "Lush";
                case TerrainType.ARID:
                    return "Arid";
                case TerrainType.ROCK_DES:
                    return "RockyDesert";
                case TerrainType.TEMPERATE:
                    return "Lush";
            }
            throw new NotImplementedException("Invalid terraintype");
        }
    }
}