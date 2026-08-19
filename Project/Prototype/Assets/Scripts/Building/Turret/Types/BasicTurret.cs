using UnityEngine;

public class BasicTurret : BaseTurret
{
    protected override void Fire()
    {
        if (ProjectilePrefab == null || ProjectileStats == null) return;

        GameObject bulletObj = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
        IProjectile bullet = bulletObj.GetComponent<IProjectile>();

        if (bullet != null)
        {
            bullet.Initialize(FirePoint.forward, ProjectileStats, this.transform);
        }

        nextFireTime = Time.time + Stats.FireRate;
    }
}