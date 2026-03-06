using UnityEngine;

public class StraightFxProjectile : MonoBehaviour
{
    private Vector3 _dir;
    private float _speed;
    private float _endTime;

    public void Init(Vector3 dir, float speed, float lifeTime)
    {
        _dir = dir.normalized;
        _speed = speed;
        _endTime = Time.time + lifeTime;

        // Z가 위로 향하게
        if (_dir.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(_dir);
    }

    private void Update()
    {
        transform.position += _dir * _speed * Time.deltaTime;

        if (Time.time >= _endTime)
            Destroy(gameObject);
    }
}