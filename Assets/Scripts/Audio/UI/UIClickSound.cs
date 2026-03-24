using UnityEngine;
using UnityEngine.UI;

public class UIClickSound : MonoBehaviour
{
    private void Awake()
    {
        foreach (var btn in GetComponentsInChildren<Button>(true))
            btn.onClick.AddListener(() => UIAudioManager.PlaySound(SoundType.UI_Click));
    }
}