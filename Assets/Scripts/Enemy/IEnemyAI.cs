using UnityEngine;

// EnemyHealth 가 AI 타입에 관계없이 호출할 수 있는 공통 인터페이스
public interface IEnemyAI
{
    EnemyData Data { get; }
    bool IsDead { get; }

    void OnDamaged(Vector3 hitPoint, bool stun);
    void Die();
}