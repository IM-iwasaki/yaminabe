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
    public override void TakeDamage(int damage, string attackerName, int attackerID) {

        // 共通ダメージ処理（HP減算・SE・スコア加算）
        base.TakeDamage(damage, attackerName, attackerID);

        RpcUpdateView(damage);

        // HPが0以下なら死亡
        if (parameter.HP <= 0) {
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

        healthView.UpdateHP(parameter.HP);
        healthView.ShowDamage(damage);
    }

    /// <summary>
    /// 敵の死亡処理
    /// </summary>
    [Server]
    private void Die() {

        parameter.isDead = true;   // 死亡フラグ

        // 死亡演出などはここに

        NetworkServer.Destroy(gameObject); // サーバーから削除
    }
}