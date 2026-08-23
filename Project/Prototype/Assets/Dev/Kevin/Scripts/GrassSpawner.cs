using System.Collections.Generic;
using UnityEngine;

public class GrassSpawner
{
    private readonly float density;
    private readonly float minScale;
    private readonly float maxScale;

    private readonly List<GrassInstance> instances =
        new List<GrassInstance>();

    public IReadOnlyList<GrassInstance> Instances =>
        instances;

    public GrassSpawner(
        float density,
        float minScale,
        float maxScale
    )
    {
        this.density = density;
        this.minScale = minScale;
        this.maxScale = maxScale;
    }

    public void Generate(
        Chunk chunk,
        IReadOnlyDictionary<Vector2Int, Chunk> chunks
    )
    {
        instances.Clear();

        int chunkSize =
            chunk.cells.GetLength(0);

        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                Cell cell =
                    chunk.cells[x, z];

                float fertility =
                    Mathf.Clamp01(
                        cell.fertility
                    );

                float spawnProbability =
                    fertility * density;

                if (
                    Random.value >
                    spawnProbability
                )
                {
                    continue;
                }

                Vector3 position =
                    new Vector3(
                        cell.worldPosition.x,
                        0f,
                        cell.worldPosition.y
                    );

                float scale =
                    Mathf.Lerp(
                        minScale,
                        maxScale,
                        fertility
                    );

                float rotation =
                    Random.Range(
                        0f,
                        360f
                    );

                instances.Add(
                    new GrassInstance(
                        position,
                        scale,
                        rotation,
                        fertility
                    )
                );
            }
        }
    }
}