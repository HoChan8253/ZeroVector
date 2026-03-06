using UnityEngine;

public class PoolPreloader : MonoBehaviour
{
    [SerializeField] private int _energyBallCount = 60;
    [SerializeField] private int _hitFxCount = 30;

    private void Start()
    {
        if (ObjectPoolManager.Instance == null) return;

        ObjectPoolManager.Instance.Preload(PoolKey.EnergyBall, _energyBallCount);
        ObjectPoolManager.Instance.Preload(PoolKey.HitFx_ElectricShort, _hitFxCount);
    }
}