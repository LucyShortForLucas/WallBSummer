using UnityEngine;

public class AdvancedProjectile : BaseProjectile
{
    protected override void OnHit(Collider hit)
    {
        BaseRobotAI robot = hit.GetComponentInParent<BaseRobotAI>();
        if (robot != null && shooter != null)
        {
            robot.AlertUnderAttack(shooter);
        }

        IDamageable target = hit.GetComponentInParent<IDamageable>();
        if (target != null) target.TakeDamage(stats.damage);
    }
}