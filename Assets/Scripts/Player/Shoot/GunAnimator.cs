using UnityEngine;

public class GunAnimator : MonoBehaviour
{
    public Animator _anim;
    public PlayerInputHub _input;

    void Awake()
    {
        if (_input == null)
            _input = GetComponentInParent<PlayerInputHub>();
    }

    void Update()
    {
        if (_input.FireHeld)
        {
            _anim.SetTrigger("Shoot");
        }

        if (_input.ReloadPressedThisFrame)
        {
            _anim.SetTrigger("Reload");
        }
    }
}