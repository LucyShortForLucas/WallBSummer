using System;
using System.Collections.Generic;
using UnityEngine;

public static class GridWorldSolidColorTileOverlayStrategies
{
    // ------ Helper methods --------------------
    public static Color32 GetGradientColor(Color targetColor, float minAlpha, float maxAlpha, float t)
    {
        t = Mathf.Clamp01(t);

        Color result = Color.Lerp(Color.white, targetColor, t);
        result.a = Mathf.Lerp(minAlpha, maxAlpha, t);

        return result; 
    }

    // ------ Strategies ------------------------
    public class NoStrat : IGridWorldSolidColorTileOverlayStrategy
    {
        public void UpdateColors(GridWorld world, Vector2Int chunkCoord, out Color32[] colors)
        {
            colors = new Color32[GridWorld.CHUNK_DATA_SIZE];
        }
    }

    public class FertilityStrat : IGridWorldSolidColorTileOverlayStrategy
    {
        public void UpdateColors(GridWorld world, Vector2Int chunkCoord, out Color32[] colors)
        {
            GridWorldInfo info = GridWorld.INFO;
            int[,] fertilityChunk = world.GetFertilityChunk(chunkCoord);
            colors = new Color32[GridWorld.CHUNK_DATA_SIZE];

            int i = 0;
            foreach(int fertility in fertilityChunk)
            {
                colors[i++] = GetGradientColor(Color.lawnGreen, 0.1f, 0.4f, (float)fertility/(float)info.maxFertility);
            }
        }
    }

    public class BuildStrat : IGridWorldSolidColorTileOverlayStrategy
    {
        // ---- Build system state
        private RectInt _buildAttemptRect = new();
        public RectInt BuildAttemptRect { get => _buildAttemptRect; set => _buildAttemptRect = value; }
        private bool _buildAllowed = true;
        public bool BuildAllowed { get => _buildAllowed; set { _buildAllowed = value; } }

        public void UpdateColors(GridWorld world, Vector2Int chunkCoord, out Color32[] colors)
        {
            static RectInt GetOverlap(RectInt a, RectInt b)
            {
                int xMin = Mathf.Max(a.xMin, b.xMin);
                int yMin = Mathf.Max(a.yMin, b.yMin);
                int xMax = Mathf.Min(a.xMax, b.xMax);
                int yMax = Mathf.Min(a.yMax, b.yMax);

                if (xMax <= xMin || yMax <= yMin)
                    return new RectInt(0, 0, 0, 0);

                return new RectInt(xMin, yMin, xMax - xMin, yMax - yMin);
            }

            colors = new Color32[GridWorld.CHUNK_DATA_SIZE];

            RectInt chunkBounds = new(GridWorld.ChunkToTile(chunkCoord), new Vector2Int(GridWorld.CHUNK_SIZE, GridWorld.CHUNK_SIZE));

            RectInt overlap = GetOverlap(_buildAttemptRect, chunkBounds);

            Color32 buildableColor = _buildAllowed ?  Color.limeGreen : Color.red;
            buildableColor.a = 100;

            overlap.x -= chunkBounds.x;
            overlap.y -= chunkBounds.y;

            var obstructions = world.GetBuildObstructionTypeChunk(chunkCoord);

            for (int y = 0; y < GridWorld.CHUNK_SIZE; ++y)
            {
                for (int x = 0; x < GridWorld.CHUNK_SIZE; ++x)
                {
                    var obstructionColor = new Color32(0, 0, 0, 0);

                    switch (obstructions[y, x])
                    {
                        case GridWorld.BuildObstructionType.Natural:
                            Color32 grey = Color.black;
                            grey.a = 50;
                            obstructionColor =  grey;
                            break;

                        case GridWorld.BuildObstructionType.Building:
                            Color32 yellow = Color.yellow;
                            yellow.a = 50;
                            obstructionColor = yellow;
                            break;
                    }

                    int id = x + y * GridWorld.CHUNK_SIZE;
                    colors[id] =  overlap.Contains(new Vector2Int(x, y)) ? buildableColor : obstructionColor;
                }
            }
        }
    }

    class PrototypeStrategy : IGridWorldSolidColorTileOverlayStrategy
    {
        public void UpdateColors(GridWorld world, Vector2Int chunkCoord, out Color32[] colors)
        {
            GridWorldInfo info = GridWorld.INFO;
            int[,] fertilityChunk = world.GetFertilityChunk(chunkCoord);
            GridWorld.WaterTileType[,] waterTypeChunk = world.GetWaterTypeChunk(chunkCoord);
            colors = new Color32[GridWorld.CHUNK_DATA_SIZE];

            for (int y = 0; y < GridWorld.CHUNK_SIZE; ++y)
            {
                for (int x = 0; x < GridWorld.CHUNK_SIZE; ++x)
                {
                    int fertility = fertilityChunk[y, x];
                    int ci = x + y * GridWorld.CHUNK_SIZE;
                    GridWorld.WaterTileType waterType = waterTypeChunk[y, x];

                    colors[ci] = waterType == GridWorld.WaterTileType.WaterSource ? Color.lightBlue : GetGradientColor(Color.lawnGreen, 0f, 1f, (float)fertility / info.maxFertility);
                }
            }
        }
    }

    public enum StrategyName
    {
        None,
        Fertility,
        WaterContent,
        Building,
        Prototype
    }

    public static readonly Dictionary<StrategyName, IGridWorldSolidColorTileOverlayStrategy> strategies =
        new(new KeyValuePair<StrategyName, IGridWorldSolidColorTileOverlayStrategy>[]
        {
            new(StrategyName.None, new NoStrat()),
            new(StrategyName.Fertility, new FertilityStrat()),
            new(StrategyName.Building, new BuildStrat()),
            new(StrategyName.Prototype, new PrototypeStrategy())
        });
}