#nullable enable
using UnityEngine;
using UnityEngine.InputSystem;

public class PrototypeSetup : MonoBehaviour, IInjectable
{
    private GridWorld? _gridWorld;
    private EnemyHandler? _enemyHandler;
    public void Inject(DependencyContainer container)
    {
        var gwHandler = container.Get<GridWorldHandler>();
        if (gwHandler != null) 
            _gridWorld = gwHandler.World;

        _enemyHandler = container.Get<EnemyHandler>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_gridWorld == null)
            return;

        RectInt[] naturalObstrRects =
        {
            new RectInt(-3,-3, 6, 6),
            new RectInt(-4,-4, 3, 3),
            new RectInt(2,2, 2, 2),
        };

        foreach (var rect in naturalObstrRects) {
            _gridWorld.FillBuildObstructionType(rect, GridWorld.BuildObstructionType.Natural);
        }
    }

    private void OnSpawnWave(InputValue value)
    {
        print("Wave spawned");
        if (_enemyHandler != null)
            _enemyHandler.TriggerWave(10);
    }
}
