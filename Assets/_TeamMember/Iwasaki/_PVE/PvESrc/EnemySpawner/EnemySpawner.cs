using Mirror;
using UnityEngine;
using System.Collections;

/// <summary>
/// 敵スポナー（サーバー専用）
/// 敵の出現管理・最大数制御
/// </summary>
public class EnemySpawner : NetworkBehaviour {
    [Header("参照")]
    public EnemyPool enemyPool;

    [Header("スポーン設定")]
    public float spawnInterval = 3f; // 出現間隔
    public int maxAlive = 5;          // 同時出現上限

    private int aliveCount = 0;

    public override void OnStartServer() {
        // サーバーのみスポーン開始
        StartCoroutine(SpawnLoop());
    }

    /// <summary>
    /// 定期的に敵を出現させる
    /// </summary>
    IEnumerator SpawnLoop() {
        while (true) {
            if (aliveCount < maxAlive) {
                SpawnEnemy();
            }

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// 敵を1体スポーン
    /// </summary>
    [Server]
    void SpawnEnemy() {
        GameObject enemy = enemyPool.Get(transform.position);

        var status = enemy.GetComponent<EnemyStatusBase>();

        // 死亡通知を受け取る
        status.onDeath += OnEnemyDeath;

        aliveCount++;
    }

    /// <summary>
    /// 敵死亡時の処理
    /// </summary>
    [Server]
    void OnEnemyDeath(EnemyStatusBase enemyStatus) {
        // 二重登録防止
        enemyStatus.onDeath -= OnEnemyDeath;

        aliveCount--;

        // プールへ戻す
        enemyPool.Return(enemyStatus.gameObject);
    }
}
