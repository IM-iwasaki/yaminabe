using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// 敵スポーン全体を管理するクラス
/// </summary>
public class EnemySpawnManager : NetworkBehaviour {

    public static EnemySpawnManager Instance;

    [Header("全体制御")]
    public int maxEnemyCount = 20;          // ステージ全体の敵最大数
    public float spawnInterval = 2.0f;      // スポーン試行間隔（秒）

    private float timer = 0f;               // 時間計測用
    private int currentEnemyCount = 0;      // 現在存在する敵数

    private List<EnemySpawnPoint> spawnPoints = new(); // Scene内スポナー一覧

    private bool bossAlive = false;

    private void Awake() {
        Instance = this;
    }

    [Server]
    private void Start() {
        // シーン内に配置されたスポナーを全取得
        spawnPoints.AddRange(FindObjectsOfType<EnemySpawnPoint>());
    }

    private int CurrentMaxEnemyCount {
        get {
            int playerCount = NetworkServer.connections.Count;
            return maxEnemyCount * Mathf.Max(1, playerCount);
        }
    }

    [ServerCallback]
    private void Update() {

        if (!GameManager.Instance.IsGameRunning()) return;

        foreach (var sp in spawnPoints) {

            // まだ1体も出していないなら即スポーン試行
            if (sp.currentSpawnCount == 0) {
                TrySpawnEnemy(sp);
                continue;
            }

            sp.timer += Time.deltaTime;

            if (sp.timer < sp.spawnInterval)
                continue;

            sp.timer = 0f;

            TrySpawnEnemy(sp);
        }
    }

    /// <summary>
    /// スポーン可能なポイントを探して敵を生成
    /// </summary>
    [Server]
    private void TrySpawnEnemy(EnemySpawnPoint sp) {
        // ボス用スポナーの場合
        if (sp.isBossSpawnPoint) {
            // 既にボスが居るなら生成しない
            if (bossAlive)
                return;

            SpawnEnemy(sp);
            bossAlive = true;
            return;
        }

        // 通常スポナーの場合

        // ボス用に1枠空ける
        if (currentEnemyCount >= CurrentMaxEnemyCount - 1)
            return;

        if (sp.currentSpawnCount >= sp.maxSpawnCount)
            return;

        if (!SpawnUtility.IsAnyPlayerInRange(sp.transform.position, sp.activateRadius))
            return;

        if (!SpawnUtility.CanSpawnOutOfPlayerView(sp.transform.position))
            return;

        SpawnEnemy(sp);
    }

    /// <summary>
    /// 実際の敵生成処理
    /// </summary>
    [Server]
    private void SpawnEnemy(EnemySpawnPoint sp) {

        var data = sp.enemyStatus;
        if (data == null || data.enemyPrefab == null)
            return;

        GameObject enemyObj = Instantiate(data.enemyPrefab,sp.transform.position,Quaternion.identity);

        NetworkServer.Spawn(enemyObj);

        // ステータスデータを敵に設定
        var status = enemyObj.GetComponent<EnemyStatusBase>();
        if (status != null) {
            status.statusData = data;
            status.SetSpawnPoint(sp);
        }

        sp.currentSpawnCount++;
        currentEnemyCount++;
    }

    /// <summary>
    /// 敵死亡時に EnemyBase などから呼ぶ
    /// </summary>
    [Server]
    public void NotifyEnemyDead(EnemySpawnPoint sp) {
        currentEnemyCount--;
        sp.currentSpawnCount--;

        if (sp.isBossSpawnPoint) {
            bossAlive = false;
        }
    }
}