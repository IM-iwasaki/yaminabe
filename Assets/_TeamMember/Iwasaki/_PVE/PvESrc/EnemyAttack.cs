using Mirror;
using UnityEngine;

/// <summary>
/// 敵の攻撃処理（サーバー専用）
/// </summary>
public class EnemyAttack : NetworkBehaviour {

    private EnemyStatus enemyStatus;

    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackInterval = 1.0f;

    private float attackTimer;

    void Awake() {
        enemyStatus = GetComponent<EnemyStatus>();
    }

    void Update() {
        if (!isServer) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0) return;

        TryAttackPlayer();
    }

    /// <summary>
    /// プレイヤー攻撃判定
    /// </summary>
    [Server]
    void TryAttackPlayer() {

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attackRange
        );

        foreach (Collider hit in hits) {

            CharacterBase player = hit.GetComponent<CharacterBase>();
            if (player == null) continue;

            // 敵→プレイヤー方向
            Vector3 dir = (player.transform.position - transform.position).normalized;

            // 前方との角度を取得
            float angle = Vector3.Angle(transform.forward, dir);

            // 例：前方90度以内（左右45度）
            if (angle > 45f) continue;

            player.TakeDamage(
                enemyStatus.GetAttack(),
                "Enemy",
                -1
            );

            attackTimer = attackInterval;
            return;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
#endif
}
