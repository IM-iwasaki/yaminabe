using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// プールの使い方例
/// スポーンさせるプレハブに付けた名前、出現位置、回転を入力すること
/// ObjectPool.Instance.SpawnFromPool("HitEffect", hitPosition, Quaternion.identity);
/// ObjectPool.Instance.SpawnFromPool("MuzzleFlash", firePoint.position, firePoint.rotation);
/// </summary>

public class ObjectPoolManager : MonoBehaviour {
    public static ObjectPoolManager Instance;

    [System.Serializable]
    public class PoolItem {
        public string name;
        public GameObject prefab;
        public int size;
    }

    public List<PoolItem> pools;

    private Dictionary<string, Queue<GameObject>> poolDictionary = new();

    void Awake() {
        Instance = this;

        foreach (var pool in pools) {
            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++) {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }
            poolDictionary.Add(pool.name, objectPool);
        }
    }

    // プールから取得
    public GameObject Get(string name, Vector3 pos, Quaternion rot) {
        if (!poolDictionary.ContainsKey(name)) {
            Debug.LogWarning($"Pool {name} が存在しません");
            return null;
        }

        GameObject obj = poolDictionary[name].Dequeue();
        obj.transform.position = pos;
        obj.transform.rotation = rot;
        obj.SetActive(true);
        poolDictionary[name].Enqueue(obj);

        // ParticleSystemなら再生
        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null) {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            ps.Play();
        }

        return obj;
    }
}

