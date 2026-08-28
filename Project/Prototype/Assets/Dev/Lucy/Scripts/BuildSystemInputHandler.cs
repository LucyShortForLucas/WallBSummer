#nullable enable
using Unity.VisualScripting;
using UnityEngine;

public class BuildSystemInputHandler : MonoBehaviour
{

    // ---- Unity object refs
    [Header("Scene agnostic")]
    [SerializeReference] private BuildSystem? _buildSystem;
    [SerializeReference] private GameObject? _buildSystemUIRoot;

    // ---- Data

    private bool _buildModeOn = false;

    // ---- Private helper methods
    
    private void EnterBuildMode()
    {
        if (_buildSystemUIRoot == null || _buildModeOn)
            return;

        _buildModeOn = true;
        _buildSystemUIRoot.SetActive(true);
    }

    private void ExitBuildMode()
    {
        if (_buildSystemUIRoot == null || !_buildModeOn)
            return;

        _buildModeOn = false;
        _buildSystemUIRoot.SetActive(false);
    }

    // ---- Unity Input messages
    private void OnStartBuilding()
    {
        if (!_buildModeOn)
            EnterBuildMode();
        else
            ExitBuildMode();
    }

    private void OnPlaceBuilding()
    {
        if (_buildSystem == null)
            return;
        
        _buildSystem.PlaceCurrent();
        _buildSystem.StopTryingToPlace();
        _buildModeOn = false;
    }

}
