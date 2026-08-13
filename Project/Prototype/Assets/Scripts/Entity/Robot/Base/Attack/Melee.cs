using UnityEngine;

public class Melee : MonoBehaviour
{
    public Vector3 hitboxSize = new Vector3(1.5f, 1.5f, 1.5f);
    public float hitboxDistance = 1f;

    // Debug
    public bool showDebugVisuals = true;

    public void PerformAttack(int damageAmount, LayerMask targetLayer)
    {
        Vector3 center = transform.position + transform.forward * hitboxDistance;
        Collider[] hits = Physics.OverlapBox(center, hitboxSize / 2f, transform.rotation, targetLayer);

        foreach (Collider col in hits)
        {
            BaseRobotAI victimRobot = col.GetComponentInParent<BaseRobotAI>();
            if (victimRobot != null)
            {
                victimRobot.AlertUnderAttack(this.transform);
            }

            IDamageable target = col.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damageAmount);
                break;
            }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!showDebugVisuals) return;

        // Draw the red attack square
        Gizmos.color = Color.red;
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(transform.position + transform.forward * hitboxDistance, transform.rotation, Vector3.one);
        Gizmos.matrix = rotationMatrix;
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
        Gizmos.matrix = Matrix4x4.identity;
    }
#endif
}