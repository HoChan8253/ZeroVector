using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator _anim;

    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Dead = Animator.StringToHash("Dead");
    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Stun = Animator.StringToHash("Stun");

    private void Awake()
    {
        if (_anim == null) _anim = GetComponent<Animator>();
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
    }

    public void SetMoveSpeed(float normalized01)
    {
        if (_anim == null) return;
        _anim.SetFloat(Speed, Mathf.Clamp01(normalized01));
    }

    public void PlayAttack()
    {
        if (_anim == null) return;
        _anim.SetTrigger(Attack);
    }

    public void PlayStun()
    {
        if (_anim == null) return;
        _anim.SetTrigger(Stun);
    }

    public void SetDead(bool value)
    {
        if (_anim == null) return;
        _anim.SetBool(Dead, value);
    }

    // 디버깅
    public bool TryGetAnimator(out Animator a)
    {
        a = _anim;
        return a != null;
    }
}