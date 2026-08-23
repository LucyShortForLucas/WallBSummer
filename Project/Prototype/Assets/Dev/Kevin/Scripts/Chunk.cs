 using UnityEngine;

 public class Chunk
{
    public Vector2Int chunkPosition;
    public Cell[,] cells;

    public Chunk(Vector2Int chunkPosition, int chunkSize)
    {
        this.chunkPosition = chunkPosition;
        cells = new Cell[chunkSize, chunkSize];
    }

}