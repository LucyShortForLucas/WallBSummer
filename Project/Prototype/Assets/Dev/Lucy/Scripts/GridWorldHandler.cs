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
    public Vector3 FocalPoint => MainCam ? MainCam.transform.position + MainCam.transform.forward * _worldFocalDepth : Vector3.zero;
    public Vector2Int FocalTile => GridWorld.PositionToTile(FocalPoint);

    private GridWorld? _world;
    public GridWorld World => _world ??= GridWorld.PrototyperGridWorld();

    private void OnDestroy()
    {
        if (_world == null) return;
        _world.Dispose();
    }

    void LateUpdate()
    {
        World.Update(Time.deltaTime);
    }
}
