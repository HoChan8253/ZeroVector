using UnityEngine;

public enum EnemyAttackType
{
    Melee,
    Ranged,
    RangedAoe
}

[CreateAssetMenu(menuName = "FPS/Enemy Data", fileName = "ED_NewEnemy")]
public class EnemyData : ScriptableObject
{
    [Header("Core")]
    public int maxHp = 100;

    [Header("Movement")]
    public float patrolSpeed = 1.6f;
    public float chaseSpeed = 3.5f;
    public float angularSpeed = 360f;
    public float acceleration = 8f;

    [Header("Aggro Ranges")]
    public float aggroRange = 12f;
    public float deaggroRange = 18f;
    public float damageAggroHoldTime = 2.0f;

    [Header("Type")]
    public EnemyAttackType attackType = EnemyAttackType.Melee;

    [Header("Attack")]
    public float attackRange = 2.0f;
    public float attackCooldown = 1.2f;

    [Header("Range Attack")]
    public float projectileSpeed = 15f;
    public float projectileLifeTime = 3.0f;
    public int projectileDamage = 10;

    [Header("AoE Timing")]
    public float aoeWarnTime = 1.2f; // 범위 표시 시간
    public float aoeDropHeight = 10f; // 낙하 시작 높이
    public float aoeDropTime = 0.35f; // 떨어지는 시간

    [Header("AoE Zone")]
    public float aoeRadius = 2.5f; // 장판 반경

    [Header("AoE Impact")]
    public int aoeImpactDamage = 15; // 착탄 피해

    [Header("Day Loop")]
    public float dayIdleTime = 2.0f;
    public float dayWalkTime = 3.0f;
    public float patrolRadius = 6f;

    [Header("Stun")]
    public bool canStun = true;
    public float stunTime = 1.0f;
}