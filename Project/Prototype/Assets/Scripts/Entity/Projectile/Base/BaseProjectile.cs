using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(Rigidbody))]
public abstract class BaseProjectile : MonoBehaviour, IProjectile
{
    protected ProjectileStatsData stats;
    protected Vector3 travelDirection;
    protected Transform shooter;
    protected bool isFired = false;

    public virtual void Initialize(Vector3 direction, ProjectileStatsData newStats, Transform shooterTransform)
    {
        stats = newStats;
        travelDirection = direction.normalized;
        shooter = shooterTransform;
        isFired = true;

        GetComponent<Collider>().isTrigger = true;
        Rigidbody rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        Destroy(gameObject, stats.lifetime);
    }

    protected virtual void Update()
    {
        if (!isFired || stats == null) return;
        transform.position += travelDirection * (stats.moveSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isFired || stats == null) return;

        if ((stats.targetLayer.value & (1 << other.gameObject.layer)) > 0)
        {
            OnHit(other);
            Destroy(gameObject);
        }
    }

    protected abstract void OnHit(Collider hit);
}