//using Mirror;
//using UnityEngine;
//using System.Collections.Generic;

///// <summary>
///// 敵オブジェクトプール（サーバー管理）
///// 敵の生成・再利用を担当する
///// </summary>
//public class EnemyPool : NetworkBehaviour {
//    [Header("プール設定")]
//    public GameObject enemyPrefab; // NetworkIdentity付き敵Prefab
//    public int initialPoolSize = 10;

//    // 未使用の敵キュー
//    private Queue<GameObject> pool = new();

//    public override void OnStartServer() {
//        // サーバー起動時にまとめて生成
//        for (int i = 0; i < initialPoolSize; i++) {
//            CreateEnemy();
//        }
//    }

//    /// <summary>
//    /// 敵を新規生成してプールに追加
//    /// </summary>
//    [Server]
//    void CreateEnemy() {
//        GameObject enemy = Instantiate(enemyPrefab);

//        // 非表示状態で待機
//        enemy.SetActive(false);

//        // ネットワークに登録（Destroyしない前提）
//        NetworkServer.Spawn(enemy);

//        pool.Enqueue(enemy);
//    }

//    /// <summary>
//    /// プールから敵を取得
//    /// </summary>
//    [Server]
//    public GameObject Get(Vector3 position) {
//        // 足りなければ追加生成
//        if (pool.Count == 0) {
//            CreateEnemy();
//        }

//        GameObject enemy = pool.Dequeue();

//        // 位置設定
//        enemy.transform.position = position;

//        // ステータス初期化（重要）
//        var status = enemy.GetComponent<EnemyStatusBase>();
//        status.ResetStatus();

//        enemy.SetActive(true);

//        return enemy;
//    }

//    /// <summary>
//    /// 敵をプールに戻す
//    /// </summary>
//    [Server]
//    public void Return(GameObject enemy) {
//        enemy.SetActive(false);
//        pool.Enqueue(enemy);
//    }
//}
