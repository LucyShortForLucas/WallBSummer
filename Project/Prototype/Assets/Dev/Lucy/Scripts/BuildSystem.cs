#nullable enable
using UnityEngine;
using UnityEngine.InputSystem;

using Strats = GridWorldSolidColorTileOverlayStrategies;
using StratNames = GridWorldSolidColorTileOverlayStrategies.StrategyName;

public static partial class Utils
{
    public static bool GetMousePointOnPlane(Plane plane, Camera camera, out Vector3 point)
    {
        Ray ray = camera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!plane.Raycast(ray, out float distance))
        {
            point = new();
            return false;
        }

        point = ray.GetPoint(distance);
        return true;
    }

    public static bool GetTileAtMousePos(float y, Camera camera, out Vector2Int tile)
    {
        if (!GetMousePointOnPlane(new Plane(Vector3.up, new Vector3(0,y,0)), camera, out var point))
        {
            tile = new();
            return false;
        }

        tile = GridWorld.PositionToTile(point);
        return true;
    }
}

public static partial class CustomColors
{
    public static readonly Color BlackTrans = new Color(0, 0, 0, 0.35f);
}

public class BuildSystem : MonoBehaviour
{

    // ---- Unity object refs
    [Header("Scene agnostic references")]
    [SerializeReference] private BuildableDatabase? _buildableDatabase;
    [SerializeReference] private MeshFilter? _previewFilter;
    [SerializeReference] private GameObject? _gridOverlay;

    [Header("Scene local references")]
    [SerializeReference] private Camera? _camera;
    [SerializeReference] private GridWorldHandler? _gridWorldHandler;
    [SerializeReference] private Transform? _focus;
    [SerializeReference] private TooltipHandler? _toolTipHandler;

    [Header("Config")]
    [SerializeField] private float _buildRange = 10f;

    // ---- State
    private BuildableDatabase.Buildable? _tryingToPlaceBuildable;

    // ---- Tooltips
    private TooltipHandler.Handle? _obstructedTooltip;
    private TooltipHandler.TooltipData _obstructedTooltipData = new("Obstructed!", Color.red, CustomColors.BlackTrans);
    private TooltipHandler.Handle? _outOfRangeTooltip;
    private TooltipHandler.TooltipData _outOfRangeTooltipData = new("Out of range!", Color.red, CustomColors.BlackTrans);


    // ---- Public API

    public void TryToPlace(int id)
    {
        if (_previewFilter == null || _buildableDatabase == null || id >= _buildableDatabase.Count || _gridOverlay == null)
            return;

        _tryingToPlaceBuildable = _buildableDatabase[id];

        _previewFilter.mesh = _buildableDatabase[id].previewMesh;
        _previewFilter.gameObject.SetActive(true);
        _gridOverlay.SetActive(true);

        if (_gridWorldHandler != null && _gridWorldHandler.World != null) _gridWorldHandler.World.FillBuildObstructionType(new RectInt(-32, -32, 32, 32), GridWorld.BuildObstructionType.Natural);

        Cursor.visible = false;
    }

    public void StopTryingToPlace()
    {
        if (_previewFilter == null || _buildableDatabase == null || _gridOverlay == null || _tryingToPlaceBuildable == null)
            return;

        _tryingToPlaceBuildable = null;
        _previewFilter.gameObject.SetActive(false);
        _gridOverlay.SetActive(false);

        _obstructedTooltip?.RemoveTooltip();
        _outOfRangeTooltip?.RemoveTooltip();

        Cursor.visible = true;
    }

    // ---- Unity methods
    private void Update()
    {
        TryToPlace(0); // FOR TESTING

        if (_camera == null || _tryingToPlaceBuildable == null || _gridWorldHandler == null || _gridWorldHandler.World == null 
            || _buildableDatabase == null || _previewFilter == null || _focus == null || _toolTipHandler == null
            || !Utils.GetTileAtMousePos(transform.position.y, _camera, out Vector2Int tile)
            || Strats.strategies[StratNames.Building] is not Strats.BuildStrat buildstrat)
            return;

        _obstructedTooltip ??= _toolTipHandler.NewHandle();
        _outOfRangeTooltip ??= _toolTipHandler.NewHandle();

        var buildable = _tryingToPlaceBuildable.Value;

        // Handle mesh preview
        _previewFilter.transform.position = GridWorld.TileToPosition(tile, transform.position.y) + buildable.placementOffset;

        // Check if building would be obstructed
        RectInt buildingTileRect = new RectInt(tile, buildable.tileFootPrint);
        buildstrat.BuildAttemptRect = buildingTileRect;
        bool obstructed = false;
        foreach (var obstruction in _gridWorldHandler.World.GetBuildObstructionType(buildingTileRect))
        {
            if (obstruction != GridWorld.BuildObstructionType.None)
            {
                obstructed = true;
                break;
            }
        }

        // Check if building would be out of range
        float distance = Vector3.Distance(_focus.position, GridWorld.TileToPosition(tile));
        bool outOfRange = distance > _buildRange;

        // Handle obstructed or out of range
        buildstrat.BuildAllowed = !obstructed && !outOfRange;

        if (obstructed && _obstructedTooltip.Empty)
            _obstructedTooltip.NewTooltip(_obstructedTooltipData);
        else if (!obstructed)
            _obstructedTooltip.RemoveTooltip();

        if (outOfRange && _outOfRangeTooltip.Empty)
            _outOfRangeTooltip.NewTooltip(_outOfRangeTooltipData);
        else if (!outOfRange)
            _outOfRangeTooltip.RemoveTooltip();
    }
}
