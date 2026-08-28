#nullable enable
using UnityEngine;

public class GridWorldHandler : MonoBehaviour
{

    // ---- Config
    [SerializeField] private float _worldFocalDepth = 20f;


    // ---- Unity object refs
    private Camera? _mainCam;

    // --- API properties
    public Camera? MainCam => _mainCam != null ? _mainCam : _mainCam = Camera.main;
    public float FocalDepth { set => _worldFocalDepth = value; }
    public Vector3 FocalPoint => MainCam.transform.position + MainCam.transform.forward * _worldFocalDepth;
    public Vector2Int FocalTile => GridWorld.PositionToTile(FocalPoint);

    private GridWorld? _world;
    public GridWorld World => _world ??= GridWorld.TestGridWorld();

    private void OnDestroy()
    {
        if (_world == null) return;
        _world.Dispose();
    }

    void LateUpdate()
    {
        World.Update(Time.fixedDeltaTime);
    }
}
