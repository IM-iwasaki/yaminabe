using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Character/Passive/Hacker_RuleBreaker")]
public class Passive_Hacker : PassiveBase {

    // CT補正倍率
    private const float ENEMY_CT_RATE = 0.5f; // 半減
    private const float SELF_CT_RATE = 3.0f; // 3倍

    public override void PassiveReflection(CharacterBase user) {

        // ゲーム中でなければ何もしない
        if (!GameManager.Instance.IsGameRunning()) return;

        // デスマッチはここでは扱わない（射撃側で処理）
        if (RuleManager.Instance.currentRule == GameRuleType.DeathMatch)
            return;

        bool enemyAffectingRule = false;

        int myTeam = user.parameter.TeamID;

        // ===== エリア判定 =====
        foreach (var area in Object.FindObjectsOfType<CaptureArea>()) {

            // エリア内に敵がいるか
            foreach (var p in area.playersInArea) {
                if (p == null) continue;
                if (p.parameter.TeamID == myTeam) continue;

                // 敵のCT減速
                p.parameter.skillAfterTime -= Time.deltaTime * ENEMY_CT_RATE;
                enemyAffectingRule = true;
            }
        }

        // ===== ホコ判定 =====
        var stage = StageManager.Instance;
        if (stage != null && stage.currentHoko != null) {

            var holder = stage.currentHoko.holder;
            if (holder != null) {
                var enemy = holder.GetComponent<CharacterBase>();
                if (enemy != null && enemy.parameter.TeamID != myTeam) {

                    // 敵のCT減速
                    enemy.parameter.skillAfterTime -= Time.deltaTime * ENEMY_CT_RATE;
                    enemyAffectingRule = true;
                }
            }
        }

        // ===== 自身のCT加速 =====
        if (enemyAffectingRule) {
            user.parameter.skillAfterTime += Time.deltaTime * SELF_CT_RATE;
        }
    }
}
