using Mirror;
using UnityEngine;

/// <summary>
/// 敵ステータス（サーバー管理）
/// </summary>
public class EnemyStatus : CreatureBase {
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
    /// 被弾・死亡判定関数
    /// </summary>
    [Server]
    public override void TakeDamage(int _damage, string _name, int _ID) {
        //既に死亡状態かロビー内なら帰る
        // if (parameter.isDead || !GameManager.Instance.IsGameRunning()) return;
        Debug.Log("被弾");
        //ダメージ倍率を適用
        float damage = _damage;
        //ダメージが0以下だったら1に補正する
        if (damage <= 0) damage = 1;
        //HPの減算処理
        currentHp -= (int)damage;

        // hitSE 再生
        PlayHitSE(_ID);

        if (currentHp <= 0) {
            currentHp = 0;

            EnemyDie();

            if (PlayerListManager.Instance != null) {
                // スコア加算
                PlayerListManager.Instance.AddScoreById(_ID, 100);
                PlayerListManager.Instance.AddKillById(_ID);
            }
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
