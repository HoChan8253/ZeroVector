using UnityEngine;

// NavMesh 없이 Transform 을 직접 제어하는 공중 이동 컴포넌트
// Y 축(높이)은 flyingHeight 로 고정하고, XZ 평면에서만 이동
public class FlyingEnemyMovement : MonoBehaviour
{
    private EnemyData _data;
    private float _targetHeight;

    // 선회 방향 (1 = 시계, -1 = 반시계)
    private int _strafeDir = 1;
    private float _nextStrafeFlip;

    // 캐시
    private float MoveSpeed => _data != null ? _data.flyingMoveSpeed : 3.5f;
    private float TurnSpeed => _data != null ? _data.turnSpeed : 8f;
    private float HeightSpeed => 3f; // 목표 높이로 올라가는 속도

    public void Init(EnemyData data, float height)
    {
        _data = data;
        _targetHeight = height;
        // 랜덤하게 선회 방향 결정
        _strafeDir = Random.value > 0.5f ? 1 : -1;
    }

    // 제자리 호버링 Idle 상태
    public void HoverInPlace()
    {
        MaintainHeight();
    }

    // 플레이어 방향으로 접근 Chase 상태
    public void MoveTowardPlayer(Vector3 playerPos)
    {
        MaintainHeight();

        Vector3 flat = Flatten(playerPos) - Flatten(transform.position);
        if (flat.sqrMagnitude < 0.01f) return;

        // 이동
        transform.position += flat.normalized * MoveSpeed * Time.deltaTime;

        // 회전: 항상 플레이어를 바라봄
        FaceFlat(playerPos);
    }

    // 플레이어 주위를 선회하며 사격
    public void StrafeAroundPlayer(Vector3 playerPos)
    {
        MaintainHeight();
        FaceFlat(playerPos);

        // 일정 시간마다 선회 방향 반전
        if (Time.time >= _nextStrafeFlip)
        {
            _strafeDir = -_strafeDir;
            _nextStrafeFlip = Time.time + Random.Range(3f, 6f);
        }

        // 플레이어를 중심으로 방향 벡터 계산
        Vector3 toSelf = (Flatten(transform.position) - Flatten(playerPos)).normalized;
        Vector3 tangent = new Vector3(-toSelf.z, 0f, toSelf.x) * _strafeDir;

        // 선회 이동
        transform.position += tangent * MoveSpeed * Time.deltaTime;
    }

    // 정지
    public void Stop()
    {
        // Update 루프는 AI 에서 관리
    }

    // 목표 높이를 향해 Y를 부드럽게 이동
    private void MaintainHeight()
    {
        float curY = transform.position.y;
        float newY = Mathf.MoveTowards(curY, _targetHeight, HeightSpeed * Time.deltaTime);
        var pos = transform.position;
        pos.y = newY;
        transform.position = pos;
    }

    // 수평 방향만 고려해서 플레이어를 바라봄
    private void FaceFlat(Vector3 target)
    {
        Vector3 dir = target - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion lookRot = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(
            transform.rotation, lookRot, TurnSpeed * Time.deltaTime);
    }

    private static Vector3 Flatten(Vector3 v)
    {
        v.y = 0f;
        return v;
    }
}