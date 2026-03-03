using UnityEngine;

// 탄착 지점 범위 표시
public class AoEIndicator : MonoBehaviour
{
    [SerializeField] private Transform _visual;

    public void SetRadius(float radius)
    {
        if (_visual == null) _visual = transform;

        float d = radius * 2f;
        _visual.localScale = new Vector3(d, d, 1f);
    }

    public void SetPosition(Vector3 groundPoint)
    {
        transform.position = groundPoint + Vector3.up * 0.02f;
    }
}