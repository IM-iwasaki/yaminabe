using Mirror;
using UnityEngine;
using System.Collections;

/// <summary>
/// 敵専用ステータス管理
/// ダメージ処理・死亡処理を担当
/// </summary>
public class EnemyStatusBase : CreatureBase {

    [Header("敵のステータスデータ")]
    public EnemyStatusBaseData statusData;    // 敵のステータスデータ
    public EnemyParameter enemyParameter { get; private set; }

    private EnemyHealthView healthView;

    private PveBossHpBarController bossHpBar;

    private EnemySpawnPoint spawnPoint;

    protected override void Awake() {
        base.Awake();
        enemyParameter = GetComponent<EnemyParameter>();

        healthView = GetComponent<EnemyHealthView>();

        if(healthView == null) {
            bossHpBar = GetComponent<PveBossHpBarController>();
        }
    }

    [Server]
    public void SetSpawnPoint(EnemySpawnPoint sp) {
        spawnPoint = sp;
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

        RpcUpdateEnemyView(enemyParameter.HP, _damage);

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
    private void RpcUpdateEnemyView(int currentHP, int damage) {
        if (healthView != null) {
            healthView.UpdateHP(currentHP);
            healthView.ShowDamage(damage);
        } else if (bossHpBar != null) {
            bossHpBar.UpdateHP(currentHP);
        }
    }

    /// <summary>
    /// 敵の死亡処理
    /// </summary>
    [Server]
    private void Die() {

        enemyParameter.isDead = true;   // 死亡フラグ

        RpcPlayDeathEffect();

        if (spawnPoint != null) {
            EnemySpawnManager.Instance.NotifyEnemyDead(spawnPoint);
        }
        StartCoroutine(DestroyAfterDelay());
    }

    [Server]
    private IEnumerator DestroyAfterDelay() {
        yield return new WaitForSeconds(0.3f); // ダメージ表示時間より少し短め

        NetworkServer.Destroy(gameObject); // サーバーから削除
    }

    /// <summary>
    /// クライアントエフェクト表示
    /// </summary>
    [ClientRpc(includeOwner = true)]
    void RpcPlayDeathEffect() {

        GameObject prefab = EffectPoolRegistry.Instance.GetDeathEffect(EffectType.Explosion);
        if (prefab != null) {
            var fx = EffectPool.Instance.GetFromPool(prefab, transform.position, Quaternion.identity);
            fx.SetActive(true);
            EffectPool.Instance.ReturnToPool(fx, 1.5f);
        }
    }
}