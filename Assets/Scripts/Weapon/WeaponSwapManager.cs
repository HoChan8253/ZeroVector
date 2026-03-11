using UnityEngine;
using System.Collections;

public class WeaponSwapManager : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerInputHub _input;

    [Header("Weapons By Slot")]
    [SerializeField] private GameObject _weapon1;
    [SerializeField] private GameObject _weapon2;
    [SerializeField] private GameObject _weapon3;

    [Header("UI")]
    [SerializeField] private CrosshairController _crosshair;

    private GameObject _current;
    private GameObject _pending;

    private GunController _currentGun;
    private Animator _currentAnim;

    [SerializeField] private float _swapTimeout = 2.0f;
    [SerializeField] private string _holsterStateName = "Holster";
    [SerializeField] private string _equipStateName = "Equip";
    private bool _isSwapping;
    private float _swapDeadline;

    private static readonly int AnimHolster = Animator.StringToHash("Holster");
    private static readonly int AnimEquip = Animator.StringToHash("Equip");

    private void Awake()
    {
        if (_input == null)
            _input = GetComponentInParent<PlayerInputHub>();

        if (_weapon3 != null)
        {
            ActivateOnly(_weapon3);
            SetCurrent(_weapon3);
        }
        else if (_weapon2 != null)
        {
            ActivateOnly(_weapon2);
            SetCurrent(_weapon2);
        }
        else if (_weapon1 != null)
        {
            ActivateOnly(_weapon1);
            SetCurrent(_weapon1);
        }
    }

    private void Update()
    {
        if (_isSwapping && Time.time > _swapDeadline)
        {
            Debug.LogWarning("[WeaponSwap] Swap timeout -> Force unlock");
            _isSwapping = false;
            _pending = null;
            if (_currentGun != null) _currentGun.OnEquipAnimEnd();
        }

        if (_input == null) return;
        if (_isSwapping) return;

        if (_input.Weapon1PressedThisFrame) RequestSwapBySlot(1);
        if (_input.Weapon2PressedThisFrame) RequestSwapBySlot(2);
        if (_input.Weapon3PressedThisFrame) RequestSwapBySlot(3);
    }

    private void RequestSwapBySlot(int slot)
    {
        GameObject target = GetWeaponBySlot(slot);
        if (target == null) return;
        RequestSwap(target);
    }

    private GameObject GetWeaponBySlot(int slot)
    {
        return slot switch
        {
            1 => _weapon1,
            2 => _weapon2,
            3 => _weapon3,
            _ => null
        };
    }

    private void ActivateOnly(GameObject active)
    {
        if (_weapon1 != null) _weapon1.SetActive(_weapon1 == active);
        if (_weapon2 != null) _weapon2.SetActive(_weapon2 == active);
        if (_weapon3 != null) _weapon3.SetActive(_weapon3 == active);
    }

    private void SetCurrent(GameObject weaponGO)
    {
        _current = weaponGO;
        _currentGun = _current != null ? _current.GetComponent<GunController>() : null;
        _currentAnim = _current != null ? _current.GetComponent<Animator>() : null;

        // 크로스헤어에 현재 활성 GunController 전달
        if (_crosshair != null)
            _crosshair.SetGunController(_currentGun);
    }

    private void RequestSwap(GameObject target)
    {
        if (target == null) return;
        if (_current == target) return;

        // 현재 무기 컨트롤러 상태만 스왑 모드로
        if (_currentGun != null) _currentGun.BeginHolsterForSwap();

        // 애니가 없으면 즉시 교체
        if (_currentAnim == null)
        {
            ForceSwap(target);
            return;
        }

        _pending = target;
        _isSwapping = true;
        _swapDeadline = Time.time + _swapTimeout;

        // Holster 시작
        _currentAnim.ResetTrigger(AnimEquip);
        _currentAnim.SetTrigger(AnimHolster);
    }

    private void ForceSwap(GameObject target)
    {
        ActivateOnly(target);
        SetCurrent(target);

        if (_currentAnim != null)
        {
            _currentAnim.ResetTrigger(AnimHolster);
            _currentAnim.SetTrigger(AnimEquip);
        }

        _isSwapping = false;
        _pending = null;
    }

    // Holster 애니 끝(Animation Event)
    public void OnHolsterEnd()
    {
        if (_pending == null)
        {
            _isSwapping = false;
            if (_currentGun != null) _currentGun.CancelSwapState();
            return;
        }

        ActivateOnly(_pending);
        SetCurrent(_pending);

        StartCoroutine(CoEquipFromHolstered());

        _pending = null;

        Debug.Log("HolsterEnd called");
    }

    private IEnumerator CoEquipFromHolstered()
    {
        // 새 무기도 스왑 중으로 만들어서 발사/장전 막기
        if (_currentGun != null) _currentGun.BeginHolsterForSwap();

        if (_currentAnim != null)
        {
            // Holster 상태를 끝 포즈로 강제
            _currentAnim.Play(_holsterStateName, 0, 1f); // normalizedTime = 1 (끝)
            _currentAnim.Update(0f);
        }

        // 한 프레임 뒤 Equip 트리거
        yield return null;

        if (_currentAnim != null)
        {
            _currentAnim.ResetTrigger(AnimHolster);
            _currentAnim.ResetTrigger(AnimEquip);
            _currentAnim.SetTrigger(AnimEquip);
        }
    }

    // Equip 애니 끝(Animation Event)
    public void OnEquipEnd()
    {
        _isSwapping = false;

        if (_currentGun != null)
        {
            _currentGun.OnEquipAnimEnd();
            _currentGun.ForceNotifyAmmo();
        }

        Debug.Log("EquipEnd called");
    }
}