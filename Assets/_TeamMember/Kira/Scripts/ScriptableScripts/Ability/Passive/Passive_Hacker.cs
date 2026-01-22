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




