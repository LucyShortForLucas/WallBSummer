#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class GridWorldSolidColorTileOverlay : MonoBehaviour
{
    // ------ Inspector fields --------

    // ---- Unity object refs
    [SerializeReference] private GridWorldHandler? _gridWorldHandler;
    [SerializeReference] private GameObject? _quadPrefab;
    [Tooltip("The gameobject given here will determine the center of the rect of chunks that is actually updated")]
    [SerializeReference] private GameObject? _focusObject;

    // ---- Config
    [SerializeField] private GridWorldSolidColorTileOverlayStrategies.StrategyName _strategyName = GridWorldSolidColorTileOverlayStrategies.StrategyName.Fertility;
    [SerializeField] private float _minUpdateTime = 0.1f;
    [SerializeField] private Vector2Int _updateChunkRectExtents = new(5, 5);

    // ------ Data --------------------
    private GridWorldSolidColorTileTextures? _textures;
    private Dictionary<Vector2Int, MeshRenderer> _childQuadMeshes = new();
    private float _updateTimeElapsed = 0f;

    // ------ Public api --------------

    public void InitChunkMesh(Vector2Int chunkCoord)
    {
        if (_childQuadMeshes.ContainsKey(chunkCoord) || _quadPrefab == null)
            return;

        var go = Instantiate<GameObject>(_quadPrefab,
            new Vector3(chunkCoord.x * GridWorld.CHUNK_SIZE, 0, chunkCoord.y * GridWorld.CHUNK_SIZE),
            Quaternion.identity, transform);

        var mr = go.GetComponentInChildren<MeshRenderer>();
        if (mr == null)
        {
            Destroy(go);
            return;
        }

        _childQuadMeshes[chunkCoord] = mr;
    }

    // ---- Properties

    public GridWorldSolidColorTileOverlayStrategies.StrategyName StrategyName { set => _strategyName = value; }
    public RectInt ChunkUpdateRect { get {
            Vector3 focusPoint = _focusObject != null ? _focusObject.transform.position : Vector3.zero;
            Vector2 focusPoint2d = new(focusPoint.x, focusPoint.z);
            return new(
                GridWorld.PositionToChunk(focusPoint2d) - _updateChunkRectExtents,
                _updateChunkRectExtents * 2 - Vector2Int.one
                );
        } }
    // ------ Private methods ---------

    // ---- Unity methods
    private void Awake()
    {
        _textures = new();
    }
    private void Start()
    {
        InitChunkMesh(new Vector2Int(0, 0));
        InitChunkMesh(new Vector2Int(1, 1));
        InitChunkMesh(new Vector2Int(0, 1));
        InitChunkMesh(new Vector2Int(1, 0));
    }

    private void LateUpdate()
    {
        _updateTimeElapsed += Time.deltaTime;
        if (_updateTimeElapsed < _minUpdateTime || _gridWorldHandler == null || _textures == null)
            return;

        _updateTimeElapsed = 0f;

        var rect = ChunkUpdateRect;
        Vector2Int start = rect.position;
        Vector2Int end = start + rect.size;
        for (int y = start.y; y < end.y; ++y)
        {
            for (int x = start.x; x < end.x; ++x)
            {
                Vector2Int chunkCoord = new(x, y);
                GridWorldSolidColorTileOverlayStrategies.strategies[_strategyName].UpdateColors(
                    _gridWorldHandler.World,
                    chunkCoord,
                    out var colors
                    );
                _textures.SetChunkColor(chunkCoord, colors);
            }
        }
    }
}
