using UnityEngine;

[CreateAssetMenu(menuName = "EnemySO/Boss Data", fileName = "BD_Boss")]
public class BossData : ScriptableObject
{
    [Header("Core")]
    public int maxHp = 3000;

    [Header("Movement")]
    public float chaseSpeed = 4f;
    public float aimMoveSpeed = 2.5f; // Run_Aim 이동 속도
    public float angularSpeed = 240f;
    public float acceleration = 8f;

    [Header("Aggro")]
    public float aggroRange = 30f;
    public float meleeRange = 2.5f; // Smack 발동 거리
    public float farRange = 18f; // Run_Aim 전환 거리

    [Header("Smack")]
    public int smackDamage = 35;
    public float smackRange = 2.8f;
    public float smackCooldown = 3f;
    public float smackKnockback = 8f;

    [Header("Shoot")]
    public int shootDamage = 20;
    public float shootCooldown = 5f;
    public float shootProjSpeed = 18f;
    public float shootProjLife = 4f;
    public float shootRadius = 6f; // 범위 판정 반경

    [Header("ShootTriple")]
    public int tripleShootDamage = 15;
    public float tripleShootCooldown = 8f;
    public float tripleShootProjSpeed = 10f;
    public float tripleShootProjLife = 6f;
    public float tripleShootTurnSpeed = 90f;
    public float tripleShootInterval = 0.4f; // 3발 사이 간격

    [Header("Slam")]
    public int slamDamage = 50;
    public float slamCooldown = 15f; // 2페이즈 반복 쿨타임
    public float slamProjSpeed = 14f;
    public float slamProjLife = 3f;
    public int slamProjCount = 12; // 360도 투사체 수

    [Header("Phase2")]
    public float phase2HpRatio = 0.5f; // 반피 기준

    [Header("Idle")]
    public float idleChance = 0.15f; // 패턴 선택 시 Idle 확률
    public float idleDuration = 1.5f;

    [Header("Reward")]
    public int goldReward = 5000;
}