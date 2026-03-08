using UnityEngine;

[CreateAssetMenu(menuName = "FPS/Boss Data", fileName = "BD_NewBoss")]
public class MiniBossData : ScriptableObject
{
    [Header("Core")]
    public int maxHp = 1000;

    [Header("Shield")]
    public bool useShield = true;
    public int maxShield = 300;
    public float shieldCooldown = 20f; // Taunt 쿨타임

    [Header("Movement")]
    public float patrolSpeed = 1.4f;
    public float chaseSpeed = 4f;
    public float angularSpeed = 240f;
    public float acceleration = 6f;
    public float patrolRadius = 8f;

    [Header("Aggro")]
    public float aggroRange = 20f;
    public float deaggroRange = 30f;
    public float damageAggroHoldTime = 5f;

    [Header("Attack1")]
    public int atk1Damage = 30;
    public float atk1Range = 2.5f;
    public float atk1Cooldown = 2f;
    public float atk1HitRadius = 1.2f;
    public float atk1HitForward = 1.0f;
    public float atk1HitHeight = 1.0f;

    [Header("Attack2")]
    public int atk2Damage = 20;
    public float atk2Range = 4f;   // 발동 거리
    public float atk2Cooldown = 5f;
    public float atk2Radius = 3.5f; // 실제 판정 반경

    [Header("Stomp")]
    public int stompDamage = 25;
    public float stompRange = 8f;   // 발동 거리
    public float stompCooldown = 8f;
    public float stompProjSpeed = 14f;
    public float stompProjLife = 3f;
    public int stompDirCount = 8;    // 방향 수 (보통 8)

    [Header("Stun")]
    public bool canStun = true;
    public float stunTime = 1.5f;

    [Header("Day Loop")]
    public float dayIdleTime = 3f;
    public float dayWalkTime = 4f;
}