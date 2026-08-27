#nullable enable
using System.Collections.Generic;
using UnityEngine;

public class BuildSystemUI : MonoBehaviour
{
    // ---- Unity Object Refs
    [SerializeReference] private GameObject? _contentRoot;
    [SerializeReference] private GameObject? _buildableRecordPrefab;
    [SerializeReference] private GameObject? _resourceCostPrefab;
    [SerializeReference] private BuildableDatabase? _buildableDatabase;

    // ---- Data
    private List<BuildableOptionRecord> _buildableOptionRecords = new();

    // ---- Private helper methods
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
            record.Populate(_buildableDatabase[i++]);

            _buildableOptionRecords.Add(record);
        }
    }

    // ---- Unity gameloop methods
    private void Start()
    {
        PopulateMenu();
    }
}
