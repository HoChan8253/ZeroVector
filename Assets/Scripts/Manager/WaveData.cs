using System;
using UnityEngine;

[CreateAssetMenu(menuName = "LoopSO/Wave Data", fileName = "WD_Wave")]
public class WaveData : ScriptableObject
{
    [Serializable]
    public class EnemyEntry
    {
        public GameObject prefab;

        [Tooltip("고정 수량 모드일 때 사용 (0이면 가중치 모드)")]
        public int fixedCount = 0;

        [Tooltip("가중치 모드일 때 사용 (fixedCount > 0 이면 무시)")]
        [Min(1)] public int weight = 1;
    }

    [Header("Wave Info")]
    public int waveNumber;

    [Header("낮 필드 유지")]
    [Tooltip("낮에 필드에 유지할 목표 적 수")]
    public int dayFieldTarget = 10;

    [Tooltip("낮에 등장 가능한 적 목록")]
    public EnemyEntry[] dayEntries;

    [Header("밤 리젠")]
    [Tooltip("밤에 리젠 가능한 적 목록")]
    public EnemyEntry[] nightEntries;

    [Header("스폰 간격")]
    public float spawnInterval = 0.4f;

    // 유틸
    public GameObject[] PickPrefabs(EnemyEntry[] entries, int count)
    {
        if (entries == null || entries.Length == 0 || count <= 0)
            return Array.Empty<GameObject>();

        var result = new GameObject[count];

        // 고정 수량 모드 : 적어도 하나라도 fixedCount > 0 이면 고정 배분
        bool hasFixed = System.Array.Exists(entries, e => e.fixedCount > 0);

        if (hasFixed)
        {
            // 고정 수량대로 리스트 구성 후 순서 셔플
            var pool = new System.Collections.Generic.List<GameObject>();
            foreach (var e in entries)
            {
                int n = e.fixedCount > 0 ? e.fixedCount : 0;
                for (int i = 0; i < n; i++)
                    pool.Add(e.prefab);
            }
            // pool 이 count 보다 짧으면 반복, 길면 자름
            for (int i = 0; i < count; i++)
                result[i] = pool[i % pool.Count];
        }
        else
        {
            // 가중치 추첨
            int totalWeight = 0;
            foreach (var e in entries) totalWeight += e.weight;

            for (int i = 0; i < count; i++)
            {
                int roll = UnityEngine.Random.Range(0, totalWeight);
                int acc = 0;
                foreach (var e in entries)
                {
                    acc += e.weight;
                    if (roll < acc) { result[i] = e.prefab; break; }
                }
            }
        }
        return result;
    }
}