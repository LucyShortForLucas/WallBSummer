using UnityEngine;

public class Cell
{
    public Vector2Int worldPosition;
    public float fertility;
    public GameObject block;

    public Cell(Vector2Int worldPosition, float fertility = 0.5f)
    {
        this.worldPosition = worldPosition;
        this.fertility = fertility;
    }
}