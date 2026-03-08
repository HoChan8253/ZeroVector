using UnityEngine;

public class GoldPopupSpawner : MonoBehaviour
{
    [SerializeField] private GoldPopup _popupPrefab;
    [SerializeField] private Canvas _canvas;

    private void OnEnable()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldAdded += SpawnPopup;
    }

    private void OnDisable()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldAdded -= SpawnPopup;
    }

    private void SpawnPopup(int total, int gained, Vector3 worldPos)
    {
        var popup = Instantiate(_popupPrefab, _canvas.transform);
        popup.Play(gained, worldPos);
    }
}