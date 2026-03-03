using UnityEngine;

/// <summary>
/// シーン上に配置する敵スポーンポイント
/// </summary>
public class EnemySpawnPoint : MonoBehaviour {

    [Header("生成する敵データ")]
    public EnemyStatusBaseData enemyStatus;     // 敵データ

    [Header("スポーン制限")]
    public int baseMaxSpawnCount = 5;            // このスポナーから同時に湧ける最大数
    [HideInInspector]
    public int currentSpawnCount = 0;           // 現在このスポナーから湧いている数

    [Header("起動条件")]
    public float activateRadius = 20f;          // プレイヤー検知距離

    [Header("スポーン間隔")]
    public float spawnInterval = 2f;   // このスポナーの試行間隔
    [HideInInspector]
    public float timer = 0f;           // 経過時間

    [Header("ボス設定")]
    public bool isBossSpawnPoint = false;   // このスポナーはボス用か

    public int CurrentMaxSpawnCount {
        get {
            int playerCount = Mirror.NetworkServer.connections.Count;
            playerCount = Mathf.Max(1, playerCount);

            return Mathf.RoundToInt(baseMaxSpawnCount * (1f + 0.5f * (playerCount - 1)));
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected() {
        // 起動範囲をScene上で可視化
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, activateRadius);
    }
#endif
}