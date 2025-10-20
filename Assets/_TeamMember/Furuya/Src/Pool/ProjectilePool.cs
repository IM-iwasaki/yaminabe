using UnityEngine;
using Mirror;
using System.Collections.Generic;
using System.Collections;

public class ProjectilePool : NetworkBehaviour {
    public static ProjectilePool Instance;

    [System.Serializable]
    public class PoolData {
        public GameObject prefab;
        public int size = 20;
    }

    public List<PoolData> poolDataList;
    private readonly Dictionary<GameObject, Queue<GameObject>> pools = new();

    void Awake() {
        if (Instance == null) Instance = this;
    }

    public override void OnStartServer() {
        foreach (var poolData in poolDataList) {
            var queue = new Queue<GameObject>();
            for (int i = 0; i < poolData.size; i++) {
                GameObject obj = Instantiate(poolData.prefab);
                NetworkServer.Spawn(obj);

                // まず非表示にしてから Spawn
                obj.SetActive(false);

                queue.Enqueue(obj);
            }
            pools.Add(poolData.prefab, queue);
        }
    }

    [Server]
    public GameObject GetFromPool(GameObject prefab, Vector3 pos, Quaternion rot) {
        if (!pools.TryGetValue(prefab, out var queue)) return null;

        GameObject obj = queue.Dequeue();
        obj.transform.SetPositionAndRotation(pos, rot);
        obj.SetActive(true);
        queue.Enqueue(obj); // 再利用のため末尾に戻す
        return obj;
    }

    [Server]
    public void ReturnToPool(GameObject obj) {
        if (obj == null) return;

        // Rigidbody があれば停止
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        ResetObject(obj);
        obj.SetActive(false);
    }

    [Server]
    public void ReturnToPool(GameObject obj, float delay) {
        if (obj == null) return;
        StartCoroutine(ReturnCoroutine(obj, delay));
    }

    private IEnumerator ReturnCoroutine(GameObject obj, float delay) {
        yield return new WaitForSeconds(delay);
        if (obj == null) yield break;

        ResetObject(obj);
        obj.SetActive(false);
    }

    // -------------------------------
    // 共通リセット処理
    // -------------------------------
    private void ResetObject(GameObject obj) {
        // Rigidbody があれば停止
        var rb = obj.GetComponent<Rigidbody>();
        if (rb != null) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
