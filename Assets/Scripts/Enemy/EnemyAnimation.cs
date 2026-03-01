using UnityEngine;

public class EnemyAnimation : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Animator _anim;

    private static readonly int Speed = Animator.StringToHash("Speed");
    private static readonly int Dead = Animator.StringToHash("Dead");

    private static readonly int Attack = Animator.StringToHash("Attack");
    private static readonly int Attack1 = Animator.StringToHash("Attack1");
    private static readonly int Attack2 = Animator.StringToHash("Attack2");
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

    public void PlayAttackDefault()
    {
        if (_anim == null) return;
        _anim.SetTrigger(Attack);
    }

    public void PlayAttack1()
    {
        if (_anim == null) return;
        _anim.SetTrigger(Attack1);
    }

    public void PlayAttack2()
    {
        if (_anim == null) return;
        _anim.SetTrigger(Attack2);
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
}