using UnityEngine;

public class PoolPreloader : MonoBehaviour
{
    [SerializeField] private int _energyBallCount = 60;
    [SerializeField] private int _hitFxCount = 30;
    [SerializeField] private int _bossProjStraightCount = 20;
    [SerializeField] private int _bossProjHomingCount = 10;

    private void Start()
    {
        if (ObjectPoolManager.Instance == null) return;

        ObjectPoolManager.Instance.Preload(PoolKey.EnergyBall, _energyBallCount);
        ObjectPoolManager.Instance.Preload(PoolKey.HitFx_ElectricShort, _hitFxCount);
        ObjectPoolManager.Instance.Preload(PoolKey.BossProj_Straight, _bossProjStraightCount);
        ObjectPoolManager.Instance.Preload(PoolKey.BossProj_Homing, _bossProjHomingCount);
    }
}