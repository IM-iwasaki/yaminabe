using Mirror;
using UnityEngine;

/// <summary>
/// 敵ステータス（サーバー管理）
/// </summary>
public class EnemyStatus : NetworkBehaviour {
    [Header("ステータスデータ")]
    public EnemyStatusData statusData;

    [SyncVar]
    private int currentHp;

    public override void OnStartServer() {
        if (statusData == null) {
            Debug.LogError($"{name} に EnemyStatusData が設定されていません");
            return;
        }

        currentHp = statusData.maxHp;
    }

    /// <summary>
    /// 敵がダメージを受ける（サーバー専用）
    /// </summary>
    [Server]
    public void ReceiveDamage(int damage) {
        currentHp -= damage;

        if (currentHp <= 0) {
            EnemyDie();
        }
    }

    [Server]
    void EnemyDie() {
        NetworkServer.Destroy(gameObject);
    }

    // 参照用
    public int GetCurrentHp() => currentHp;
    public int GetMaxHp() => statusData.maxHp;
    public int GetAttack() => statusData.attack;
    public float GetMoveSpeed() => statusData.moveSpeed;
}
