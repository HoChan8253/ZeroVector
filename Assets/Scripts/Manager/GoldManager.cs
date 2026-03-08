using System;
using UnityEngine;

// 플레이어 재화 관리 매니저 (싱글톤)
public class GoldManager : MonoBehaviour
{
    public static GoldManager Instance { get; private set; }

    public int Gold { get; private set; }

    public event Action<int, int, Vector3> OnGoldAdded;

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
        OnGoldAdded?.Invoke(Gold, amount, worldPos);
    }
}