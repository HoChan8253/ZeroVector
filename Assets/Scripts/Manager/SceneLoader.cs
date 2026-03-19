using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance { get; private set; }

    [Header("로딩 UI")]
    [SerializeField] private GameObject _loadingPanel;
    [SerializeField] private Slider _progressBar;
    [SerializeField] private TextMeshProUGUI _progressText;

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        _loadingPanel?.SetActive(false);
    }

    public void LoadScene(string sceneName)
    {
        StartCoroutine(CoLoad(sceneName));
    }

    private IEnumerator CoLoad(string sceneName)
    {
        _loadingPanel?.SetActive(true);

        yield return null; // 한 프레임 대기

        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            // 0 ~ 0.9 범위를 0 ~ 1로 변환
            float progress = Mathf.Clamp01(async.progress / 0.9f);

            if (_progressBar) _progressBar.value = progress;
            if (_progressText) _progressText.text = $"{(int)(progress * 100f)}%";

            // 로드 완료
            if (async.progress >= 0.9f)
            {
                if (_progressText) _progressText.text = "100%";
                if (_progressBar) _progressBar.value = 1f;

                yield return new WaitForSeconds(0.5f); // 잠깐 대기 후 전환
                async.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}