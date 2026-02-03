using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 敵スキル：敵版突撃攻撃
/// </summary>
[CreateAssetMenu(
    fileName = "EnemyDashAttack",
    menuName = "Enemy/Skill/DashAttack"
)]
public class EnemyDashAttack : EnemySkillData
{
    [Header("攻撃設定")]
    [SerializeField]private float attackRadius = 1.0f; // 攻撃判定半径
    [SerializeField]private int damage = 30;           // ダメージ                        
    [SerializeField]private float forwardPower = 30.0f;//前に移動する力の強さ

    /// <summary>
    /// スキル実行（即時）
    /// </summary>
    public override void Execute(GameObject owner, EnemyStatusBase status, Transform target) {
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
