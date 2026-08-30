#nullable enable
using UnityEngine;
using UnityEngine.UIElements;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Scene Dependencies")]
    [SerializeReference] private TimeManager? _timeManager;
    [SerializeReference] private EnemyHandler? _enemyHandler;
    [SerializeReference] private InventoryUIManager? _uiManager;
    [SerializeReference] private GlobalRecipeDatabase? _recipeDatabase;
    [SerializeReference] private GridWorldHandler? _gridWorldHandler;
    [SerializeReference] private TooltipHandler? _toolTipHandler;
    [SerializeReference] private PlayerObjectRegistry? _playerObjectRegistry;

    public CentralResourceHub ResourceHub; // TEMP, remove in production

    private void Awake()
    {
        DependencyContainer container = new DependencyContainer();

        ResourceHub = new CentralResourceHub();

        //  Register systems
        container.Register(ResourceHub);
        container.SafeRegister(_timeManager);
        container.SafeRegister(_uiManager);
        container.SafeRegister(_enemyHandler);
        container.SafeRegister(_gridWorldHandler);
        container.SafeRegister(_toolTipHandler);
        container.SafeRegister(_playerObjectRegistry);

        // Orphan all children
        for (int i = transform.childCount - 1; i >= 0; --i)
        {
            transform.GetChild(i).SetParent(null);
        }

        var allScripts = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include);

        foreach (var script in allScripts)
        {
            if (script is IInjectable injectable)
            {
                injectable.Inject(container);
            }
        }
    }
}