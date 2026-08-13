using UnityEngine;

public interface IProjectile
{
    void Initialize(Vector3 direction, ProjectileStatsData stats, Transform shooter);
}