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

    [Header("ダメージ乱数設定")]
    [SerializeField] private float randomRange = 0.05f;
    [SerializeField] private float criticalChance = 0.2f;
    [SerializeField] private float criticalMultiplier = 1.5f;

    protected override void Awake() {
        base.Awake();
        enemyParameter = GetComponent<EnemyParameter>();

        healthView = GetComponent<EnemyHealthView>();

        if (healthView == null) {
            bossHpBar = GetComponent<PveBossHpBarController>();
        }
    }

    [Server]
    public void SetSpawnPoint(EnemySpawnPoint sp) {
        spawnPoint = sp;
    }

    public override void OnStartServer() {
        if (statusData == null || enemyParameter == null) return;

        int finalHp = statusData.maxHp;

        // ボスなら人数倍率を適用
        if (spawnPoint != null && spawnPoint.isBossSpawnPoint) {
            int playerCount = NetworkServer.connections.Count;
            playerCount = Mathf.Max(1, playerCount);

            float multiplier = Mathf.Pow(2f, playerCount - 1);

            finalHp = Mathf.RoundToInt(statusData.maxHp * multiplier);
        }

        enemyParameter.HP = finalHp;
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

        // 乱数
        float rand = Random.Range(1f - randomRange, 1f + randomRange);
        damage *= rand;

        bool isCritical = false;

        // 会心
        if (Random.value < criticalChance) {
            damage *= criticalMultiplier;
            isCritical = true;
        }

        if (damage <= 0f) damage = 0.1f;

        damage = Mathf.Round(damage * 10f) / 10f;

        enemyParameter.HP -= Mathf.RoundToInt(damage);

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

        RpcUpdateEnemyView(enemyParameter.HP, damage, isCritical);

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
    private void RpcUpdateEnemyView(int currentHP, float damage, bool isCritical) {
        if (healthView != null) {
            healthView.UpdateHP(currentHP);
            healthView.ShowDamage(damage, isCritical);
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
        if (spawnPoint.isBossSpawnPoint) {
            RpcPlayDeathEffect();
        }

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

            // エフェクトサイズ変更
            fx.transform.localScale *= 4f;

            fx.SetActive(true);
            EffectPool.Instance.ReturnToPool(fx, 1.5f);
        }
    }
}