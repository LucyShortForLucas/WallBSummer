#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class BuildSystemUI : MonoBehaviour, IInjectable
{
    // ---- Unity Object Refs
    [SerializeReference] private GameObject? _contentRoot;
    [SerializeReference] private GameObject? _buildableRecordPrefab;
    [SerializeReference] private GameObject? _resourceCostPrefab;
    [SerializeReference] private BuildableDatabase? _buildableDatabase;
    [SerializeReference] private StorageComponent? _mainPlayerStorage;
    [SerializeReference] private BuildSystem? _buildSystem;


    // ---- Dependencies
    private CentralResourceHub? _resourceHub;

    public void Inject(DependencyContainer container)
    {
        _resourceHub = container.Get<CentralResourceHub>();
    }

    // ---- Data
    private List<BuildableOptionRecord> _buildableOptionRecords = new();
    private bool _dirty = true;

    // ---- Private helper methods

    private bool BuildableCostMet(BuildableDatabase.Buildable buildable)
    {
        if (_mainPlayerStorage == null || _resourceHub == null)
            return false;

        foreach (var cost in buildable.resourceCost)
        {
            if (!_resourceHub.HasEnough(_mainPlayerStorage.StorageID, (int)cost.resource, cost.cost))
                return false;
        }
        return true;
    }

    private void PopulateMenu()
    {
        if (_buildableDatabase == null || _contentRoot == null)
            return;

        int i = 0;
        foreach (var buildable in _buildableDatabase)
        {
            var go = Instantiate(_buildableRecordPrefab, _contentRoot.transform);
            if (go == null)
                continue;

            var record = go.GetComponent<BuildableOptionRecord>();
            record.Populate(_buildableDatabase[i++], BuildableCostMet);
            
            _buildableOptionRecords.Add(record);
        }
    }

    // ---- Unity gameloop methods
    private void OnEnable()
    {
        if (_dirty)
        {
            PopulateMenu();
            _dirty = false;
        }
        foreach (var record in _buildableOptionRecords)
            record.Build += OnBuild;
    }

    private void OnDisable()
    {
        foreach (var record in _buildableOptionRecords)
            record.Build -= OnBuild;
    }

    // Event handlers
    private void OnBuild(BuildableDatabase.Buildable buildable)
    {
        if (_buildSystem == null || _resourceHub == null || _mainPlayerStorage == null)
            return;

        foreach (var cost in buildable.resourceCost)
        {
            if (!_resourceHub.ConsumeResource(_mainPlayerStorage.StorageID, (int)cost.resource, cost.cost))
                return;
        }

        _buildSystem.TryToPlace(buildable);

        gameObject.SetActive(false);
    }
}
