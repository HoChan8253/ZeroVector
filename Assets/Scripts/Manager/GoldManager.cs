using System;
using UnityEngine;

// 플레이어 재화 관리 매니저 (싱글톤)
public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    private const int MaxGold = 90000000;

    public int Gold { get; private set; }

    public bool CanAfford(int amount) => Gold >= amount;

    public event Action<int, int, Vector3> OnGoldAdded;

    public event System.Action<int> OnGoldSpent;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Add(int amount, Vector3 worldPos)
    {
        if (amount <= 0) return;

        Gold += amount;
        Gold = Mathf.Clamp(Gold, 0, MaxGold);

        OnGoldAdded?.Invoke(Gold, amount, worldPos);
    }

    public bool Spend(int amount)
    {
        if (amount <= 0) return false;
        if (Gold < amount) return false;

        Gold -= amount;
        Gold = Mathf.Clamp(Gold, 0, MaxGold);

        OnGoldSpent?.Invoke(Gold);
        OnGoldAdded?.Invoke(Gold, -amount, Vector3.zero);
        return true;
    }
}