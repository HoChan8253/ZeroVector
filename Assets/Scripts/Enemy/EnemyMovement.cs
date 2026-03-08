using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    [SerializeField] private NavMeshAgent _agent;

    private EnemyData _data;

    public bool IsMoving => _agent != null && !_agent.isStopped
                            && _agent.velocity.sqrMagnitude > 0.01f;

    public void Init(EnemyData data)
    {
        _data = data;
        if (_agent == null) _agent = GetComponent<NavMeshAgent>();
    }

    // 이동 명령
    public void ChaseTo(Vector3 destination)
    {
        if (_agent == null) return;
        ApplySpeed(_data != null ? _data.chaseSpeed : 3.5f);
        _agent.isStopped = false;
        _agent.SetDestination(destination);
    }

    public void PatrolTo(Vector3 destination)
    {
        if (_agent == null) return;
        ApplySpeed(_data != null ? _data.patrolSpeed : 1.6f);
        _agent.isStopped = false;
        _agent.SetDestination(destination);
    }

    public void Stop()
    {
        if (_agent == null) return;
        _agent.isStopped = true;
        _agent.ResetPath();
    }

    // 유틸
    public float GetNormalizedSpeed()
    {
        if (_agent == null || _agent.speed < 0.01f) return 0f;
        return Mathf.Clamp01(_agent.velocity.magnitude / _agent.speed);
    }

    public bool TryGetRandomPatrolPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 12; i++)
        {
            Vector3 rand = center + Random.insideUnitSphere * radius;
            if (NavMesh.SamplePosition(rand, out var hit, 2f, NavMesh.AllAreas))
            {
                result = hit.position;
                return true;
            }
        }
        result = center;
        return false;
    }

    // 내부
    private void ApplySpeed(float speed)
    {
        if (_agent == null) return;
        _agent.speed = speed;

        if (_data != null)
        {
            _agent.angularSpeed = _data.angularSpeed;
            _agent.acceleration = _data.acceleration;
        }
    }
}