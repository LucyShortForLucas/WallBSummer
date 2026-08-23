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
                colors[i++] = GetGradientColor(Color.lawnGreen, 0.4f, 0.7f, fertility/info.maxFertility);
            }
        }
    }

    public class BuildStrat : IGridWorldSolidColorTileOverlayStrategy
    {
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

            RectInt overlap = GetOverlap(world.BuildAttemptRect, chunkBounds);

            overlap.x -= chunkBounds.x;
            overlap.y -= chunkBounds.y;

            for (int y = 0; y < GridWorld.CHUNK_SIZE; ++y)
            {
                for (int x = 0; x < GridWorld.CHUNK_SIZE; ++x)
                {
                    int id = x + y * GridWorld.CHUNK_SIZE;
                    Color32 buildableColor = Color.limeGreen;
                    buildableColor.a = 100;

                    colors[id] = overlap.Contains(new Vector2Int(x, y)) ? buildableColor : new Color32(0, 0, 0, 0);
                }
            }
        }
    }

    public enum StrategyName
    {
        None,
        Fertility,
        WaterContent,
        Building
    }

    public static readonly Dictionary<StrategyName, IGridWorldSolidColorTileOverlayStrategy> strategies =
        new(new KeyValuePair<StrategyName, IGridWorldSolidColorTileOverlayStrategy>[]
        {
            new(StrategyName.None, new NoStrat()),
            new(StrategyName.Fertility, new FertilityStrat()),
            new(StrategyName.Building, new BuildStrat())
        });
}