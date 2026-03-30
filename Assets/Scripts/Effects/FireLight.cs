using UnityEngine;

public class FireLight : MonoBehaviour
{
    Light fireLight;

    [Header("Flicker Settings")]
    public float baseIntensity = 3f;
    public float flickerAmount = 1f;
    public float flickerSpeed = 8f;

    void Start()
    {
        fireLight = GetComponent<Light>();
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * flickerSpeed, 0f);
        fireLight.intensity = baseIntensity + (noise - 0.5f) * flickerAmount;
    }
}