using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// 敵スポーン全体を管理するクラス
/// </summary>
public class EnemySpawnManager : NetworkBehaviour {

    public static EnemySpawnManager Instance;

    [Header("全体制御")]
    public int maxEnemyCount = 30;          // ステージ全体の敵最大数
    public float spawnInterval = 2.0f;      // スポーン試行間隔（秒）

    private float timer = 0f;               // 時間計測用
    private int currentEnemyCount = 0;      // 現在存在する敵数

    private List<EnemySpawnPoint> spawnPoints = new(); // Scene内スポナー一覧

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
        // ゲーム中でなければ何もしない
        if (!GameManager.Instance.IsGameRunning()) return;

        timer += Time.deltaTime;
        if (timer < spawnInterval) return;

        timer = 0f;
        TrySpawnEnemy();
    }

    /// <summary>
    /// スポーン可能なポイントを探して敵を生成
    /// </summary>
    [Server]
    private void TrySpawnEnemy() {

        // 全体数制限
        if (currentEnemyCount >= maxEnemyCount)
            return;

        foreach (var sp in spawnPoints) {

            // スポナー個別の上限
            if (sp.currentSpawnCount >= sp.maxSpawnCount)
                continue;

            // プレイヤーが近くにいない
            if (!SpawnUtility.IsAnyPlayerInRange(
                    sp.transform.position,
                    sp.activateRadius))
                continue;

            // 視界チェック
            if (!SpawnUtility.CanSpawnOutOfPlayerView(sp.transform.position))
                continue;

            // 条件を満たしたのでスポーン
            SpawnEnemy(sp);
            break; // 1回のUpdateで1体だけ
        }
    }

    /// <summary>
    /// 実際の敵生成処理
    /// </summary>
    [Server]
    private void SpawnEnemy(EnemySpawnPoint sp) {

        var data = sp.enemyStatus;
        if (data == null || data.enemyPrefab == null)
            return;

        GameObject enemyObj = Instantiate(
            data.enemyPrefab,
            sp.transform.position,
            Quaternion.identity
        );

        NetworkServer.Spawn(enemyObj);

        // ステータスデータを敵に設定
        var status = enemyObj.GetComponent<EnemyStatusBase>();
        if (status != null) {
            status.statusData = data;
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
    }
}