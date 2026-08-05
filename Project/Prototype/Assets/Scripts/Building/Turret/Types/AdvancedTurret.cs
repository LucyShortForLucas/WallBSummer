using UnityEngine;
using System.Collections;

public class AdvancedTurret : BaseTurret
{
    public int burstCount = 3;
    public float timeBetweenBurstShots = 0.1f;

    private bool isBursting = false;

    protected override void Update()
    {
        if (Health.IsDead) return;

        if (isBursting) return;

        base.Update();
    }

    protected override void Fire()
    {
        StartCoroutine(FireBurstRoutine());

        nextFireTime = Time.time + stats.fireRate;
    }

    private IEnumerator FireBurstRoutine()
    {
        isBursting = true;

        for (int i = 0; i < burstCount; i++)
        {
            if (currentTarget == null) break;

            if (projectilePrefab != null && projectileStats != null)
            {
                GameObject bulletObj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
                IProjectile bullet = bulletObj.GetComponent<IProjectile>();

                if (bullet != null)
                {
                    bullet.Initialize(firePoint.forward, projectileStats, this.transform);
                }
            }

            yield return new WaitForSeconds(timeBetweenBurstShots);
        }

        isBursting = false;
    }
}