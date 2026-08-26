using UnityEngine;
using System.Collections;
using System;

public class AdvancedTurret : BaseTurret
{
    [SerializeField] private int burstCount = 3;
    [SerializeField] private float timeBetweenBurstShots = 0.1f;

    private bool isBursting = false;

    public event Action OnFireAdvancedTurret;

    protected override void Update()
    {
        if (Health.IsDead) return;

        if (isBursting) return;

        base.Update();
    }

    protected override void Fire()
    {
        OnFireAdvancedTurret?.Invoke();
        StartCoroutine(FireBurstRoutine());

        nextFireTime = Time.time + Stats.FireRate;
    }

    private IEnumerator FireBurstRoutine()
    {
        isBursting = true;

        for (int i = 0; i < burstCount; i++)
        {
            if (currentTarget == null) break;

            if (ProjectilePrefab != null && ProjectileStats != null)
            {
                GameObject bulletObj = Instantiate(ProjectilePrefab, FirePoint.position, FirePoint.rotation);
                IProjectile bullet = bulletObj.GetComponent<IProjectile>();

                if (bullet != null)
                {
                    bullet.Initialize(FirePoint.forward, ProjectileStats, this.transform);
                }
            }

            yield return new WaitForSeconds(timeBetweenBurstShots);
        }

        isBursting = false;
    }
}