using UnityEngine;

public class BossAnimCtrl : MonoBehaviour
{
    [SerializeField] private Animator _anim;

    private static readonly int H_IsMoving = Animator.StringToHash("IsMoving");
    private static readonly int H_IsRanged = Animator.StringToHash("IsRanged");
    private static readonly int H_IsHalfHp = Animator.StringToHash("IsHalfHP");
    private static readonly int H_IsDead = Animator.StringToHash("IsDead");
    private static readonly int H_Slam = Animator.StringToHash("Slam");
    private static readonly int H_Smack = Animator.StringToHash("Smack");
    private static readonly int H_Shoot = Animator.StringToHash("Shoot");
    private static readonly int H_ShootTriple = Animator.StringToHash("ShootTriple");

    private void Awake()
    {
        if (_anim == null) _anim = GetComponent<Animator>();
    }

    public void SetMoving(bool v) => _anim?.SetBool(H_IsMoving, v);
    public void SetRanged(bool v) => _anim?.SetBool(H_IsRanged, v);
    public void SetHalfHp(bool v) => _anim?.SetBool(H_IsHalfHp, v);
    public void SetDead(bool v) => _anim?.SetBool(H_IsDead, v);
    public void PlaySlam() => _anim?.SetTrigger(H_Slam);
    public void PlaySmack() => _anim?.SetTrigger(H_Smack);
    public void PlayShoot() => _anim?.SetTrigger(H_Shoot);
    public void PlayShootTriple() => _anim?.SetTrigger(H_ShootTriple);
}