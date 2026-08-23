using System.IO;
using UnityEngine;

public static class ChunkDataLoader
{
    public static bool TryLoadChunk(
        Vector2Int chunkPosition,
        int chunkSize,
        Chunk chunk)
    {
        string fileName =
            $"chunk_{chunkPosition.x}_{chunkPosition.y}.csv";

        string path = Path.Combine(
            Application.streamingAssetsPath,
            "Chunks",
            fileName
        );

        // CSV doesn't exist
        if (!File.Exists(path))
        {
            return false;
        }

        string[] lines = File.ReadAllLines(path);

        // Make sure we have 16 rows
        if (lines.Length != chunkSize)
        {
            Debug.LogWarning(
                $"Invalid chunk file: {fileName}. " +
                $"Expected {chunkSize} rows."
            );

            return false;
        }

        for (int z = 0; z < chunkSize; z++)
        {
            string[] values = lines[z].Split(',');

            // Make sure we have 16 columns
            if (values.Length != chunkSize)
            {
                Debug.LogWarning(
                    $"Invalid row in {fileName}. " +
                    $"Expected {chunkSize} values."
                );

                return false;
            }

            for (int x = 0; x < chunkSize; x++)
            {
                if (!float.TryParse(values[x], out float value))
                {
                    Debug.LogWarning(
                        $"Invalid cell value in {fileName}: {values[x]}"
                    );

                    return false;
                }

                float fertility = value;

                int worldX =
                    chunkPosition.x * chunkSize + x;

                int worldZ =
                    chunkPosition.y * chunkSize + z;

                Vector2Int worldPosition =
                    new Vector2Int(worldX, worldZ);

                chunk.cells[x, z] =
                    new Cell(worldPosition, fertility);
            }
        }

        return true;
    }
}