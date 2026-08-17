using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack Setup")]
    [SerializeField] private Vector3 hitboxSize = new Vector3(2f, 2f, 2f);
    [SerializeField] private float hitboxDistance = 1.5f;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private int damageAmount = 10;

    // Cooldown tracking
    private float attackCooldown = 0.5f;
    private float nextAttackTime = 0f;

    public void OnAttack()
    {
        if (Time.time >= nextAttackTime)
        {
            PerformAttack();

            nextAttackTime = Time.time + attackCooldown;
        }
    }

    private void PerformAttack()
    {
        Vector3 boxCenter = transform.position + transform.forward * hitboxDistance;

        Collider[] hitEnemies = Physics.OverlapBox(boxCenter, hitboxSize / 2f, transform.rotation, enemyLayer);

        if (hitEnemies.Length > 0)
        {
            Collider targetEnemy = hitEnemies[0];

            IDamageable target = targetEnemy.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(damageAmount);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;

        Vector3 boxCenter = transform.position + transform.forward * hitboxDistance;

        Gizmos.matrix = Matrix4x4.TRS(boxCenter, transform.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, hitboxSize);
    }
}