using UnityEngine;
using Mirror;
using System.Collections.Generic;

/// <summary>
/// 敵スキル管理（確率＆クールタイム制御）
/// </summary>
public class EnemySkillController : NetworkBehaviour {

    [SerializeField]
    private List<EnemySkillData> skills = new();
    [Header("上から抽選されます")]
    private Dictionary<EnemySkillData, float> cooldownTimers = new();
    private EnemyStatusBase status;

    void Awake() {
        status = GetComponent<EnemyStatusBase>();

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
    /// 確率判定込みでスキル使用を試みる
    /// </summary>
    /// <returns>スキルを使ったか</returns>
    [Server]
    public bool TryUseSkill(Transform target) {

        foreach (var skill in skills) {

            // クールタイム中は不可
            if (cooldownTimers[skill] > 0f) continue;

            // 確率判定
            if (Random.value > skill.useRate) continue;

            // スキル実行
            skill.Execute(gameObject, status, target);

            // クールタイムリセット
            cooldownTimers[skill] = skill.cooldown;

            return true; // 1回使ったら終了
        }

        return false;
    }
}
