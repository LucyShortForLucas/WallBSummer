using System.Collections.Generic;
using UnityEngine;

public class GrassRenderer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Mesh grassMesh;
    [SerializeField] private Material grassMaterial;
    [SerializeField] private GridGenerator gridGenerator;

    [Header("Grass Settings")]
    [SerializeField] private float density = 0.5f;
    [SerializeField] private float minScale = 0.5f;
    [SerializeField] private float maxScale = 1.5f;

    private GrassSpawner spawner;

    private Dictionary<Vector2Int, Matrix4x4[]> chunkMatrices =
        new Dictionary<Vector2Int, Matrix4x4[]>();

    private const int MaxInstancesPerBatch = 1023;

    private void Awake()
    {
        spawner = new GrassSpawner(
            density,
            minScale,
            maxScale
        );
    }

    private void OnEnable()
    {
        if (gridGenerator != null)
        {
            gridGenerator.ChunkCreated += OnChunkCreated;
        }
    }

    private void OnDisable()
    {
        if (gridGenerator != null)
        {
            gridGenerator.ChunkCreated -= OnChunkCreated;
        }
    }

    private void OnChunkCreated(Chunk chunk)
    {
        GenerateGrassForChunk(chunk);
    }

    private void GenerateGrassForChunk(Chunk chunk)
    {
        Vector2Int chunkPosition =
            chunk.chunkPosition;

        // Generate only this chunk's instances.
        spawner.Generate(
            chunk,
            gridGenerator.Chunks
        );

        Matrix4x4[] matrices =
            BuildMatrices(spawner.Instances);

        // Store the result.
        chunkMatrices[chunkPosition] =
            matrices;

        Debug.Log(
            $"Generated {matrices.Length} grass instances " +
            $"for chunk {chunkPosition}."
        );
    }

    private Matrix4x4[] BuildMatrices(
        IReadOnlyList<GrassInstance> instances
    )
    {
        Matrix4x4[] matrices =
            new Matrix4x4[instances.Count];

        for (int i = 0; i < instances.Count; i++)
        {
            GrassInstance instance =
                instances[i];

            Quaternion rotation =
                Quaternion.Euler(
                    0f,
                    instance.rotation,
                    0f
                );

            Vector3 scale =
                Vector3.one * instance.scale;

            matrices[i] =
                Matrix4x4.TRS(
                    instance.position,
                    rotation,
                    scale
                );
        }

        return matrices;
    }

    private void Update()
    {
        DrawGrass();
    }

    private void DrawGrass()
    {
        foreach (
            KeyValuePair<Vector2Int, Matrix4x4[]> pair
            in chunkMatrices
        )
        {
            Matrix4x4[] matrices =
                pair.Value;

            DrawChunkGrass(matrices);
        }
    }

    private void DrawChunkGrass(
        Matrix4x4[] matrices
    )
    {
        for (
            int start = 0;
            start < matrices.Length;
            start += MaxInstancesPerBatch
        )
        {
            int count = Mathf.Min(
                MaxInstancesPerBatch,
                matrices.Length - start
            );

            Matrix4x4[] batch =
                new Matrix4x4[count];

            System.Array.Copy(
                matrices,
                start,
                batch,
                0,
                count
            );

            Graphics.DrawMeshInstanced(
                grassMesh,
                0,
                grassMaterial,
                batch
            );
        }
    }
}