using UnityEngine;

public interface IEnemyAI
{
    // EnemyData 직접 노출 대신 EnemyHealth가 실제로 쓰는 값만 노출
    int MaxHp { get; }
    int MaxShield { get; }
    bool UseShield { get; }
    bool CanStun { get; }
    float StunTime { get; }
    bool IsDead { get; }

    void OnDamaged(Vector3 hitPoint, bool stun);
    void Die();
}