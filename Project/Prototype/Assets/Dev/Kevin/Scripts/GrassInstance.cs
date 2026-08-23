using UnityEngine;

public struct GrassInstance
{
    public Vector3 position;
    public float scale;
    public float rotation;
    public float fertility;

    public GrassInstance(
        Vector3 position,
        float scale,
        float rotation,
        float fertility)
    {
        this.position = position;
        this.scale = scale;
        this.rotation = rotation;
        this.fertility = fertility;
    }
}