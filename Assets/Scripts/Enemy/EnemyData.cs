using UnityEngine;

public enum EnemyAttackType
{
    Melee,
    Ranged
}

[CreateAssetMenu(menuName = "FPS/Enemy Data", fileName = "ED_NewEnemy")]
public class EnemyData : ScriptableObject
{
    [Header("Core")]
    public int maxHp = 100;

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

    [Header("Day Loop")]
    public float dayIdleTime = 2.0f;
    public float dayWalkTime = 3.0f;
    public float patrolRadius = 6f;

    [Header("Stun")]
    public bool canStun = true;
    public float stunTime = 1.0f;

    public bool useTwoAttackTriggers = true;
}