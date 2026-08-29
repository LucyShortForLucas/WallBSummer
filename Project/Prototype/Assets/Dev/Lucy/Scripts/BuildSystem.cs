#nullable enable
using UnityEngine;
using UnityEngine.InputSystem;
using static BuildableDatabase;
using StratNames = GridWorldSolidColorTileOverlayStrategies.StrategyName;
using Strats = GridWorldSolidColorTileOverlayStrategies;

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

public class BuildSystem : MonoBehaviour, IInjectable
{
    // ---- Unity inspector object refs
    [Header("Scene agnostic references")]
    [SerializeReference] private BuildableDatabase? _buildableDatabase;
    [SerializeReference] private MeshFilter? _previewFilter;
    [SerializeReference] private GameObject? _gridOverlay;
    [SerializeReference] private StorageComponent? _mainPlayerStorage;

    // ---- Unity inspector fields
    [Header("Config")]
    [SerializeField] private float _buildRange = 10f;

    // ---- Dependencies
    private GridWorldHandler? _gridWorldHandler;
    private TooltipHandler? _toolTipHandler;
    private PlayerObjectRegistry? _playerObjectRegistry;
    private CentralResourceHub? _resourceHub;

    public void Inject(DependencyContainer container)
    {
        _gridWorldHandler = container.Get<GridWorldHandler>();
        _toolTipHandler = container.Get<TooltipHandler>();
        _playerObjectRegistry = container.Get<PlayerObjectRegistry>();
        _resourceHub = container.Get<CentralResourceHub>();
    }

    // ---- State
    private BuildableDatabase.Buildable? _tryingToPlaceBuildable;
    private bool _placingAllowed = false;
    private RectInt _tryingToPlaceTileRect = new();

    // ---- Tooltips
    private TooltipHandler.Handle? _obstructedTooltip;
    private TooltipHandler.TooltipData _obstructedTooltipData = new("Obstructed!", Color.red, CustomColors.BlackTrans);
    private TooltipHandler.Handle? _outOfRangeTooltip;
    private TooltipHandler.TooltipData _outOfRangeTooltipData = new("Out of range!", Color.red, CustomColors.BlackTrans);
    private TooltipHandler.Handle? _insufficientResourcesTooltip;
    private TooltipHandler.TooltipData _insufficientResourcesTooltipData = new("Insufficient resources!", Color.red, CustomColors.BlackTrans);


    // ---- Public API
    public void TryToPlace(BuildableDatabase.Buildable buildable)
    {
        if (_previewFilter == null || _buildableDatabase == null || _gridOverlay == null)
            return;

        _tryingToPlaceBuildable = buildable;

        _previewFilter.mesh = _tryingToPlaceBuildable?.previewMesh;
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
        _insufficientResourcesTooltip?.RemoveTooltip();

        Cursor.visible = true;
    }

    public void PlaceCurrent()
    {
        if (_tryingToPlaceBuildable == null ||  !_placingAllowed || _gridWorldHandler == null || _gridWorldHandler.World == null
            || _mainPlayerStorage == null || _resourceHub == null)
            return;

        var buildable = _tryingToPlaceBuildable.Value;

        Vector3? nullableBuildingPosition = (new Vector3(_tryingToPlaceTileRect.position.x, transform.position.y, _tryingToPlaceTileRect.position.y) + _tryingToPlaceBuildable?.placementOffset);
        
        if (nullableBuildingPosition == null)
            return;

        foreach (var cost in buildable.resourceCost)
        {
            if (!_resourceHub.ConsumeResource(_mainPlayerStorage.StorageID, (int)cost.resource, cost.cost))
                return;
        }

        Vector3 buildingPosition = nullableBuildingPosition.Value;

        _gridWorldHandler.World.FillBuildObstructionType(_tryingToPlaceTileRect, GridWorld.BuildObstructionType.Building);
        Instantiate(_tryingToPlaceBuildable?.building, buildingPosition, Quaternion.identity);

        _placingAllowed = false;
    }

    // ---- Unity methods
    private void Update()
    {
        // Null checks
        if (_tryingToPlaceBuildable == null || _gridWorldHandler == null || _gridWorldHandler.World == null
            || _buildableDatabase == null || _previewFilter == null || _toolTipHandler == null || _playerObjectRegistry == null
            || _resourceHub == null || _mainPlayerStorage == null)
            return;

        Camera? camera = _gridWorldHandler.MainCam;

        if (camera == null || !Utils.GetTileAtMousePos(transform.position.y, camera, out Vector2Int tile)
            || Strats.strategies[StratNames.Building] is not Strats.BuildStrat buildstrat)
            return;

        // Tooltip handle init
        _obstructedTooltip ??= _toolTipHandler.NewHandle();
        _outOfRangeTooltip ??= _toolTipHandler.NewHandle();
        _insufficientResourcesTooltip ??= _toolTipHandler.NewHandle();

        // unpack non-nullable buildable
        var buildable = _tryingToPlaceBuildable.Value;

        // Handle mesh preview
        _previewFilter.transform.position = GridWorld.TileToPosition(tile, transform.position.y) + buildable.placementOffset;

        // Check if building would be obstructed
        _tryingToPlaceTileRect = new RectInt(tile, buildable.tileFootPrint);
        buildstrat.BuildAttemptRect = _tryingToPlaceTileRect;
        bool obstructed = false;
        foreach (var obstruction in _gridWorldHandler.World.GetBuildObstructionType(_tryingToPlaceTileRect))
        {
            if (obstruction != GridWorld.BuildObstructionType.None)
            {
                obstructed = true;
                break;
            }
        }

        // Check if building would be out of range
        float distance = _playerObjectRegistry.ClosestPlayerDistance(GridWorld.TileToPosition(tile));
        bool outOfRange = distance > _buildRange;

        // Check if enough resources
        bool notEnoughResources = false;
        foreach (var cost in buildable.resourceCost)
        {
            if (!_resourceHub.HasEnough(_mainPlayerStorage.StorageID, (int)cost.resource, cost.cost))
                notEnoughResources = true;
        }

        // Handle obstructed or out of range or not enough resources
        buildstrat.BuildAllowed = _placingAllowed = !obstructed && !outOfRange && !notEnoughResources;

        if (obstructed && _obstructedTooltip.Empty)
            _obstructedTooltip.NewTooltip(_obstructedTooltipData);
        else if (!obstructed)
            _obstructedTooltip.RemoveTooltip();

        if (outOfRange && _outOfRangeTooltip.Empty)
            _outOfRangeTooltip.NewTooltip(_outOfRangeTooltipData);
        else if (!outOfRange)
            _outOfRangeTooltip.RemoveTooltip();

        if (notEnoughResources && _insufficientResourcesTooltip.Empty)
            _insufficientResourcesTooltip.NewTooltip(_insufficientResourcesTooltipData);
        else if (!notEnoughResources)
            _insufficientResourcesTooltip.RemoveTooltip();
    }
}
