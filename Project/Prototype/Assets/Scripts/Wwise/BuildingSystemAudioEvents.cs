using UnityEngine;

public class BuildingSystemAudioEvents : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event _placeEvent;
    private BuildSystem _systemReference;

    private void Awake()
    {
        _systemReference = GetComponent<BuildSystem>();
    }
    private void OnEnable()
    {
        _systemReference.OnBuildingPlaced += BuildingPlace;
    }
    private void OnDisable()
    {
     _systemReference.OnBuildingPlaced -= BuildingPlace;
    }

    private void BuildingPlace(string unneededString)
    {
        _placeEvent.Post(gameObject);
    }
}
