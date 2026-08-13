using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileStats", menuName = "Entity/Projectile/Projectile Stats")]
public class ProjectileStatsData : ScriptableObject
{
    public float moveSpeed = 25f;
    public int damage = 15;
    public float lifetime = 4f;

    public LayerMask targetLayer;
}