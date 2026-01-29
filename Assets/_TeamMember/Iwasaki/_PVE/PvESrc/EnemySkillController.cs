using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// 敵スキル管理クラス
/// ・スキルのクールタイム管理
/// ・サーバー上でのみスキル実行を許可
/// </summary>
public class EnemySkillController : NetworkBehaviour {

    [SerializeField]
    private List<EnemySkillData> skills = new();
    // この敵が使用可能なスキル一覧（ScriptableObject）

    private Dictionary<EnemySkillData, float> cooldownTimers = new();
    // 各スキルごとの残りクールタイム管理用

    private EnemyStatus status;
    // 敵のステータス参照（攻撃力など取得用）

    void Awake() {
        // 敵ステータスを取得
        status = GetComponent<EnemyStatus>();

        // すべてのスキルのクールタイムを初期化
        foreach (var skill in skills) {
            cooldownTimers[skill] = 0f;
        }
    }

    void Update() {
        // クールタイムの更新はサーバーのみで行う
        if (!isServer) return;

        // 各スキルのクールタイムを減算
        foreach (var skill in skills) {
            cooldownTimers[skill] -= Time.deltaTime;
        }
    }

    /// <summary>
    /// スキル使用要求（サーバー専用）
    /// ・クールタイムが残っていない場合のみ実行
    /// ・実際の処理内容は EnemySkillData 側に委譲
    /// </summary>
    /// <param name="skill">使用するスキル</param>
    /// <param name="target">攻撃対象（主にプレイヤー）</param>
    [Server]
    public void TryUseSkill(EnemySkillData skill, Transform target) {

        // 管理対象外のスキルは無視
        if (!cooldownTimers.ContainsKey(skill)) return;

        // クールタイム中なら使用不可
        if (cooldownTimers[skill] > 0f) return;

        // スキル実行
        skill.Execute(gameObject, status, target);

        // クールタイムをリセット
        cooldownTimers[skill] = skill.cooldown;
    }
}
