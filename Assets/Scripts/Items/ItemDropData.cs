using UnityEngine;

public enum ItemDropType
{
    AmmoAR,
    AmmoShotgun,
    EnergyDrink,
    MedKit,
    Gold
}

[CreateAssetMenu(menuName = "Item/ItemDropData", fileName = "ID_New")]
public class ItemDropData : ScriptableObject
{
    [Header("기본 정보")]
    public ItemDropType dropType;
    public GameObject prefab;
    [Range(0f, 1f)] public float dropChance = 0.3f;

    [Header("탄약")]
    public int ammoAmount = 30;

    [Header("에너지 드링크")]
    public float staminaRegenMultiplier = 2f;
    public float staminaBoostDuration = 5f;

    [Header("구급 상자")]
    [Range(0f, 1f)] public float healPercent = 0.15f;

    [Header("골드")]
    public int goldMin = 50;
    public int goldMax = 150;

    [Header("라이프타임")]
    public float lifetime = 25f;
    public float blinkStartTime = 10f;
}