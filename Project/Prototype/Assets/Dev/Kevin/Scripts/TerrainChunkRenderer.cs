using System.Collections.Generic;
using UnityEngine;

public class TerrainChunkRenderer : MonoBehaviour
{
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private MaterialPropertyBlock propertyBlock;

    public void Initialize(
        Chunk chunk,
        Material terrainMaterial,
        Dictionary<Vector2Int, Chunk> chunks
    )
    {
        meshFilter = gameObject.AddComponent<MeshFilter>();
        meshRenderer = gameObject.AddComponent<MeshRenderer>();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        meshRenderer.material = terrainMaterial;
        propertyBlock = new MaterialPropertyBlock();

        GenerateMesh(chunk);
        GenerateFertilityTexture(chunk, chunks);
        ApplyFertilityTexture();
    }

    private void GenerateMesh(Chunk chunk)
    {
        int chunkSize = chunk.cells.GetLength(0);

        int verticesPerSide = chunkSize + 1;

        Vector3[] vertices =
            new Vector3[verticesPerSide * verticesPerSide];

        Vector2[] uv =
            new Vector2[vertices.Length];

        int[] triangles =
            new int[chunkSize * chunkSize * 6];

        // Generate vertices
        for (int z = 0; z <= chunkSize; z++)
        {
            for (int x = 0; x <= chunkSize; x++)
            {
                int vertexIndex =
                    z * verticesPerSide + x;

                vertices[vertexIndex] =
                    new Vector3(x, 0, z);

                uv[vertexIndex] =
                    new Vector2(
                        (float)x / chunkSize,
                        (float)z / chunkSize
                    );
            }
        }

        // Generate triangles
        int triangleIndex = 0;

        for (int z = 0; z < chunkSize; z++)
        {
            for (int x = 0; x < chunkSize; x++)
            {
                int bottomLeft =
                    z * verticesPerSide + x;

                int bottomRight =
                    bottomLeft + 1;

                int topLeft =
                    bottomLeft + verticesPerSide;

                int topRight =
                    topLeft + 1;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topLeft;
                triangles[triangleIndex++] = topRight;

                triangles[triangleIndex++] = bottomLeft;
                triangles[triangleIndex++] = topRight;
                triangles[triangleIndex++] = bottomRight;
            }
        }

        Mesh mesh = new Mesh();

        mesh.name = "Terrain Chunk Mesh";

        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        meshFilter.mesh = mesh;
        meshCollider.sharedMesh = mesh;
    }

    public Texture2D fertilityTexture;
    public void GenerateFertilityTexture(
        Chunk chunk,
        Dictionary<Vector2Int, Chunk> chunks
    )
    {
        int chunkSize = chunk.cells.GetLength(0);
        int textureSize = chunkSize + 2; // to fix sampling cut off between chunks

        fertilityTexture = new Texture2D(
            textureSize,
            textureSize,
            TextureFormat.R8,
            false
        )
        {
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp
        };

        for (int textureX = 0; textureX < textureSize; textureX++)
        {
            for (int textureZ = 0; textureZ < textureSize; textureZ++)
            {
                int cellX = textureX - 1;
                int cellZ = textureZ - 1;

                float fertility = GetFertility(
                    chunk,
                    chunks,
                    cellX,
                    cellZ
                );

                Color pixel = new Color(
                    fertility,
                    0f,
                    0f,
                    1f
                );

                fertilityTexture.SetPixel(
                    textureX, 
                    textureZ, 
                    pixel
                );
            }
        }

        fertilityTexture.Apply();
    }

    private void ApplyFertilityTexture()
    {
        meshRenderer.GetPropertyBlock(propertyBlock);

        propertyBlock.SetTexture(
            "_FertilityMap",
            fertilityTexture
        );

        meshRenderer.SetPropertyBlock(propertyBlock);
    }

    private float GetFertility(
        Chunk chunk,
        Dictionary<Vector2Int, Chunk> chunks,
        int x,
        int z
    )
    {
        int chunkSize = chunk.cells.GetLength(0);

        // Inside this chunk
        if (
            x >= 0 &&
            x < chunkSize &&
            z >= 0 &&
            z < chunkSize
        )
        {
            return Mathf.Clamp01(
                chunk.cells[x, z].fertility
            );
        }

        int neighborChunkX = chunk.chunkPosition.x;
        int neighborChunkZ = chunk.chunkPosition.y;

        int localX = x;
        int localZ = z;

        // Left
        if (x < 0)
        {
            neighborChunkX--;
            localX = x + chunkSize;
        }

        // Right
        else if (x >= chunkSize)
        {
            neighborChunkX++;
            localX = x - chunkSize;
        }

        // Bottom
        if (z < 0)
        {
            neighborChunkZ--;
            localZ = z + chunkSize;
        }

        // Top
        else if (z >= chunkSize)
        {
            neighborChunkZ++;
            localZ = z - chunkSize;
        }

        Vector2Int neighborPosition =
            new Vector2Int(
                neighborChunkX,
                neighborChunkZ
            );

        if (chunks.TryGetValue(
            neighborPosition,
            out Chunk neighborChunk
        ))
        {
            return Mathf.Clamp01(
                neighborChunk.cells[
                    localX,
                    localZ
                ].fertility
            );
        }

        // Neighbor isn't loaded.
        // For now, duplicate the closest edge value.
        int clampedX =
            Mathf.Clamp(x, 0, chunkSize - 1);

        int clampedZ =
            Mathf.Clamp(z, 0, chunkSize - 1);

        return Mathf.Clamp01(
            chunk.cells[
                clampedX,
                clampedZ
            ].fertility
        );
    }
}