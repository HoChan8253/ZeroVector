using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TitleLightning : MonoBehaviour
{
    [Header("Post Processing")]
    [SerializeField] private Volume _globalVolume;

    [Header("번개 설정")]
    [SerializeField] private float _minInterval = 3f;
    [SerializeField] private float _maxInterval = 10f;
    [SerializeField] private float _minIntensity = 1.5f;
    [SerializeField] private float _maxIntensity = 3f;
    [SerializeField] private int _minFlashes = 2;
    [SerializeField] private int _maxFlashes = 4;

    private ColorAdjustments _colorAdjustments;

    private void Start()
    {
        if (_globalVolume != null)
            _globalVolume.profile.TryGet(out _colorAdjustments);

        StartCoroutine(CoLightningLoop());
    }

    private IEnumerator CoLightningLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(_minInterval, _maxInterval));
            yield return StartCoroutine(CoFlash());
        }
    }

    private IEnumerator CoFlash()
    {
        int flashes = Random.Range(_minFlashes, _maxFlashes + 1);
        for (int i = 0; i < flashes; i++)
        {
            float intensity = Random.Range(_minIntensity, _maxIntensity);

            // 디버그
            Debug.Log($"[Lightning] postExposure ON: {intensity} / colorAdj:{_colorAdjustments != null}");

            if (_colorAdjustments != null)
                _colorAdjustments.postExposure.Override(intensity);

            yield return new WaitForSeconds(Random.Range(0.05f, 0.1f));

            if (_colorAdjustments != null)
                _colorAdjustments.postExposure.Override(0f);

            Debug.Log("[Lightning] postExposure OFF");

            yield return new WaitForSeconds(Random.Range(0.05f, 0.15f));
        }
    }
}