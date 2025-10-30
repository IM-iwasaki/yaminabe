using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectilePool : NetworkBehaviour {
    public static ProjectilePool Instance;

    [System.Serializable]
    public class PoolItem {
        [Tooltip("プール識別名")]
        [NonSerialized]public string name;
        public GameObject prefab;
        [Range(1, 100)]
        public int size = 10;
        [Tooltip("分類（弾、グレネード、設置物など）")]
        public ProjectileCategory category = ProjectileCategory.Other;
    }

    [Header("登録されたオブジェクトのプール一覧")]
    public List<PoolItem> pools = new();

    private readonly Dictionary<string, Queue<GameObject>> poolDictionary = new();

    void Awake() {
        Instance = this;
    }

    public override void OnStartServer() {
        foreach (var pool in pools) {
            string key = string.IsNullOrWhiteSpace(pool.name)
                ? pool.prefab.name
                : pool.name;

            if (poolDictionary.ContainsKey(key)) {
                Debug.LogWarning($"プール '{key}' は重複しています。スキップ。");
                continue;
            }

            Queue<GameObject> objectPool = new Queue<GameObject>();
            for (int i = 0; i < pool.size; i++) {
                GameObject obj = Instantiate(pool.prefab);
                obj.SetActive(false);

                // NetworkObjectが必須（Mirror）
                NetworkServer.Spawn(obj);

                objectPool.Enqueue(obj);
            }

            poolDictionary.Add(key, objectPool);
            Debug.Log($"[ProjectilePool] '{key}' を {pool.size} 個生成しました。");
        }
    }

    /// <summary>
    /// サーバーからオブジェクトを有効化して返す
    /// </summary>
    [Server]
    public GameObject SpawnFromPool(string name, Vector3 position, Quaternion rotation) {
        if (!poolDictionary.TryGetValue(name, out Queue<GameObject> pool)) {
            Debug.LogWarning($"[ProjectilePool] プール '{name}' は存在しません。");
            return null;
        }

        GameObject obj = pool.Dequeue();
        pool.Enqueue(obj);

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);

        // Mirror同期
        RpcSetActive(obj.GetComponent<NetworkIdentity>().netId, true);

        return obj;
    }

    /// <summary>
    /// サーバーからオブジェクトを非表示に戻す
    /// </summary>
    [Server]
    public void DespawnToPool(GameObject obj, float delay = 0f) {
        StartCoroutine(DespawnCoroutine(obj, delay));
    }

    private IEnumerator DespawnCoroutine(GameObject obj, float delay) {
        yield return new WaitForSeconds(delay);
        if (obj == null) yield break;

        //obj.SetActive(false);
        RpcSetActive(obj.GetComponent<NetworkIdentity>().netId, false);

        // Rigidbody停止
        if (obj.TryGetComponent(out Rigidbody rb)) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    // クライアントにも非表示／表示を同期
    [ClientRpc]
    private void RpcSetActive(uint netId, bool state) {
        if (NetworkClient.spawned.TryGetValue(netId, out NetworkIdentity id)) {
            id.gameObject.SetActive(state);
        }
    }

}
