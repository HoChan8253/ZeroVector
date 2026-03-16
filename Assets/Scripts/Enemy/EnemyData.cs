using UnityEngine;

public enum EnemyAttackType
{
    Melee,
    Ranged,
    RangedAoe
}

[CreateAssetMenu(menuName = "EnemySO/Enemy Data", fileName = "ED_NewEnemy")]
public class EnemyData : ScriptableObject
{
    [Header("Core")]
    public int maxHp = 100;

    [Header("Shield")]
    public bool useShield = false;
    public int maxShield = 0;

    [Header("Reward")]
    public int goldReward = 10;
    public int goldRewardMin = 8; // 랜덤 최솟값
    public int goldRewardMax = 15; // 랜덤 최댓값
    public bool randomGold = false;

    [Header("Day Loop")]
    public float dayIdleTime = 2.0f;
    public float dayWalkTime = 3.0f;
    public float patrolRadius = 6f;

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

    [Header("Flying")]
    public bool isFlying = false; // 공중 유닛인지
    public float flyingHeight = 4.5f; // 유지할 Y 높이
    public float flyingMoveSpeed = 3.5f; // 공중 이동 속도
    public float turnSpeed = 8f; // 플레이어를 바라보는 회전 속도

    [Header("Combat")]
    public float attackRange = 2.0f;
    public float attackCooldown = 1.2f;

    [Header("Damage")]
    public int attackDamage = 10; // 근접 / 원거리 / 장판 공통 공격력

    [Header("Melee Attack")]
    public float meleeHitRadius = 1.0f;   // 타격 판정 반경
    public float meleeHitForward = 0.9f;  // 적 앞쪽 오프셋
    public float meleeHitHeight = 1.0f;   // 판정 높이 오프셋

    [Header("Range Attack")]
    public float projectileSpeed = 15f;
    public float projectileLifeTime = 3.0f;

    [Header("AoE Timing")]
    public float aoeWarnTime = 2f; // 범위 표시 시간
    public float aoeDropHeight = 15f; // 낙하 시작 높이
    public float aoeDropTime = 2f; // 떨어지는 시간

    [Header("AoE Zone")]
    public float aoeRadius = 3f; // 범위 공격 반경

    [Header("Stun")]
    public bool canStun = true; // 헤드샷 여부
    public float stunTime = 1.0f; // 헤드샷시 움찔
}