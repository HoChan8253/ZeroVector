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
    [SerializeField] private CanvasGroup _loadingCanvasGroup;

    [Header("페이드 설정")]
    [SerializeField] private float _fadeDuration = 0.8f;

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
        if (_loadingCanvasGroup != null)
            _loadingCanvasGroup.alpha = 0f;

        // 알파 페이드 인
        float t = 0f;
        while (t < _fadeDuration)
        {
            t += Time.deltaTime;
            if (_loadingCanvasGroup != null)
                _loadingCanvasGroup.alpha = Mathf.Lerp(0f, 1f, t / _fadeDuration);
            yield return null;
        }
        if (_loadingCanvasGroup != null)
            _loadingCanvasGroup.alpha = 1f;

        // 로딩 시작
        yield return null;
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        async.allowSceneActivation = false;

        while (!async.isDone)
        {
            float progress = Mathf.Clamp01(async.progress / 0.9f);
            if (_progressBar) _progressBar.value = progress;
            if (_progressText) _progressText.text = $"{(int)(progress * 100f)}%";

            if (async.progress >= 0.9f)
            {
                if (_progressText) _progressText.text = "100%";
                if (_progressBar) _progressBar.value = 1f;
                yield return new WaitForSeconds(0.5f);
                async.allowSceneActivation = true;
            }

            yield return null;
        }
    }
}