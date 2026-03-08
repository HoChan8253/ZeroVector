using UnityEngine;

public class GoldPopupSpawner : MonoBehaviour
{
    [SerializeField] private GoldPopup _popupPrefab;
    [SerializeField] private Canvas _canvas;
    [SerializeField] private Camera _cam;

    private void Start()
    {
        if (_cam == null) _cam = Camera.main;

        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldAdded += SpawnPopup;
    }

    private void OnDestroy()
    {
        if (GoldManager.Instance != null)
            GoldManager.Instance.OnGoldAdded -= SpawnPopup;
    }

    private void SpawnPopup(int total, int gained, Vector3 worldPos)
    {
        var popup = Instantiate(_popupPrefab, _canvas.transform);
        popup.Play(gained, worldPos, _cam);  // ← 카메라 전달
    }
}