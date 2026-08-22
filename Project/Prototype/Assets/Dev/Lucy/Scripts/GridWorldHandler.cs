#nullable enable
using UnityEngine;

public class GridWorldHandler : MonoBehaviour
{
    private GridWorld? _world;
    public GridWorld? World => _world;

    void Start()
    {
        _world = GridWorld.TestGridWorld();
    }

    private void OnDestroy()
    {
        if (_world == null) return;
        _world.Dispose();
    }

    void FixedUpdate()
    {
        if (_world == null) return;
        _world.Update(Time.fixedDeltaTime);
    }
}
