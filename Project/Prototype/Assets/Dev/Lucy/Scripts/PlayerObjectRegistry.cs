#nullable enable
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This object carries basic and systematic information about the current 'player' object(s) in the scene.
/// A 'player' object is any object that represents a (part of an) avatar of a player in the game world.
/// </summary>
/// <remarks>
/// This information is primarily used to fetch things like the complete list of all transforms of all 
/// player objects, which then in turn determines which parts of the world should actually be awake,
/// rendered, and updated. 
/// </remarks>
public class PlayerObjectRegistry : MonoBehaviour
{
    // ---- Unity inspector object refs
    [SerializeReference] private List<GameObject> _initialPlayerObjects = new();

    // ---- data
    /// <summary>
    /// All current player object transforms, as well as the canonical source of which player objects are registered.
    /// </summary>
    private List<Transform> _playerTransforms = new();

    // ---- API - properties
    public IReadOnlyList<Transform> PlayerTransforms => _playerTransforms;

    public float ClosestPlayerDistance(Vector3 point)
    {
        float result = float.MaxValue;

        foreach (var transform in _playerTransforms)
            result = Mathf.Min(result, Vector3.Distance(transform.position, point));

        return result;
    }

    // ---- API - Register players
    public void RegisterPlayerObject(GameObject playerObject)
    {
        _playerTransforms.Add(playerObject.transform);
    }

    public void UnregisterPlayerObject(GameObject playerObject)
    {
    }

    // ---- Unity gameloop methods

    private void Awake()
    {
        foreach (var player in _initialPlayerObjects)
            RegisterPlayerObject(player);
    }
}
