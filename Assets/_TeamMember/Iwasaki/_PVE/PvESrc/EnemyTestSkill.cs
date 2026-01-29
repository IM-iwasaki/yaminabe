using UnityEngine;
using UnityEngine.AI;
using Mirror;
using System.Collections;

/// <summary>
/// 敵スキル：突進（テスト兼実戦用）
/// </summary>
[CreateAssetMenu(
    fileName = "EnemyTestSkill",
    menuName = "Enemy/Skill/TestSkill"
)]
public class EnemyTestSkill : EnemySkillData {

    [Header("突進設定")]
    public float dashSpeed = 12f;        // 突進速度
    public float dashDuration = 0.5f;    // 突進時間
    public float hitRadius = 0.6f;       // ヒット判定半径
    public int damage = 20;              // ダメージ量

    public override void Execute(
        GameObject owner,
        EnemyStatus status,
        Transform target
    ) {
        if (owner == null || target == null) return;

        // Coroutine を回すための MonoBehaviour
        EnemySkillRunner runner = owner.GetComponent<EnemySkillRunner>();
        if (runner == null) {
            runner = owner.AddComponent<EnemySkillRunner>();
        }

        runner.StartCoroutine(
            DashCoroutine(owner, status, target)
        );
    }

    /// <summary>
    /// 突進処理本体
    /// </summary>
    IEnumerator DashCoroutine(
        GameObject owner,
        EnemyStatus status,
        Transform target
    ) {
        NavMeshAgent agent = owner.GetComponent<NavMeshAgent>();
        if (agent == null) yield break;

        // NavMesh 停止
        agent.isStopped = true;
        agent.updatePosition = false;
        agent.updateRotation = false;

        // 突進方向確定（開始時点）
        Vector3 dir =
            (target.position - owner.transform.position).normalized;

        float timer = 0f;
        bool hitPlayer = false;

        while (timer < dashDuration) {
            timer += Time.deltaTime;

            // 前進
            owner.transform.position +=
                dir * dashSpeed * Time.deltaTime;

            // 当たり判定
            Collider[] hits = Physics.OverlapSphere(
                owner.transform.position,
                hitRadius
            );

            foreach (var hit in hits) {
                CharacterBase player =
                    hit.GetComponent<CharacterBase>();
                if (player == null) continue;

                // ダメージ（サーバー）
                player.TakeDamage(
                    damage,
                    "EnemySkill",
                    -1
                );

                hitPlayer = true;
                break;
            }

            if (hitPlayer) break;

            yield return null;
        }

        // NavMesh に復帰
        agent.Warp(owner.transform.position);
        agent.updatePosition = true;
        agent.updateRotation = true;
        agent.isStopped = false;
    }
}
