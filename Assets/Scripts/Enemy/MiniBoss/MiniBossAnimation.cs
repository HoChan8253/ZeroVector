using UnityEngine;

public class BossAnimation : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    private static readonly int H_MoveSpeed = Animator.StringToHash("MoveSpeed");
    private static readonly int H_Attack1 = Animator.StringToHash("Attack1");
    private static readonly int H_Attack2 = Animator.StringToHash("Attack2");
    private static readonly int H_Stomp = Animator.StringToHash("Stomp");
    private static readonly int H_Taunt = Animator.StringToHash("Taunt");
    private static readonly int H_Stun = Animator.StringToHash("Stun");
    private static readonly int H_Dead = Animator.StringToHash("Dead");

    private void Awake()
    {
        if (_anim == null) _anim = GetComponent<Animator>();
        if (_anim == null) _anim = GetComponentInChildren<Animator>();
    }

    public void SetMoveSpeed(float v) => _anim?.SetFloat(H_MoveSpeed, Mathf.Clamp01(v));
    public void PlayAttack1() => _anim?.SetTrigger(H_Attack1);
    public void PlayAttack2() => _anim?.SetTrigger(H_Attack2);
    public void PlayStomp() => _anim?.SetTrigger(H_Stomp);
    public void PlayTaunt() => _anim?.SetTrigger(H_Taunt);
    public void SetStun(bool value) => _anim?.SetBool(H_Stun, value);
    public void SetDead(bool value) => _anim?.SetBool(H_Dead, value);
}