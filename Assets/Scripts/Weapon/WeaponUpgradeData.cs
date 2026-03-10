using UnityEngine;

[CreateAssetMenu(fileName = "WeaponUpgradeData", menuName = "Game/WeaponUpgradeData")]
public class WeaponUpgradeData : ScriptableObject
{
    [Header("무기 기본 정보")]
    [Tooltip("연결할 WeaponData ScriptableObject — 기본 스탯 원본")]
    public WeaponData weaponData;
    public bool isPistol;
    public bool isShotgun;
    public int purchasePrice;

    [Header("치명타 (권총 전용)")]
    [Range(0f, 1f)]
    [Tooltip("업그레이드 없이 기본 치명타 확률")]
    public float baseCritChance = 0.05f;

    [Header("업그레이드 가격 (10단계)")]
    [Tooltip("인덱스 0 = 1단계 구매 비용, 인덱스 9 = 10단계 구매 비용")]
    public int[] powerPrices = new int[10];
    public int[] magPrices = new int[10];
    public int[] thirdPrices = new int[10];

    [Header("업그레이드 단계당 증가량")]
    [Tooltip("단계당 damage 증가량")]
    public float powerPerLevel = 5f;
    [Tooltip("단계당 탄창 증가량 / 샷건은 펠릿 수 증가량")]
    public int magPerLevel = 2;
    [Tooltip("단계당 예비탄약 증가량 / 권총은 치명타 확률 증가")]
    public int thirdPerLevel = 3;
}