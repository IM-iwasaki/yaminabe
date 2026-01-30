using Mirror;
using UnityEngine;

/// <summary>
/// 敵の攻撃処理（通常攻撃 ＋ スキル切り替え）
/// ・サーバー専用
/// ・スキルが設定されていれば優先して使用
/// </summary>
public class EnemyAttack : NetworkBehaviour {

    private EnemyStatus enemyStatus;
    private EnemySkillController skillController;

    [Header("通常攻撃設定")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackInterval = 1.0f;

    [Header("使用スキル（任意）")]
    [SerializeField] private EnemySkillData specialSkill;
    // ここに ScriptableObject のスキルを設定する

    private float attackTimer;

    void Awake() {
        enemyStatus = GetComponent<EnemyStatus>();
        skillController = GetComponent<EnemySkillController>();
    }

    void Update() {
        if (!isServer) return;

        attackTimer -= Time.deltaTime;
        if (attackTimer > 0f) return;

        TryAttackPlayer();
    }

    /// <summary>
    /// プレイヤーを検知して攻撃 or スキル使用
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

            // スキル優先
            if (skillController != null) {
                skillController.TryUseSkill(
                    skillController.GetDefaultSkill(), // 仮
                    player.transform
                );
                attackTimer = attackInterval;
                return;
            }

            // 通常攻撃
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
