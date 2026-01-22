using UnityEngine;

[CreateAssetMenu(menuName = "Character/Passive/Hacker_")]
public class Passive_Hacker : PassiveBase {

    //
    // パッシブ名　：背水の陣【癒】
    // タイプ      ：HP発動型
    // 効果        ：自身の残り体力が20％以下になった時、2秒掛けてHPを30％回復する。
    //               この効果は一度発動すると35秒間は発動しない。
    //

   

    public override void PassiveReflection(CharacterBase user) {
        // デスマッチかどうか
        if (RuleManager.Instance.currentRule == GameRuleType.DeathMatch) {
            return;
        }

        // ルールに関与している敵を取得
        foreach (var enemy in FindObjectsOfType<CharacterBase>()) {
            if (enemy == user) continue;
            if (enemy.parameter.TeamID == user.parameter.TeamID) continue;

            // エリア or ホコに関与しているか
            if (IsEnemyAffectingRule(enemy)) {
                // 敵のスキルCT回復を半減
                enemy.parameter.skillAfterTime -= Time.deltaTime / 2f;

                // 自身のスキルCT回復を3倍
                user.parameter.skillAfterTime += Time.deltaTime * 3f;
            }
        }
    }


    /// <summary>
    /// 敵がルールに関与しているか判定
    /// </summary>
    //private bool IsEnemyAffectingRule(CharacterBase enemy) {
    //    // エリア制圧中
    //    if (enemy.TryGetComponent<CaptureArea>(out var area)) {
    //        if (area.IsCapturing) return true;
    //    }

    //    // ホコ保持中
    //    if (enemy.TryGetComponent<CaptureHoko>(out var hoko)) {
    //        if (hoko.IsHolder) return true;
    //    }

    //    return false;
    //}
}





}
}
