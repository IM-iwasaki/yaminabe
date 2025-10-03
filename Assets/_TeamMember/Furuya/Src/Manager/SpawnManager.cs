using UnityEngine;
using Mirror;

/// <summary>
/// サーバー側で一つだけ持つ生成マネージャー
/// プレハブは Inspector にアサインし、NetworkSystemPrefabs にこの SpawnManager のプレハブを登録しておく。
/// </summary>
public class SpawnManager : NetworkSystemObject<SpawnManager> {
    [Header("生成用プレハブ")]
    public GameObject bulletPrefab;
    public GameObject enemyPrefab;
    public GameObject playerPrefab;

    public override void Initialize() {
        base.Initialize();
        Debug.Log("SpawnManager (server) 初期化完了");
    }

    /// <summary>サーバーでのみ呼べる汎用 Spawn</summary>
    [Server]
    public GameObject SpawnObject(GameObject prefab, Vector3 pos, Quaternion rot) {
        if (prefab == null) return null;
        GameObject obj = Instantiate(prefab, pos, rot);
        NetworkServer.Spawn(obj);
        return obj;
    }

    [Server]
    public void DestroyNetworkObject(GameObject go) {
        if (go == null) return;
        NetworkServer.Destroy(go);
    }

    [Server]
    public GameObject SpawnPlayerForConnection(NetworkConnectionToClient conn, Vector3 pos, Quaternion rot) {
        GameObject player = Instantiate(playerPrefab, pos, rot);
        NetworkServer.AddPlayerForConnection(conn, player);
        return player;
    }

    // 便利メソッド例
    [Server]
    public GameObject SpawnBullet(Vector3 pos, Quaternion rot) {
        return SpawnObject(bulletPrefab, pos, rot);
    }
}
