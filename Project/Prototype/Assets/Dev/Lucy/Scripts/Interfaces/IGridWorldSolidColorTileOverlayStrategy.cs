using UnityEngine;

public interface IGridWorldSolidColorTileOverlayStrategy
{
    public void UpdateColors(GridWorld world, Vector2Int chunkCoord, out Color32[] colors);
}
