using UnityEngine;
using Mirror;

/// <summary>
/// サーバー側で一つだけ持つ生成マネージャー
/// プレハブは Inspector にアサインし、NetworkSystemPrefabs にこの SpawnManager のプレハブを登録しておく。
/// </summary>
public class SpawnManager : NetworkSystemObject<SpawnManager> {

    public override void Initialize() {
        base.Initialize();
    }

    /// <summary>サーバーでのみ呼べる汎用 Spawn</summary>
    [Server]
    public GameObject SpawnObject(GameObject prefab, Vector3 pos, Quaternion rot) {
        if (prefab == null) return null;
        GameObject obj = Instantiate(prefab, pos, rot);
        NetworkServer.Spawn(obj);
        return obj;
    }

    /// <summary>
    /// オブジェクト破棄用
    /// </summary>
    /// <param name="go">破棄するオブジェクト</param>
    [Server]
    public void DestroyNetworkObject(GameObject go) {
        if (go == null) return;
        NetworkServer.Destroy(go);
    }

}
