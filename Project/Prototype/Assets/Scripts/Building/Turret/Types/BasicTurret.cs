using UnityEngine;
using System;

public class BasicTurret : BaseTurret
{
    public event Action OnFireBasicTurret;

    protected override void Fire()
    {
        if (ProjectilePrefab == null || ProjectileStats == null) return;

        GameObject bulletObj = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
        IProjectile bullet = bulletObj.GetComponent<IProjectile>();

        if (bullet != null)
        {
            bullet.Initialize(FirePoint.forward, ProjectileStats, this.transform);
        }

        OnFireBasicTurret?.Invoke();

        nextFireTime = Time.time + Stats.FireRate;
    }
}