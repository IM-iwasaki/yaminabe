using UnityEngine;
using Mirror;
using System.Collections.Generic;

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
                obj.SetActive(false);
                NetworkServer.Spawn(obj);
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
        queue.Enqueue(obj); // Ä—˜—p‚Ì‚½‚ß––”ö‚É–ß‚·
        return obj;
    }
}
