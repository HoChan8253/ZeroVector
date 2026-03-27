using UnityEngine;
using UnityEngine.InputSystem;

public class CheatManager : MonoBehaviour
{
    [Header("Gold Cheat")]
    [SerializeField] private int _goldAmount = 100000;

    [Header("Boss Fight Cheat")]
    [SerializeField] private WaveManager _waveManager;

    private void Update()
    {
        // 골드 치트
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            if (GoldManager.Instance == null) return;
            GoldManager.Instance.Add(_goldAmount, Vector3.zero);
            Debug.Log($"[Cheat] 골드 +{_goldAmount} 지급");
        }

        // 보스전 치트
        if (Keyboard.current.f9Key.wasPressedThisFrame)
        {
            if (_waveManager == null) return;
            _waveManager.CheatToLastWave();
            Debug.Log("[Cheat] 보스전 진입!");
        }
    }
}