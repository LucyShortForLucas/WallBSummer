using UnityEngine;
using UnityEngine.UIElements;

public class GameBootstrapper : MonoBehaviour
{
    [Header("Scene Dependencies")]
    [SerializeField] private TimeManager timeManager;
    [SerializeField] private EnemyHandler enemyHandler;
    [SerializeField] private InventoryUIManager uiManager;
    [SerializeField] private GlobalRecipeDatabase recipeDatabase;

    private CentralResourceHub resourceHub;

    private void Awake()
    {
        DependencyContainer container = new DependencyContainer();

        //  Register systems
        container.Register(new CentralResourceHub());
        container.Register(timeManager);
        container.Register(uiManager);
        container.Register(enemyHandler);

        var allScripts = FindObjectsByType<MonoBehaviour>();

        foreach (var script in allScripts)
        {
            if (script is IInjectable injectable)
            {
                injectable.Inject(container);
            }
        }
    }
}