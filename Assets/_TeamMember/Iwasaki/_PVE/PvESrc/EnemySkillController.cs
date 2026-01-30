using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// 敵スキル管理クラス
/// </summary>
public class EnemySkillController : NetworkBehaviour {

    [SerializeField]
    private List<EnemySkillData> skills = new();

    private Dictionary<EnemySkillData, float> cooldownTimers = new();

    private EnemyStatus status;

    void Awake() {
        status = GetComponent<EnemyStatus>();

        foreach (var skill in skills) {
            cooldownTimers[skill] = 0f;
        }
    }

    void Update() {
        if (!isServer) return;

        // クールタイム更新
        foreach (var skill in skills) {
            cooldownTimers[skill] -= Time.deltaTime;
        }
    }

    /// <summary>
    /// 使用可能なスキルをランダムで1つ実行
    /// </summary>
    /// <returns>スキルを使えたか</returns>
    [Server]
    public bool TryUseAnySkill(Transform target) {

        // 使用可能なスキルを抽出
        List<EnemySkillData> usableSkills = new();

        foreach (var skill in skills) {
            if (cooldownTimers[skill] <= 0f) {
                usableSkills.Add(skill);
            }
        }

        // 使えるスキルがなければ失敗
        if (usableSkills.Count == 0)
            return false;

        // ランダムで1つ選択
        EnemySkillData selected =
            usableSkills[Random.Range(0, usableSkills.Count)];

        // 実行
        selected.Execute(gameObject, status, target);

        // クールタイムリセット
        cooldownTimers[selected] = selected.cooldown;

        return true;
    }
}
