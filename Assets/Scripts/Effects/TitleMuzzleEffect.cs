using UnityEngine;
using System.Collections;

public class TitleMuzzleEffect : MonoBehaviour
{
    [Header("총구 오브젝트들 (Muzzle 빈 오브젝트)")]
    public Transform[] muzzlePoints;

    [Header("총알 프리팹 (Trail Renderer 붙은 것)")]
    public GameObject bulletPrefab;

    [Header("발사 설정")]
    public int burstCount = 5;
    public float fireRate = 0.08f;
    public float bulletSpeed = 40f;
    public float bulletLifetime = 1.5f;
    public float spread = 2.5f;

    [Header("발사 루프 간격")]
    public float minInterval = 2f;
    public float maxInterval = 4f;

    [Header("Point Light 설정")]
    public float lightDuration = 0.05f;
    public float lightIntensity = 3f;

    void Start()
    {
        foreach (var muzzle in muzzlePoints)
        {
            StartCoroutine(FireLoop(muzzle));
        }
    }

    IEnumerator FireLoop(Transform muzzle)
    {
        // 시작 타이밍 어긋나게
        yield return new WaitForSeconds(Random.Range(0f, 3f));

        while (true)
        {
            yield return StartCoroutine(FireBurst(muzzle));
            yield return new WaitForSeconds(Random.Range(minInterval, maxInterval));
        }
    }

    IEnumerator FireBurst(Transform muzzle)
    {
        ParticleSystem muzzleFlash = muzzle.GetComponentInChildren<ParticleSystem>();

        Light pointLight = muzzle.GetComponentInChildren<Light>();
        if (pointLight != null)
        {
            pointLight.intensity = lightIntensity;
            pointLight.enabled = false;
        }

        for (int i = 0; i < burstCount; i++)
        {
            // 파티클 재생
            if (muzzleFlash != null)
                muzzleFlash.Play();

            // 라이트 깜빡임
            if (pointLight != null)
                StartCoroutine(FlashLight(pointLight));

            // 탄착군 적용한 발사 방향
            Vector3 direction = GetSpreadDirection(muzzle.forward, spread);

            // 총알 생성
            if (bulletPrefab != null)
            {
                GameObject bullet = Instantiate(bulletPrefab, muzzle.position, Quaternion.LookRotation(direction));
                Rigidbody rb = bullet.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.linearVelocity = direction * bulletSpeed;
                }
                else
                {
                    // Rigidbody 없으면 코루틴으로 이동
                    StartCoroutine(MoveBullet(bullet, direction));
                }

                Destroy(bullet, bulletLifetime);
            }

            yield return new WaitForSeconds(fireRate);
        }
    }

    Vector3 GetSpreadDirection(Vector3 baseDir, float spreadAngle)
    {
        float x = Random.Range(-spreadAngle, spreadAngle);
        float y = Random.Range(-spreadAngle, spreadAngle);
        return Quaternion.Euler(x, y, 0) * baseDir;
    }

    IEnumerator FlashLight(Light light)
    {
        light.enabled = true;
        yield return new WaitForSeconds(lightDuration);
        light.enabled = false;
    }

    IEnumerator MoveBullet(GameObject bullet, Vector3 direction)
    {
        while (bullet != null)
        {
            bullet.transform.position += direction * bulletSpeed * Time.deltaTime;
            yield return null;
        }
    }
}