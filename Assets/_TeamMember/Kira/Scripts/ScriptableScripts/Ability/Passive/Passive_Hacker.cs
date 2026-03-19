using UnityEngine;
using Mirror;
using Mirror.BouncyCastle.Security;

[CreateAssetMenu(menuName = "Character/Passive/Hacker_RuleBreaker")]
public class Passive_Hacker : PassiveBase {
    // ハッカー用CT補正倍率
    public const float SELF_CT_RATE_DEATHMATCH = 1.2f; // 1.2倍
    public const float SELF_CT_RATE = 2.0f; // 2倍

    public override void PassiveReflection(CharacterBase user) {
        if (!user.isLocalPlayer) return;
        // ゲーム中でなければ何もしない
        if (!GameManager.Instance.IsGameRunning()) return;

        // デスマッチ中のみ発動
        if (RuleManager.Instance.currentRule == GameRuleType.DeathMatch) {
            user.parameter.skillAfterTime += Time.deltaTime * SELF_CT_RATE_DEATHMATCH;
            return;
        }

        bool enemyAffectingRule = false;
        int myTeam = user.parameter.TeamID;

        // ===== エリア判定 =====
        foreach (var area in Object.FindObjectsOfType<CaptureArea>()) {

            // エリア内に敵がいるか
            foreach (var p in area.playersInArea) {
                if (p == null) continue;
                if (p.parameter.TeamID == myTeam) continue;

                // 敵のCT減速
                //p.parameter.skillAfterTime -= Time.deltaTime * ENEMY_CT_RATE;
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
                    //enemy.parameter.skillAfterTime -= Time.deltaTime * ENEMY_CT_RATE;
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