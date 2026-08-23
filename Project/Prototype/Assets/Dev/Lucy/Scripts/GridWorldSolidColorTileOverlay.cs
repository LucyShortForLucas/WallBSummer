#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.LightTransport;

public class GridWorldSolidColorTileOverlay : MonoBehaviour
{
    // ------ Inspector fields --------

    // ---- Unity object refs
    [SerializeReference] private GridWorldHandler? _gridWorldHandler;
    [SerializeReference] private GameObject? _quadPrefab;
    [Tooltip("The gameobject given here will determine the center of the rect of chunks that is actually updated")]
    [SerializeReference] private GameObject? _focusObject;
    [SerializeReference] private Material? _material;

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
        if (_childQuadMeshes.ContainsKey(chunkCoord) || _quadPrefab == null || _textures == null)
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

        _textures.MakeChunkTexture(chunkCoord);

        mr.sharedMaterial = _material;
        var mpb = new MaterialPropertyBlock();
        mr.GetPropertyBlock(mpb);
        mpb.SetFloat("_Layer", _textures.GetTextureId(chunkCoord));
        mr.SetPropertyBlock(mpb);

        _childQuadMeshes[chunkCoord] = mr;
    }

    // ---- Properties

    public GridWorldSolidColorTileOverlayStrategies.StrategyName StrategyName { set => _strategyName = value; }
    public RectInt ChunkUpdateRect { get {
            Vector3 focusPoint = _focusObject != null ? _focusObject.transform.position : Vector3.zero;
            Vector2 focusPoint2d = new(focusPoint.x, focusPoint.z);
            return new(
                GridWorld.PositionToChunk(focusPoint2d) - _updateChunkRectExtents + Vector2Int.one,
                _updateChunkRectExtents * 2 - Vector2Int.one
                );
        } }
    // ------ Private methods ---------

    // ---- Unity methods
    private void Awake()
    {
        _textures = new();
        if (_material != null)
        {
            _material.SetTexture("_MainTex", _textures.TextureArray);
        }
    }
    private void Start()
    {
        RectInt rect = ChunkUpdateRect;
        Vector2Int start = rect.position;
        Vector2Int end = start + rect.size;

        for (int y = start.y; y < end.y; ++y )
        {
            for (int x = start.x; x < end.x; ++x)
            {
                var coord = new Vector2Int(x, y);
                InitChunkMesh(coord);
                
            }
        }
    }

    private void LateUpdate()
    {
        _updateTimeElapsed += Time.deltaTime;
        if (_updateTimeElapsed < _minUpdateTime || _gridWorldHandler == null || _textures == null)
            return;
        _updateTimeElapsed = 0f;

        RectInt rect = ChunkUpdateRect;
        Vector2Int start = rect.position;
        Vector2Int end = start + rect.size;
        var strategy = GridWorldSolidColorTileOverlayStrategies.strategies[_strategyName];
        var world = _gridWorldHandler.World;

        for (int y = start.y; y < end.y; ++y)
        {
            for (int x = start.x; x < end.x; ++x)
            {
                Vector2Int chunkCoord = new(x, y);
                strategy.UpdateColors(world, chunkCoord, out var colors);
                _textures.SetChunkColor(chunkCoord, colors);
            }
        }

        _textures.UpdateDirtyTextures();
    }
}
