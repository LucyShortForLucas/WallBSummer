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

        SetupMesh(chunk);
        GenerateFertilityTexture(chunk, chunks);
        ApplyFertilityTexture();
    }

    private static Mesh terrainQuad;

    private static Mesh GetTerrainQuad()
    {
        if (terrainQuad != null)
            return terrainQuad;

        GameObject temp = GameObject.CreatePrimitive(PrimitiveType.Quad);

        terrainQuad = temp.GetComponent<MeshFilter>().sharedMesh;
        terrainQuad.name = "Terrain Quad";

        Destroy(temp);

        return terrainQuad;
    }

    private void SetupMesh(Chunk chunk)
    {
        int chunkSize = chunk.cells.GetLength(0);
        Mesh mesh = GetTerrainQuad();
        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = mesh;
        transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        transform.localScale = new Vector3(chunkSize, chunkSize, 1f);
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