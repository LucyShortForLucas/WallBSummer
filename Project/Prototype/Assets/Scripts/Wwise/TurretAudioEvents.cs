using UnityEngine;

public class TurretAudioEvents : MonoBehaviour
{
    [SerializeField] private AK.Wwise.Event _turretShotEvent;
    private BasicTurret _turretReference;

    private void Awake()
    {
        _turretReference = GetComponent<BasicTurret>();
    }
    private void OnEnable()
    {
        _turretReference.OnFireBasicTurret += TurretFire;
    }
    private void OnDisable()
    {
        _turretReference.OnFireBasicTurret -= TurretFire;
    }

    private void TurretFire()
    {
        _turretShotEvent.Post(gameObject);
    }
}
