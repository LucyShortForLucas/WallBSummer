using UnityEngine;

public class BasicTurret : BaseTurret
{
    protected override void Fire()
    {
        if (projectilePrefab == null || projectileStats == null) return;

        GameObject bulletObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        IProjectile bullet = bulletObj.GetComponent<IProjectile>();

        if (bullet != null)
        {
            bullet.Initialize(firePoint.forward, projectileStats, this.transform);
        }

        nextFireTime = Time.time + stats.fireRate;
    }
}