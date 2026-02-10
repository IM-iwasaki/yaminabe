using Mirror;
using UnityEngine;

/// <summary>
/// 敵専用ステータス管理
/// ダメージ処理・死亡処理を担当
/// </summary>
public class EnemyStatusBase : CreatureBase {

    [Header("敵のステータスデータ")]
    public EnemyStatusBaseData statusData;    // 敵のステータスデータ
    public EnemyParameter enemyParameter { get; private set; }

    private EnemyHealthView healthView;

    protected override void Awake() {
        base.Awake();
        enemyParameter = GetComponent<EnemyParameter>();
        healthView = GetComponent<EnemyHealthView>();
    }

    public override void OnStartServer() {
        if (statusData == null || enemyParameter == null) return;

        enemyParameter.HP = statusData.maxHp;
        enemyParameter.attack = statusData.attack;
        enemyParameter.moveSpeed = (int)statusData.moveSpeed;
    }

    /// <summary>
    /// ダメージ処理（敵専用）
    /// </summary>
    [Server]
    public override void TakeDamage(int _damage, string attackerName, int attackerID) {
        //既に死亡状態かロビー内なら帰る
        if (enemyParameter.isDead) return;

        //ダメージ倍率を適用
        float damage = _damage;
        //ダメージが0以下だったら1に補正する
        if (damage <= 0) damage = 1;
        //HPの減算処理
        enemyParameter.HP -= (int) damage;

        // hitSE 再生
        PlayHitSE(attackerID);

        if (enemyParameter.HP <= 0) {
            enemyParameter.HP = 0;

            if (PlayerListManager.Instance != null) {
                // スコア加算
                PlayerListManager.Instance.AddScoreById(attackerID, 100);
                PlayerListManager.Instance.AddKillById(attackerID);
            }
        }
        RpcUpdateView(_damage);

        // HPが0以下なら死亡
        if (enemyParameter.HP <= 0) {
            Die();
        }
    }

    /// <summary>
    /// ダメージテキスト、HPUI表示用
    /// </summary>
    /// <param name="damage"></param>
    [ClientRpc]
    private void RpcUpdateView(int damage) {
        if (healthView == null) return;

        healthView.UpdateHP(enemyParameter.HP);
        healthView.ShowDamage(damage);
    }

    /// <summary>
    /// 敵の死亡処理
    /// </summary>
    [Server]
    private void Die() {

        enemyParameter.isDead = true;   // 死亡フラグ

        // 死亡演出などはここに

        NetworkServer.Destroy(gameObject); // サーバーから削除
    }
}