using System;
using System.Collections.Generic;
using UnityEngine;

public class GridGenerator : MonoBehaviour
{
    public GameObject player;
    public Material terrainMaterial;
    [SerializeField] private Vector3 startPosition;
    public Vector3 StartPosition => startPosition;
    private int chunkSize = 16;
    private Dictionary<Vector2Int, Chunk> chunks = new Dictionary<Vector2Int, Chunk>();
    private Dictionary<Vector2Int, GameObject> chunkRenderers = new Dictionary<Vector2Int, GameObject>();
    public IReadOnlyDictionary<Vector2Int, Chunk> Chunks => chunks;

    Vector2Int GetPlayerChunk()
    {
        int chunkX = Mathf.FloorToInt(
            (player.transform.position.x
                - startPosition.x
                + chunkSize * 0.5f)
            / chunkSize
        );

        int chunkZ = Mathf.FloorToInt(
            (player.transform.position.z
                - startPosition.z
                + chunkSize * 0.5f)
            / chunkSize
        );

        return new Vector2Int(chunkX, chunkZ);
    }

    void CreateDefaultChunk(Chunk chunk)
    {
        for (int x = 0; x < chunkSize; x++)
        {
            for (int z = 0; z < chunkSize; z++)
            {
                int worldX =
                    chunk.chunkPosition.x * chunkSize + x;

                int worldZ =
                    chunk.chunkPosition.y * chunkSize + z;

                Vector2Int worldPosition =
                    new Vector2Int(worldX, worldZ);

                chunk.cells[x, z] =
                    new Cell(
                        worldPosition,
                        0.5f
                    );
            }
        }
    }

    void LoadChunk(Vector2Int chunkPosition)
    {
        if (chunks.ContainsKey(chunkPosition))
        {
            return;
        }

        Chunk chunk = new Chunk(
            chunkPosition,
            chunkSize
        );

        // Try to load data from CSV
        bool loaded = ChunkDataLoader.TryLoadChunk(
            chunkPosition,
            chunkSize,
            chunk
        );

        if (!loaded)
        {
            Debug.Log(
                $"No CSV found for chunk {chunkPosition}. " +
                "Generating default chunk."
            );

            CreateDefaultChunk(chunk);
        }

        chunks.Add(chunkPosition, chunk);
        ChunkCreated?.Invoke(chunk);
    }

    void CreateTerrainRenderer(Chunk chunk, string id)
    {
        GameObject chunkObject = new GameObject(id);
        TerrainChunkRenderer renderer = chunkObject.AddComponent<TerrainChunkRenderer>();
        renderer.Initialize(
            chunk,
            terrainMaterial,
            chunks
        );
        renderer.transform.position = new Vector3(
            chunk.chunkPosition.x * chunkSize + startPosition.x,
            startPosition.y,
            chunk.chunkPosition.y * chunkSize + startPosition.z
        );
        chunkRenderers.Add(chunk.chunkPosition, chunkObject);
    }

    void LoadSurroundingChunks(Vector2Int playerChunk)
    {
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                Vector2Int chunkPosition = new Vector2Int(
                    playerChunk.x + x,
                    playerChunk.y + z
                );

                if (!chunks.ContainsKey(chunkPosition))
                {
                    LoadChunk(chunkPosition);
                }
            }
        }

        // Load chunks before rendering so neighboring data is available to sample
        UpdateTerrainRenderers();
    }

    void UpdateTerrainRenderers()
    {
        foreach (var pair in chunks)
        {
            Vector2Int position = pair.Key;
            Chunk chunk = pair.Value;

            if (!chunkRenderers.ContainsKey(position))
            {
                CreateTerrainRenderer(
                    chunk,
                    $"Chunk_{position.x}_{position.y}"
                );
            }
        }
    }

    private Vector2Int previousPlayerChunk;
    public event Action<Chunk> ChunkCreated;

    void Start()
    {
        // initialize grid where the player position is
        startPosition = new Vector3(player.transform.position.x, 0f, player.transform.position.z);
        
        previousPlayerChunk = GetPlayerChunk();
        LoadSurroundingChunks(previousPlayerChunk);
    }


    void Update()
    {
        Vector2Int currentPlayerChunk =
            GetPlayerChunk();

        if (currentPlayerChunk != previousPlayerChunk)
        {
            previousPlayerChunk = currentPlayerChunk;
            LoadSurroundingChunks(currentPlayerChunk);
        }
    }
}
