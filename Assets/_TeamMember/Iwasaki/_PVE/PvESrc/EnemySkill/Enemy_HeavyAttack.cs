using UnityEngine;

/// <summary>
/// 敵スキル：高威力攻撃（シンプル版）
/// </summary>
[CreateAssetMenu(
    fileName = "EnemyHeavyAttackSkill",
    menuName = "Enemy/Skill/HeavyAttack"
)]
public class EnemyTestSkill : EnemySkillData {

    [Header("攻撃設定")]
    public float attackRadius = 1.5f; // 攻撃判定半径
    public int damage = 20;           // 高ダメージ

    /// <summary>
    /// スキル実行（即時）
    /// </summary>
    public override void Execute(
        GameObject owner,
        EnemyStatusBase status,
        Transform target
    ) {
        Debug.Log("スキル");
        if (owner == null) return;

        // 攻撃中心（敵の位置）
        Vector3 center = owner.transform.position;

        // 範囲内の当たり判定取得
        Collider[] hits = Physics.OverlapSphere(
            center,
            attackRadius
        );

        foreach (var hit in hits) {

            // プレイヤー判定
            CharacterBase player =
                hit.GetComponent<CharacterBase>();

            if (player == null) continue;

            // ダメージ付与（サーバー）
            player.TakeDamage(
                damage,
                "EnemyHeavyAttack",
                -1
            );
        }
    }
}
