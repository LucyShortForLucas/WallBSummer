using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildSystemObstructionFootprint : MonoBehaviour, IInjectable
{
    public static List<Vector2Int> GetFootprintOBB(BoxCollider box)
    {
        Transform t = box.transform;
        Vector3 worldCenter = t.TransformPoint(box.center);
        Vector3 halfExtents = Vector3.Scale(box.size * 0.5f, t.lossyScale);

        Vector3 right = t.right * halfExtents.x;
        Vector3 forward = t.forward * halfExtents.z;

        // Corners in order around the rectangle (XZ plane)
        Vector2[] corners = {
        To2D(worldCenter + right + forward),
        To2D(worldCenter - right + forward),
        To2D(worldCenter - right - forward),
        To2D(worldCenter + right - forward),
    };

        // Candidate range from the AABB of those corners
        Vector2 min = corners[0], max = corners[0];
        foreach (var c in corners)
        {
            min = Vector2.Min(min, c);
            max = Vector2.Max(max, c);
        }

        int minX = Mathf.FloorToInt(min.x);
        int maxX = Mathf.CeilToInt(max.x) - 1;
        int minZ = Mathf.FloorToInt(min.y);
        int maxZ = Mathf.CeilToInt(max.y) - 1;

        var tiles = new List<Vector2Int>();
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Vector2[] tileCorners = {
                new Vector2(x, z),
                new Vector2(x + 1, z),
                new Vector2(x + 1, z + 1),
                new Vector2(x, z + 1),
            };

                if (PolygonsIntersect(corners, tileCorners))
                    tiles.Add(new Vector2Int(x, z));
            }
        }

        return tiles;
    }

    static Vector2 To2D(Vector3 v) => new Vector2(v.x, v.z);

    // Separating Axis Theorem for two convex polygons
    static bool PolygonsIntersect(Vector2[] a, Vector2[] b)
    {
        return !HasSeparatingAxis(a, b) && !HasSeparatingAxis(b, a);
    }

    static bool HasSeparatingAxis(Vector2[] poly, Vector2[] other)
    {
        for (int i = 0; i < poly.Length; i++)
        {
            Vector2 edge = poly[(i + 1) % poly.Length] - poly[i];
            Vector2 axis = new Vector2(-edge.y, edge.x).normalized;

            Project(poly, axis, out float minA, out float maxA);
            Project(other, axis, out float minB, out float maxB);

            if (maxA < minB || maxB < minA)
                return true; // found a gap -> separated, no intersection
        }
        return false;
    }

    static void Project(Vector2[] poly, Vector2 axis, out float min, out float max)
    {
        min = max = Vector2.Dot(poly[0], axis);
        for (int i = 1; i < poly.Length; i++)
        {
            float p = Vector2.Dot(poly[i], axis);
            min = Mathf.Min(min, p);
            max = Mathf.Max(max, p);
        }
    }

    private void Start()
    {
        var box = GetComponent<BoxCollider>();

        var tiles = GetFootprintOBB(box);

        foreach (var tile in tiles)
        {
            _gridWorld.FillBuildObstructionType(new RectInt(tile, Vector2Int.one), GridWorld.BuildObstructionType.Natural);
        }
    }

    private GridWorld _gridWorld;
    public void Inject(DependencyContainer container)
    {
        _gridWorld = container.Get<GridWorldHandler>().World;
    }
}
