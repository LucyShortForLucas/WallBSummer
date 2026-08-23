#nullable enable
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildSystem : MonoBehaviour
{
    // ---- Unity object refs
    [SerializeReference] private BuildableDatabase? _buildableDatabase;
    [SerializeReference] private Camera? _camera;
    [SerializeReference] private GameObject? _focus;
    [SerializeReference] private GridWorldHandler? _handler;

    // ---- State
    private int _tryingToPlaceId = 2;

    // ---- Unity methods
    private void Update()
    {
        if (_camera == null || _tryingToPlaceId < 0)
            return;

        Plane plane = new Plane(transform.up, transform.position);
        Ray ray = _camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPoint = ray.GetPoint(distance);
            Vector2 gridPoint = new Vector2(worldPoint.x, worldPoint.z);
            var tile = GridWorld.PositionToTile(gridPoint);
            if (_handler != null && _handler.World != null) _handler.World.BuildAttemptRect = new RectInt(tile, new Vector2Int(2, 2));
        }
    }
}
