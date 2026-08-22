using System.Collections.Generic;
using UnityEngine;

public static class GridWorldSolidColorTileOverlayStrategies
{
    public class NoStrat : IGridWorldSolidColorTileOverlayStrategy
    {
        public void UpdateColors(GridWorld world, Vector2Int chunkCoord, out Color32[] colors)
        {
            colors = new Color32[GridWorld.CHUNK_DATA_SIZE];
        }
    }

    public enum StrategyName
    {
        None,
        Fertility,
        WaterContent
    }

    public static readonly Dictionary<StrategyName, IGridWorldSolidColorTileOverlayStrategy> strategies =
        new(new KeyValuePair<StrategyName, IGridWorldSolidColorTileOverlayStrategy>[]
        {
            new(StrategyName.None, new NoStrat())
        });
}