using UnityEngine;

public class Health : MonoBehaviour
{
    public float _hp = 100f;

    public void TakeDamage(float dmg)
    {
        _hp -= dmg;
        if (_hp <= 0f)
            Destroy(gameObject);
    }
}