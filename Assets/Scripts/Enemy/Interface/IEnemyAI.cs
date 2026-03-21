using UnityEngine;

public interface IEnemyAI
{
    int MaxHp { get; }
    int MaxShield { get; }
    bool UseShield { get; }
    bool CanStun { get; }
    float StunTime { get; }
    float HeadshotMultiplier { get; }
    bool IsDead { get; }
    void OnDamaged(Vector3 hitPoint, bool stun);
    void Die();
    void ActivateCombat();
}