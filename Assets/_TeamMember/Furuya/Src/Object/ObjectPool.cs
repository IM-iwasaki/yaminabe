using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プールの使い方例
/// スポーンさせるプレハブに付けた名前、出現位置、回転を入力すること
/// ObjectPool.Instance.SpawnFromPool("HitEffect", hitPosition, Quaternion.identity);
/// ObjectPool.Instance.SpawnFromPool("MuzzleFlash", firePoint.position, firePoint.rotation);
/// </summary>

public class ObjectPool : MonoBehaviour {
    public static ObjectPool Instance;

    [System.Serializable]
    public class Pool {
        public string tag;
        public GameObject prefab;
        public int size;
    }

    public List<Pool> pools;
    private Dictionary<string, Queue<GameObject>> poolDictionary;

    void Awake() {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start() {
        poolDictionary = new Dictionary<string, Queue<GameObject>>();

        foreach (Pool pool in pools) {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int i = 0; i < pool.size; i++) {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);
                obj.transform.SetParent(transform); // Hierarchy整理
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(pool.tag, objectPool);
        }
    }

    public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation) {
        if (!poolDictionary.ContainsKey(tag)) {
            Debug.LogWarning($"Pool with tag {tag} doesn't exist!");
            return null;
        }

        GameObject objToSpawn = poolDictionary[tag].Dequeue();
        objToSpawn.SetActive(true);
        objToSpawn.transform.SetPositionAndRotation(position, rotation);

        poolDictionary[tag].Enqueue(objToSpawn);

        return objToSpawn;
    }
}
