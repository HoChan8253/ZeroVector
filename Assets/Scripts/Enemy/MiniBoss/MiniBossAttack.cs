using UnityEngine;

public class BossAttack : MonoBehaviour
{
    [Header("Spawner")]
    [SerializeField] private Transform _bulletSpawner;

    [Header("Melee Hit")]
    [SerializeField] private LayerMask _playerMask;

    private MiniBossData _data;
    private Transform _target;
    private readonly Collider[] _hits = new Collider[8];

    public void Init(MiniBossData data, Transform target)
    {
        _data = data;
        _target = target;
    }

    public void UpdateTarget(Transform t) => _target = t;

    // 애니메이션 이벤트
    // Attack1 클립의 프레임에 등록
    public void AE_Attack1Hit() => TryMeleeHit(
        _data != null ? _data.atk1Damage : 30,
        _data != null ? _data.atk1HitRadius : 1.2f,
        _data != null ? _data.atk1HitForward : 1.0f,
        _data != null ? _data.atk1HitHeight : 1.0f);

    // Attack2 클립의 프레임에 등록
    public void AE_Attack2Hit() => TryAoeHit(
        _data != null ? _data.atk2Damage : 20,
        _data != null ? _data.atk2Radius : 3.5f);

    public void AE_StompFire() => FireStompProjectiles();

    // Taunt 클립의 실드 연출 시작 프레임에 등록
    public void AE_ShieldRegen()
    {
        // 시각 연출만 담당
        Debug.Log("[BossAttack] AE_ShieldRegen — 실드 연출 재생");
    }

    // 내부 공격 로직
    private void TryMeleeHit(int damage, float radius, float fwd, float height)
    {
        if (damage <= 0) return;

        // 캡슐의 두 끝점
        Vector3 bottom = transform.position + Vector3.up * height;
        Vector3 top = bottom + transform.forward * fwd;

        int count = Physics.OverlapCapsuleNonAlloc(
            bottom, top, radius, _hits, _playerMask,
            QueryTriggerInteraction.Ignore);

        for (int i = 0; i < count; i++)
        {
            var col = _hits[i];
            if (col == null) continue;
            if (col.transform.IsChildOf(transform) || col.transform == transform) continue;

            var d = col.GetComponentInParent<IDamageable>();
            if (d != null) { d.TakeDamage(damage); break; }
        }
    }

    private void TryAoeHit(int damage, float radius)
    {
        if (damage <= 0) return;

        foreach (var h in Physics.OverlapSphere(transform.position, radius))
        {
            if (h.transform.IsChildOf(transform)) continue;
            var d = h.GetComponentInParent<IDamageable>();
            d?.TakeDamage(damage);
        }
    }

    private void FireStompProjectiles()
    {
        if (ObjectPoolManager.Instance == null) return;

        int dirs = _data != null ? _data.stompDirCount : 8;
        float speed = _data != null ? _data.stompProjSpeed : 14f;
        float life = _data != null ? _data.stompProjLife : 3f;
        int damage = _data != null ? _data.stompDamage : 25;

        Vector3 origin = _bulletSpawner != null
            ? _bulletSpawner.position
            : transform.position + Vector3.up;

        float step = 360f / dirs;
        for (int i = 0; i < dirs; i++)
        {
            float angle = i * step;
            Vector3 dir = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;

            var obj = ObjectPoolManager.Instance.Spawn(
                PoolKey.EnergyBall,
                origin,
                Quaternion.LookRotation(dir, Vector3.up));

            if (obj == null) continue;

            var proj = obj.GetComponent<EnergyBall>();
            proj?.Init(PoolKey.EnergyBall, transform, dir, speed, damage, life);
        }
    }
}