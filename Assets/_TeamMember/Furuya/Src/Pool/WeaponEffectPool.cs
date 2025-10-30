using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponEffectPool : NetworkBehaviour {
    public static WeaponEffectPool Instance;

    [System.Serializable]
    public class PoolItem {
        //public string name;
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
            poolDictionary.Add(pool.prefab.name, objectPool);
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

    // 既存 Get をラップ：Prefab から直接取得
    public GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot) {
        return Get(prefab.name, pos, rot);
    }

    // 秒数付きで自動で戻す
    public void ReturnToPool(GameObject obj, float delay) {
        StartCoroutine(ReturnCoroutine(obj, delay));
    }

    private IEnumerator ReturnCoroutine(GameObject obj, float delay) {
        yield return new WaitForSeconds(delay);
        obj.SetActive(false);

        // Rigidbody があれば停止
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // ParticleSystem があれば停止
        var ps = obj.GetComponent<ParticleSystem>();
        if (ps != null)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
    }
}
