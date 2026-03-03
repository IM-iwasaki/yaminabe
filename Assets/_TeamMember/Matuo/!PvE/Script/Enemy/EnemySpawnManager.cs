using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// 敵スポーン全体を管理するクラス
/// </summary>
public class EnemySpawnManager : NetworkBehaviour {

    public static EnemySpawnManager Instance;

    [Header("全体制御")]
    public float spawnInterval = 2.0f;      // スポーン試行間隔（秒）

    private float timer = 0f;               // 時間計測用

    private List<EnemySpawnPoint> spawnPoints = new(); // Scene内スポナー一覧

    private bool bossAlive = false;

    [Header("ボス再出現設定")]
    public float bossRespawnDelay = 60f;

    private float bossRespawnTimer = 0f;

    private void Awake() {
        Instance = this;
    }

    [Server]
    private void Start() {
        // シーン内に配置されたスポナーを全取得
        spawnPoints.AddRange(FindObjectsOfType<EnemySpawnPoint>());
    }

    [ServerCallback]
    private void Update() {

        if (!GameManager.Instance.IsGameRunning()) return;

        // ボス死亡中ならタイマー進行
        if (!bossAlive && bossRespawnTimer > 0f) {
            bossRespawnTimer -= Time.deltaTime;

            if (bossRespawnTimer <= 0f) {
                // 再スポーン可能にする
                bossRespawnTimer = 0f;
            }
        }

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

            // 生きているなら湧かない
            if (bossAlive)
                return;

            // タイマー中なら湧かない
            if (bossRespawnTimer > 0f)
                return;

            SpawnEnemy(sp);
            bossAlive = true;

            return;
        }

        if (sp.currentSpawnCount >= sp.CurrentMaxSpawnCount)
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

        GameObject enemyObj = Instantiate(data.enemyPrefab, sp.transform.position, Quaternion.identity);

        NetworkServer.Spawn(enemyObj);

        // ステータスデータを敵に設定
        var status = enemyObj.GetComponent<EnemyStatusBase>();
        if (status != null) {
            status.statusData = data;
            status.SetSpawnPoint(sp);
        }

        sp.currentSpawnCount++;
    }

    /// <summary>
    /// 敵死亡時に EnemyBase などから呼ぶ
    /// </summary>
    [Server]
    public void NotifyEnemyDead(EnemySpawnPoint sp) {
        sp.currentSpawnCount--;

        if (sp.isBossSpawnPoint) {
            bossAlive = false;
            bossRespawnTimer = bossRespawnDelay;

            if (sp.removeOnBossDeath != null && sp.removeOnBossDeath.gameObject.activeSelf) {
                sp.removeOnBossDeath.RpcExecute();
            }
        }
    }
}