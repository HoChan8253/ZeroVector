using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Enemy Data", fileName = "ED_NewEnemy")]
public class EnemyData : ScriptableObject
{
    public int maxHp = 100;

    public float aggroRange = 12f;
    public float attackRange = 2.0f;
    public float attackCooldown = 1.2f;

    public float dayIdleTime = 2.0f;
    public float dayWalkTime = 3.0f;
    public float patrolRadius = 6f;

    public float stunTime = 1.0f;
}