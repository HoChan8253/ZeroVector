using TMPro;
using UnityEngine;

public class GoldUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _goldText;

    private const int MaxDisplayGold = 90000000;

    private void Start()
    {
        if (GoldManager.Instance != null)
        {
            GoldManager.Instance.OnGoldAdded += HandleGoldAdded;
            Refresh(GoldManager.Instance.Gold);
        }
        else
        {
            Debug.LogError("[GoldUI] GoldManager.Instance가 null! 씬에 GoldManager가 있는지 확인하세요.");
        }
    }

    private void OnDestroy()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldAdded -= HandleGoldAdded;
    }

    private void HandleGoldAdded(int total, int gained, Vector3 _)
        => Refresh(total);

    private void Refresh(int total)
    {
        int displayGold = Mathf.Clamp(total, 0, MaxDisplayGold);
        _goldText.text = $"{displayGold:N0}";
    }
}