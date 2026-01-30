using Mirror;
using UnityEngine;

/// <summary>
/// 敵の攻撃処理（通常攻撃 or スキルを選択）
/// </summary>
public class EnemyAttack : NetworkBehaviour {

    private EnemyStatus enemyStatus;
    private EnemySkillController skillController;

    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackInterval = 1.0f;

    [Header("スキル発動確率")]
    [Range(0f, 1f)]
    [SerializeField] private float skillUseRate = 0.3f; // 30%でスキル

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
    /// プレイヤーが範囲内にいれば攻撃
    /// </summary>
    [Server]
    void TryAttackPlayer() {

        Collider[] hits = Physics.OverlapSphere(
            transform.position,
            attackRange
        );

        foreach (var hit in hits) {

            CharacterBase player = hit.GetComponent<CharacterBase>();
            if (player == null) continue;

            // 前方判定
            Vector3 dir =
                (player.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > 45f) continue;

            // ---------- 攻撃方法の選択 ----------
            bool usedSkill = false;

            // スキルがあり、確率判定に成功したらスキル
            if (skillController != null &&
                Random.value < skillUseRate) {

                usedSkill =
                    skillController.TryUseAnySkill(player.transform);
            }

            // スキルを使わなかった場合は通常攻撃
            if (!usedSkill) {
                player.TakeDamage(
                    enemyStatus.GetAttack(),
                    "Enemy",
                    -1
                );
            }

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
