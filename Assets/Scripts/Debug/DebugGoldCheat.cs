using UnityEngine;
using UnityEngine.InputSystem;

public class DebugGoldCheat : MonoBehaviour
{
    [SerializeField] private int _goldAmount = 100000;

    private void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            if (GoldManager.Instance == null) return;
            GoldManager.Instance.Add(_goldAmount, Vector3.zero);
            Debug.Log($"[Cheat] 골드 +{_goldAmount} 지급");
        }
    }
}